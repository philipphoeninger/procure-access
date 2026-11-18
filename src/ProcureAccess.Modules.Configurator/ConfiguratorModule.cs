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
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Configurator.Contracts.Drafts;
using ProcureAccess.Modules.Configurator.Drafts;
using ProcureAccess.Modules.Configurator.Persistence;
using ProcureAccess.Modules.Configurator.Settings;
using ProcureAccess.Modules.Configurator.Sharing;
using ProcureAccess.Modules.Configurator.Usage;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.Platform.Contracts.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Sharing;

namespace ProcureAccess.Modules.Configurator;

/// <summary>
/// Registration of the Configurator module (ARCHITECTURE §4.3): saved criteria configurations
/// (organisation lists and project copies), user overrides and copies to a newer release.
/// Since S4-18b it maps <c>POST /api/v1/configurator/criteria-list-shown</c>, the usage event of
/// S11 (<see cref="MapConfiguratorEndpoints"/>). Since S5-19 it owns the saved lists
/// (<see cref="ConfiguratorDbContext"/>) and answers <see cref="IConfigurationReader"/> and
/// <see cref="ICriteriaListDataExport"/>. Since S5-20a it maps the saved-list endpoints below
/// <c>/configurations</c> (<see cref="ConfigurationEndpoints"/>: save, read, list) and answers
/// <see cref="IConfigurationDraftCheck"/>, the draft check of the anonymous export and of saving.
/// Since S5-22 it serves its 🔧 weight range at <c>GET /configurator/settings</c>
/// (<see cref="ConfiguratorSettingsEndpoint"/>). Since S5-23 it maps the share links of a list
/// (<see cref="ShareLinkEndpoints"/>) and reads a shared list for Platform
/// (<see cref="ConfigurationShareProvider"/>). Since S6-12 it stores the copy a procurement project
/// owns and answers <see cref="IProjectCriteriaLists"/> for Workspace.
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

        // The copy a procurement project owns (C53, S6-12): Workspace assigns and reads it through it.
        services.AddScoped<IProjectCriteriaLists, ProjectCriteriaLists>();
        services.AddScoped<ICriteriaListDataExport, CriteriaListDataExportService>();
        services.AddScoped<IOrganizationDataDeletion, CriteriaListDeletionService>();

        // Read once here, so an impossible 🔧 weight range stops the start (SPEC SD23).
        services.AddSingleton(PriorityWeightRange.From(configuration));
        services.AddScoped<IConfigurationDraftCheck, ConfigurationDraftCheck>();
        services.AddScoped<ConfigurationService>();

        // The saved lists' side of a share link (S5-23, C59): Platform reads a shared list through it.
        services.AddScoped<IShareableResourceProvider, ConfigurationShareProvider>();
        return services;
    }

    /// <summary>
    /// Maps the configurator endpoints as minimal APIs below <c>/configurator</c> (the usage event, the settings)
    /// and <c>/configurations</c> (the saved lists, ARCHITECTURE §7.2) of <paramref name="routes"/>,
    /// the host's versioned group <c>/api/v{version}</c>, as <c>MapCatalogEndpoints</c> does. Map it
    /// before the Swagger set-up (#176).
    /// </summary>
    public static IEndpointRouteBuilder MapConfiguratorEndpoints(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);
        var configurator = routes.MapGroup("/configurator").WithTags("Configurator");
        CriteriaListShownEndpoint.Map(configurator);
        ConfiguratorSettingsEndpoint.Map(configurator);
        ConfigurationEndpoints.Map(routes);
        ShareLinkEndpoints.Map(routes);
        return routes;
    }
}
