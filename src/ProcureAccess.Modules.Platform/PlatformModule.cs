// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcureAccess.Modules.Platform.Analytics;
using ProcureAccess.Modules.Platform.Contracts.Persistence;
using ProcureAccess.Modules.Platform.Email;
using ProcureAccess.Modules.Platform.Errors;
using ProcureAccess.Modules.Platform.Health;
using ProcureAccess.Modules.Platform.Logging;
using ProcureAccess.Modules.Platform.Persistence;
using ProcureAccess.Modules.Platform.RateLimiting;

namespace ProcureAccess.Modules.Platform;

/// <summary>
/// Registration of the Platform module (ARCHITECTURE §4.3): the technical services every module
/// may use through <c>Platform.Contracts</c>. Since S2-07 it holds the platform code built ahead
/// in sprint 01: time, logging with retention, usage events and the health endpoints, with their
/// own database context (ADR-014); since S2-10 Problem Details for every error response; since
/// S4-21a the client address behind the proxies and rate limiting; since S5-11 e-mail sending
/// (<see cref="AddPlatformEmail"/>). Share links, file storage, i18n and caching follow with their steps.
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

        // Usage events in the analytics schema, date only (ARCHITECTURE §5.7): queued on the
        // request path, stored by a background service without retrying inserts (#115).
        services.AddUsageEvents(connectionString);

        // /health/live and /health/ready (ARCHITECTURE §9.5); the ready check is limited (#112)
        services.AddPlatformHealthChecks(configuration);

        // Client IP from the trusted proxies and the rate limits of SPEC §7.3 (ARCHITECTURE §9.2)
        services.AddClientAddress(configuration);
        services.AddPlatformRateLimiting(configuration);

        // Errors as RFC 9457 Problem Details with the problem types of SPEC §8.6 (API-01)
        services.AddPlatformProblemDetails(configuration);

        return services;
    }

    /// <summary>
    /// Registers <c>IEmailSender</c> (ARCHITECTURE C71, SPEC §7.6): MailKit over SMTP with TLS, settings
    /// from <c>Email:*</c>. Only the web host calls it — the <c>catalog</c> and <c>roles</c> command
    /// hosts send no e-mail and need no SMTP settings. Outside Development and E2E the start fails
    /// without credentials or with an unencrypted connection (owner decision S5-11 4a).
    /// </summary>
    public static IServiceCollection AddPlatformEmail(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment) =>
        services.AddEmailSender(configuration, environment);

    /// <summary>
    /// Adds the middleware of the Platform module: the client address from the trusted proxies,
    /// then Problem Details for unhandled exceptions and for error responses without a body. Call
    /// it first, before any other middleware of the pipeline.
    /// </summary>
    public static IApplicationBuilder UsePlatformMiddleware(this IApplicationBuilder app) =>
        app.UseClientAddress()
           .UsePlatformProblemDetails();

    /// <summary>
    /// Adds the rate limiter (SPEC §7.3). Call it after CORS, so a 429 carries the CORS headers a
    /// browser on another origin needs to read it, and after authentication.
    /// </summary>
    public static IApplicationBuilder UsePlatformRateLimiting(this IApplicationBuilder app) =>
        app.UseRateLimiter();

    /// <summary>Maps the endpoints of the Platform module: <c>/health/live</c> and <c>/health/ready</c>.</summary>
    public static IEndpointRouteBuilder MapPlatformEndpoints(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPlatformHealthChecks();
}
