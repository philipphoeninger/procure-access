// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Persistence;

namespace ProcureAccess.Modules.Catalog;

/// <summary>
/// Registration of the Catalog module (ARCHITECTURE §4.3): criteria, revisions, releases, app
/// types, product parts, use-case templates, test types, priority rules and the procedure mapping.
/// Since S3-12 it holds the core entities (releases, criteria, revisions, standard references) with
/// their own database context (ADR-014). The legacy criteria code in API, DAL, MODELS and SERVICES
/// keeps serving the old screens until they are replaced (O-S3-5).
/// </summary>
public static class CatalogModule
{
    /// <summary>Name of the connection string of the application database.</summary>
    private const string ConnectionStringName = "ProcureAccess";

    /// <summary>
    /// Registers the services of the Catalog module. Called once from <c>Program.cs</c>, after the
    /// Platform module, which provides <see cref="TimeProvider"/>.
    /// </summary>
    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Same database and connection string as the other contexts (ADR-014).
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured. " +
                $"Set the environment variable ConnectionStrings__{ConnectionStringName} (see .env.example).");
        }

        services.AddNpgsql<CatalogDbContext>(connectionString, options =>
        {
            options.EnableRetryOnFailure().CommandTimeout(60);
        });
        services.AddScoped<IModuleDatabaseMigrator, CatalogDatabaseMigrator>();

        return services;
    }
}
