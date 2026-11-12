// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.Platform.Contracts.Analytics;
using ProcureAccess.Modules.Platform.Contracts.RateLimiting;

namespace ProcureAccess.Modules.Configurator.Usage;

/// <summary>
/// <c>POST /api/v1/configurator/criteria-list-shown</c> (SPEC §8.7, ARCHITECTURE §5.7; owner
/// decision O-S4-48 (a)): the client reports that S11 shows the list of a new complete selection,
/// and the endpoint writes the usage event <c>CriteriaListShown</c> (R-MET-01, R-CON-10). No body,
/// answer 204; open to everyone, and the dimension is whether the request is signed in.
/// </summary>
/// <remarks>
/// The client decides what a new selection is (O-S4-55 (a)) and sends nothing about it: the event
/// stores only its day and the dimension (PRV-05). The writer only queues, so the answer never
/// waits for the database and never fails because of it. Rate limit (SPEC §7.3, O-S4-56 (b)):
/// policy <see cref="RateLimitPolicies.CriteriaListShown"/>, per client IP, 30 per 10 minutes 🔧,
/// so a script cannot fill the bounded event queue; the client ignores the 429.
/// </remarks>
internal static class CriteriaListShownEndpoint
{
    public const string Path = "/criteria-list-shown";

    public static RouteHandlerBuilder Map(IEndpointRouteBuilder routes) =>
        routes.MapPost(Path, Handle)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.CriteriaListShown)
            .WithName("PostCriteriaListShown")
            .WithSummary("Counts that S11 shows the criteria list of a new complete selection")
            .WithDescription(
                "Writes the usage event CriteriaListShown with the dimension signedIn or anonymous. " +
                "No body; the client calls it once per new selection.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

    private static NoContent Handle(ClaimsPrincipal user, IUsageEventWriter usageEvents)
    {
        usageEvents.CriteriaListShown(signedIn: user.Identity?.IsAuthenticated == true);
        return TypedResults.NoContent();
    }
}
