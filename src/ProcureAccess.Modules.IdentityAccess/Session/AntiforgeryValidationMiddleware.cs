// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
/// <para>Until S5-10 the anonymous legacy identity endpoints (<c>signUp</c>, <c>forgot-password</c>,
/// <c>reset-password</c>) are exempt by metadata. The skip for a Bearer header went with the JWT
/// (S5-08): every request is validated by its endpoint alone.</para>
/// <para>Runs after authorization and the rate limiter, so a forged request still counts against
/// its limit and an anonymous request to a protected endpoint stays a 401.</para>
/// </remarks>
internal sealed class AntiforgeryValidationMiddleware(RequestDelegate next, IAntiforgery antiforgery)
{

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

        return endpoint.Metadata.GetMetadata<IAntiforgeryMetadata>() is not { RequiresValidation: false };
    }

    private static bool IsSafeMethod(string method) =>
        HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method);
}
