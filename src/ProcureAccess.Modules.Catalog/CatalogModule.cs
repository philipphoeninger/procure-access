// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog.Commands;
using ProcureAccess.Modules.Catalog.Criteria;
using ProcureAccess.Modules.Catalog.FilterOptions;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.Catalog.Reading;
using ProcureAccess.Modules.Platform.Contracts.Persistence;

namespace ProcureAccess.Modules.Catalog;

/// <summary>
/// Registration of the Catalog module (ARCHITECTURE §4.3): criteria, revisions, releases, app
/// types, product parts, use-case templates, test types, priority rules and the procedure mapping.
/// Since S3-12 it holds the core entities (releases, criteria, revisions, standard references) with
/// their own database context (ADR-014); since S3-16 the command <c>catalog import</c>, since S3-17
/// <c>catalog activate</c> and the cached catalog reads (<see cref="CatalogReader"/>), since S3-19
/// the endpoint <c>GET /api/v1/catalog/filter-options</c> (<see cref="MapCatalogEndpoints"/>). The legacy
/// criteria code in API, DAL, MODELS and SERVICES keeps serving the old screens until they are
/// replaced (O-S3-5).
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

        // Catalog reads, cached by release (ARCHITECTURE §9.6, D10; CAT-09, S3-17).
        services.AddMemoryCache();
        services.AddScoped<CatalogReader>();

        return services;
    }

    /// <summary>
    /// Maps the catalog endpoints (ARCHITECTURE §7.2) as minimal APIs below <c>/catalog</c> of
    /// <paramref name="routes"/> (owner decision O-S3-6). The host hands in its versioned group
    /// <c>/api/v{version}</c>, so the module needs no versioning of its own (#153, #176). Map it before
    /// the Swagger set-up, which lists the API versions of the endpoints mapped so far.
    /// </summary>
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);
        var catalog = routes.MapGroup("/catalog").WithTags("Catalog");
        FilterOptionsEndpoint.Map(catalog);
        CriteriaEndpoint.Map(catalog);
        return routes;
    }

    /// <summary>
    /// True when the host was started with a catalog command (<c>catalog import &lt;version&gt;</c>,
    /// <c>catalog activate &lt;version&gt;</c>) instead of as the API (owner, S3-16 decision 1).
    /// </summary>
    public static bool IsCommand(IReadOnlyList<string> args) =>
        args is [CatalogCommandLine.CommandName, ..];

    /// <summary>
    /// Runs the catalog command in <paramref name="args"/> on <paramref name="services"/>, which
    /// must hold the Platform and Catalog modules and the configuration. Returns the exit code:
    /// 0 done, 1 refused (nothing written), 2 wrong usage.
    /// </summary>
    public static Task<int> RunCommandAsync(
        IServiceProvider services,
        IReadOnlyList<string> args,
        TextWriter output,
        TextWriter error,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);
        return CatalogCommandLine.RunAsync(services, args, output, error, cancellationToken);
    }
}
