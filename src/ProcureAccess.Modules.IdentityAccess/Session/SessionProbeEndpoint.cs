// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace ProcureAccess.Modules.IdentityAccess.Session;

/// <summary>
/// <c>GET /api/v1/auth/session</c>, the session probe (ARCHITECTURE §7.1, §8.3, C60): always 200,
/// <c>{ user: null }</c> for an anonymous request, otherwise the signed-in user with the two
/// expiry times of the session. Every answer (re)issues the antiforgery token cookie, so the
/// shell calls it once at start and a sign-in, registration or password reset carries a valid
/// token. It reads the principal only: it never signs in, never renews the session
/// (<see cref="SessionProbeMetadata"/>), never touches the database.
/// </summary>
/// <remarks>
/// <c>expiresAt</c> is the idle limit of the cookie ticket, <c>absoluteExpiresAt</c> the stored
/// sign-in time plus the absolute limit; both are left out for a sign-in without session limits
/// (the legacy JWT until S5-08, the test scheme of the integration host). The response is never
/// cached: issuing the token sets <c>Cache-Control: no-cache, no-store</c> (TEN-04).
/// </remarks>
internal static class SessionProbeEndpoint
{
    public const string Path = "/session";

    public static RouteHandlerBuilder Map(IEndpointRouteBuilder routes) =>
        routes.MapGet(Path, Handle)
            .AllowAnonymous()
            .WithMetadata(new SessionProbeMetadata())
            .WithName("GetSession")
            .WithSummary("Answers who is signed in and issues the antiforgery token cookie")
            .WithDescription(
                "Always 200: { user: null } when anonymous, else the user with the expiry times of the session. " +
                "Sets the readable cookie XSRF-TOKEN, whose value state-changing requests send as X-XSRF-TOKEN. " +
                "Never renews the session.")
            .Produces<SessionResponse>();

    private static Ok<SessionResponse> Handle(HttpContext context, ClaimsPrincipal user, SessionSettings settings)
    {
        // Issuing the token also sets Cache-Control: no-cache, no-store (TEN-04).
        AntiforgeryTokenCookie.Issue(context);

        if (user.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Ok(SessionResponse.Anonymous);
        }

        var properties = context.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Properties;
        return TypedResults.Ok(new SessionResponse(
            new SessionUser(
                user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue(ClaimTypes.Name) ?? "",
                [.. user.FindAll(ClaimTypes.Role).Select(c => c.Value)]),
            properties?.ExpiresUtc,
            properties is null ? null : SessionCookieEvents.AbsoluteExpiry(properties, settings)));
    }
}

/// <summary>Answer of the session probe; see <see cref="SessionProbeEndpoint"/>.</summary>
internal sealed record SessionResponse(
    SessionUser? User,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DateTimeOffset? ExpiresAt,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DateTimeOffset? AbsoluteExpiresAt)
{
    public static readonly SessionResponse Anonymous = new(null, null, null);
}

/// <summary>The signed-in user as the probe reports it: id, e-mail address and role names from the claims.</summary>
internal sealed record SessionUser(string Id, string Email, IReadOnlyList<string> Roles);
