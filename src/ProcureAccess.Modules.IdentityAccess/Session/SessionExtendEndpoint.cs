// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ProcureAccess.Modules.IdentityAccess.Session;

/// <summary>
/// <c>POST /api/v1/auth/session/extend</c>, „Angemeldet bleiben“ of the expiry warning
/// (ARCHITECTURE §7.1, §8.3, C52, C60; SPEC R-AUTH-06; SEC-07): renews the idle limit and answers
/// 204 with the two limits in the expiry headers. The absolute limit and the sign-in time stay.
/// </summary>
/// <remarks>
/// The handler does nothing: every authenticated request except the probe renews the idle limit
/// (<see cref="SessionCookieEvents.ValidatePrincipal"/>), and this is one the shell can send
/// without another effect. State-changing by its method, so the antiforgery token is required
/// (SEC-02); no rate limit (SPEC §7.3 names none).
/// </remarks>
internal static class SessionExtendEndpoint
{
    public const string Path = "/session/extend";

    public static RouteHandlerBuilder Map(IEndpointRouteBuilder routes) =>
        routes.MapPost(Path, Handle)
            .RequireAuthorization()
            .WithName("PostSessionExtend")
            .WithSummary("Renews the idle limit of the session")
            .WithDescription(
                "204 with X-Session-Expires-At and X-Session-Absolute-Expires-At. The idle limit never passes the " +
                "absolute limit, which stays. 401 without a session; 400 antiforgery without the token.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

    private static NoContent Handle() => TypedResults.NoContent();
}
