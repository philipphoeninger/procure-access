// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Contracts.Persistence;
using ProcureAccess.Modules.Workspace.Persistence;
using ProcureAccess.Modules.Workspace.Projects;

namespace ProcureAccess.Modules.Workspace;

/// <summary>
/// Registration of the Workspace module (ARCHITECTURE §4.3): the organisations' procurement
/// projects, candidate products, test evidences and private assessments. Since S6-11 it owns the
/// projects (<see cref="WorkspaceDbContext"/>) and maps their endpoints
/// (<see cref="ProjectEndpoints"/>); since S6-12 a project gets its own copy of a list
/// (<see cref="ProjectCriteriaListEndpoint"/>, stored by Configurator); the candidates and the
/// assessments follow with their tasks of migration step 5. Until their step retires them, products and
/// favorites are legacy (the proposals went with S6-23).
/// </summary>
public static class WorkspaceModule
{
    private const string ConnectionStringName = "ProcureAccess";

    /// <summary>
    /// Registers the services of the Workspace module. Called once from <c>Program.cs</c>, after
    /// the IdentityAccess module (organisation context, role check).
    /// </summary>
    public static IServiceCollection AddWorkspaceModule(this IServiceCollection services, IConfiguration configuration)
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

        services.AddNpgsql<WorkspaceDbContext>(connectionString, options => options.EnableRetryOnFailure().CommandTimeout(60));
        services.AddScoped<IModuleDatabaseMigrator, WorkspaceDatabaseMigrator>();
        services.AddScoped<ProjectService>();

        // The project's own copy of a list (S6-12, C53). The lock answers „not locked“ until S6-18
        // replaces this registration by the check on its assessment rows (C47).
        services.AddScoped<ProjectCriteriaListService>();
        services.AddScoped<ICriteriaListLock, NoAssessmentsYet>();
        return services;
    }

    /// <summary>
    /// Maps the workspace endpoints as minimal APIs below <c>/organizations/{organizationId}/projects</c>
    /// and <c>/projects</c> (ARCHITECTURE §7.2) of <paramref name="routes"/>, the host's versioned
    /// group <c>/api/v{version}</c>. Map it before the Swagger set-up (#176).
    /// </summary>
    public static IEndpointRouteBuilder MapWorkspaceEndpoints(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);
        ProjectEndpoints.Map(routes);
        ProjectCriteriaListEndpoint.Map(routes);
        return routes;
    }
}
