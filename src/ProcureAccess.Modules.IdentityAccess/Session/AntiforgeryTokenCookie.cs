// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ProcureAccess.Modules.IdentityAccess.Session;

/// <summary>
/// The antiforgery token as the client reads it (ARCHITECTURE §8.3, §9.1; SEC-02, SEC-09).
/// ASP.NET's antiforgery keeps its half in an <c>HttpOnly</c> cookie of its own; the request
/// token goes into the readable cookie <see cref="CookieName"/>, which the client sends back as
/// the header <see cref="HeaderName"/> on every state-changing request (Angular's XSRF support
/// does exactly this). The token is bound to the signed-in user, so the probe, sign-in and
/// sign-out each issue a fresh one.
/// </summary>
internal static class AntiforgeryTokenCookie
{
    public const string CookieName = "XSRF-TOKEN";
    public const string HeaderName = "X-XSRF-TOKEN";

    /// <summary>Issues the token for the user of <paramref name="context"/> (<c>HttpContext.User</c>).</summary>
    public static void Issue(HttpContext context)
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append(CookieName, tokens.RequestToken!, new CookieOptions
        {
            // Readable by the client script, otherwise it could not copy it into the header.
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            IsEssential = true,
        });
    }
}
