// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Configurator.Usage;

namespace ProcureAccess.Modules.Configurator;

/// <summary>
/// Registration of the Configurator module (ARCHITECTURE §4.3): saved criteria configurations
/// (organisation lists and project copies), user overrides and copies to a newer release.
/// It registers no services yet; saved lists arrive with migration step 3. Since S4-18b it maps
/// <c>POST /api/v1/configurator/criteria-list-shown</c>, the usage event of S11
/// (<see cref="MapConfiguratorEndpoints"/>).
/// </summary>
public static class ConfiguratorModule
{
    /// <summary>Registers the services of the Configurator module. Called once from <c>Program.cs</c>.</summary>
    public static IServiceCollection AddConfiguratorModule(this IServiceCollection services) => services;

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
