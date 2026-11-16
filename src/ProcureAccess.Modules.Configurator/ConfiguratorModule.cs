// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Configurator.Accounts;
using ProcureAccess.Modules.Configurator.Configurations;
using ProcureAccess.Modules.Configurator.Contracts.Accounts;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Configurator.Persistence;
using ProcureAccess.Modules.Configurator.Usage;
using ProcureAccess.Modules.Platform.Contracts.Persistence;

namespace ProcureAccess.Modules.Configurator;

/// <summary>
/// Registration of the Configurator module (ARCHITECTURE §4.3): saved criteria configurations
/// (organisation lists and project copies), user overrides and copies to a newer release.
/// Since S4-18b it maps <c>POST /api/v1/configurator/criteria-list-shown</c>, the usage event of
/// S11 (<see cref="MapConfiguratorEndpoints"/>). Since S5-19 it owns the saved lists
/// (<see cref="ConfiguratorDbContext"/>) and answers <see cref="IConfigurationReader"/> and
/// <see cref="ICriteriaListDataExport"/>; the endpoints of the saved lists follow with S5-20.
/// </summary>
public static class ConfiguratorModule
{
    private const string ConnectionStringName = "ProcureAccess";

    /// <summary>
    /// Registers the services of the Configurator module. Called once from <c>Program.cs</c>, after
    /// the IdentityAccess module (organisation context, <c>IOrganizationMembers</c>) and the Catalog
    /// module (<c>ICatalogSelectionResolver</c>).
    /// </summary>
    public static IServiceCollection AddConfiguratorModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // Same database and connection string as the other contexts (ADR-014).
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured. " +
                $"Set the environment variable ConnectionStrings__{ConnectionStringName} (see .env.example).");
        }

        services.AddNpgsql<ConfiguratorDbContext>(connectionString, options =>
        {
            // A list is loaded with its four row tables: one query per table, so the collections
            // do not multiply each other's rows.
            options.EnableRetryOnFailure().CommandTimeout(60).UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
        services.AddScoped<IModuleDatabaseMigrator, ConfiguratorDatabaseMigrator>();
        services.AddScoped<IConfigurationReader, ConfigurationReader>();
        services.AddScoped<ICriteriaListDataExport, CriteriaListDataExportService>();
        return services;
    }

    /// <summary>
    /// Maps the configurator endpoints as minimal APIs below <c>/configurator</c> of
    /// <paramref name="routes"/>, the host's versioned group <c>/api/v{version}</c>, as
    /// <c>MapCatalogEndpoints</c> does. Map it before the Swagger set-up (#176).
    /// </summary>
    public static IEndpointRouteBuilder MapConfiguratorEndpoints(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);
        var configurator = routes.MapGroup("/configurator").WithTags("Configurator");
        CriteriaListShownEndpoint.Map(configurator);
        return routes;
    }
}
