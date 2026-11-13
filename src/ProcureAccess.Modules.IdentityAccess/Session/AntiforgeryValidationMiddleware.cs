// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace ProcureAccess.Modules.IdentityAccess.Session;

/// <summary>
/// Antiforgery on every state-changing endpoint (ARCHITECTURE §9.1; SEC-02, SEC-09): a request
/// whose method is not GET, HEAD, OPTIONS or TRACE must carry the token of
/// <see cref="AntiforgeryTokenCookie"/>, or it is answered 400 Problem Details of type
/// <see cref="ProblemTypes.Antiforgery"/> before the endpoint runs.
/// </summary>
/// <remarks>
/// <para>Opt-out, not opt-in like ASP.NET's own <c>UseAntiforgery</c>: an endpoint is exempt only
/// with <c>DisableAntiforgery()</c> (<see cref="IAntiforgeryMetadata.RequiresValidation"/> false),
/// which the anonymous export carries, the only exemption the architecture names. The test
/// <c>INV:SEC-02</c> lists every exempt endpoint, so the list can only shrink knowingly.</para>
/// <para>Legacy until S5-08 removes the JWT: a request that authenticates with a Bearer token is
/// not validated. A bearer token is no ambient credential — the browser never attaches it on its
/// own, so a cross-site request cannot ride on it — and the legacy client has no token cookie
/// yet. Also until S5-10: the anonymous legacy identity endpoints are exempt by metadata.</para>
/// <para>Runs after authorization and the rate limiter, so a forged request still counts against
/// its limit and an anonymous request to a protected endpoint stays a 401.</para>
/// </remarks>
internal sealed class AntiforgeryValidationMiddleware(RequestDelegate next, IAntiforgery antiforgery)
{
    private const string BearerPrefix = "Bearer ";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!RequiresValidation(context))
        {
            await next(context);
            return;
        }

        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            await Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                type: ProblemTypes.Antiforgery,
                detail: $"The request needs the antiforgery token: send the value of the cookie {AntiforgeryTokenCookie.CookieName} " +
                        $"as the header {AntiforgeryTokenCookie.HeaderName}. GET /api/v1/auth/session issues it.")
                .ExecuteAsync(context);
            return;
        }

        await next(context);
    }

    internal static bool RequiresValidation(HttpContext context)
    {
        if (IsSafeMethod(context.Request.Method))
        {
            return false;
        }

        // No endpoint: nothing to protect, the request ends as 404 (not as a token failure).
        if (context.GetEndpoint() is not { } endpoint)
        {
            return false;
        }

        if (endpoint.Metadata.GetMetadata<IAntiforgeryMetadata>() is { RequiresValidation: false })
        {
            return false;
        }

        return !HasBearerToken(context);
    }

    private static bool IsSafeMethod(string method) =>
        HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method);

    /// <summary>Legacy JWT (removed with S5-08): see the remarks of the class.</summary>
    private static bool HasBearerToken(HttpContext context) =>
        context.Request.Headers[HeaderNames.Authorization].ToString().StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase);
}
