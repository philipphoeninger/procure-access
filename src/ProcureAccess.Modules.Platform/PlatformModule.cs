// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Analytics;
using ProcureAccess.Modules.Platform.Contracts.Analytics;
using ProcureAccess.Modules.Platform.Contracts.Persistence;
using ProcureAccess.Modules.Platform.Health;
using ProcureAccess.Modules.Platform.Logging;
using ProcureAccess.Modules.Platform.Persistence;

namespace ProcureAccess.Modules.Platform;

/// <summary>
/// Registration of the Platform module (ARCHITECTURE §4.3): the technical services every module
/// may use through <c>Platform.Contracts</c>. Since S2-07 it holds the platform code built ahead
/// in sprint 01: time, logging with retention, usage events and the health endpoints, with their
/// own database context (ADR-014). Problem Details follows with S2-10; share links, file storage,
/// e-mail sending, rate limiting, i18n and caching with their steps.
/// </summary>
public static class PlatformModule
{
    /// <summary>Name of the connection string of the application database.</summary>
    private const string ConnectionStringName = "ProcureAccess";

    /// <summary>
    /// Registers the services of the Platform module. Called once from <c>Program.cs</c>, before
    /// the other modules. Replaces the default logging providers with Serilog.
    /// </summary>
    public static IServiceCollection AddPlatformModule(this IServiceCollection services, IConfiguration configuration)
    {
        // The application reads time only through TimeProvider (CLAUDE.md §7); tests replace it
        // with FakeTimeProvider.
        services.AddSingleton(TimeProvider.System);

        // Console + PostgreSQL sink in the logging schema (ARCHITECTURE §9.5)
        services.AddPlatformLogging(configuration);

        // Set via the environment variable ConnectionStrings__ProcureAccess (see .env.example).
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured. " +
                $"Set the environment variable ConnectionStrings__{ConnectionStringName} (see .env.example).");
        }

        services.AddNpgsql<PlatformDbContext>(connectionString, options =>
        {
            options.EnableRetryOnFailure().CommandTimeout(60);
        });
        services.AddScoped<IModuleDatabaseMigrator, PlatformDatabaseMigrator>();

        // Log retention in the logging schema (OPS-02)
        services.AddLogRetention();

        // Usage events in the analytics schema, date only (ARCHITECTURE §5.7). Singleton: the
        // writer keeps no state and opens a scope of its own for every event.
        services.AddSingleton<IUsageEventWriter, UsageEventWriter>();

        // /health/live and /health/ready (ARCHITECTURE §9.5)
        services.AddPlatformHealthChecks();

        return services;
    }

    /// <summary>Maps the endpoints of the Platform module: <c>/health/live</c> and <c>/health/ready</c>.</summary>
    public static IEndpointRouteBuilder MapPlatformEndpoints(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPlatformHealthChecks();
}
