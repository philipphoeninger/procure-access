// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.Configurator.Drafts;

namespace ProcureAccess.Modules.Configurator.Settings;

/// <summary>The 🔧 range of a user weight, as <c>Configurator:PriorityWeight</c> sets it.</summary>
/// <param name="Min">Smallest weight a B criterion may take.</param>
/// <param name="Max">Largest weight a B criterion may take.</param>
internal sealed record WeightRangeResponse(int Min, int Max);

/// <summary>The settings the configurator client takes from the server (SPEC §8.11).</summary>
/// <param name="PriorityWeight">The weight range of R-LST-03 (SPEC SD23).</param>
internal sealed record ConfiguratorSettingsResponse(WeightRangeResponse PriorityWeight);

/// <summary>
/// <c>GET /api/v1/configurator/settings</c> (SPEC §8.11; S5-22, TD-45, owner decision S5-22 1a):
/// the server's 🔧 weight range, so the client no longer keeps its own copy. Open to everyone,
/// because the anonymous draft of S11 needs the range as much as a saved list; no rate limit, like
/// the catalog reads. The answer carries the platform default <c>Cache-Control: no-store</c>
/// (TEN-04), so a changed setting reaches the next page load.
/// </summary>
internal static class ConfiguratorSettingsEndpoint
{
    public const string Path = "/settings";

    public static RouteHandlerBuilder Map(IEndpointRouteBuilder routes) =>
        routes.MapGet(Path, Handle)
            .AllowAnonymous()
            .WithName("GetConfiguratorSettings")
            .WithSummary("Reads the settings of the configurator: the range of a user weight")
            .WithDescription(
                "Answers the range a user weight of a B criterion may take (Configurator:PriorityWeight). " +
                "Open to everyone; the client loads it before S11 or a saved list shows.")
            .Produces<ConfiguratorSettingsResponse>(StatusCodes.Status200OK);

    private static Ok<ConfiguratorSettingsResponse> Handle(PriorityWeightRange weights) =>
        TypedResults.Ok(new ConfiguratorSettingsResponse(new WeightRangeResponse(weights.Min, weights.Max)));
}
