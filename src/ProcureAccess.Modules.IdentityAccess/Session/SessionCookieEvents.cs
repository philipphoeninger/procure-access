// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace ProcureAccess.Modules.IdentityAccess.Session;

/// <summary>
/// Events of the Identity application cookie (ARCHITECTURE §9.1, C60; SEC-01, SEC-03). Resolved
/// from the request services (<c>EventsType</c>), so the clock is the API's <see cref="TimeProvider"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>An API never redirects: a request without a valid session gets 401, a signed-in user
/// without the right 403, both as Problem Details through the Platform status-code pages.</item>
/// <item>The sign-in time is stored in the ticket (<see cref="SignedInUtcKey"/>), because the
/// cookie handler rewrites <c>IssuedUtc</c> on every renewal. A ticket past
/// <see cref="SessionSettings.AbsoluteTimeout"/> is rejected and the cookie deleted, after the
/// security-stamp validator ran, which the default events of <c>AddIdentityCookies</c> wire.</item>
/// <item>The session probe never renews the sliding expiry (C60), so a shell that only asks how
/// long the session still lasts does not keep it alive. Every other authenticated request renews
/// it when the handler asks (its default: after half of the idle window); S5-29 renews on each one.</item>
/// </list>
/// </remarks>
internal sealed class SessionCookieEvents(TimeProvider clock, SessionSettings settings) : CookieAuthenticationEvents
{
    /// <summary>Key of the sign-in time in <see cref="AuthenticationProperties.Items"/> (UTC, round-trip format).</summary>
    public const string SignedInUtcKey = "pa:signedInUtc";

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }

    public override Task SigningIn(CookieSigningInContext context)
    {
        // Only a fresh sign-in sets the time; a renewal re-signs with the existing items.
        context.Properties.Items.TryAdd(SignedInUtcKey, clock.GetUtcNow().ToString("O", CultureInfo.InvariantCulture));
        return base.SigningIn(context);
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        await SecurityStampValidator.ValidatePrincipalAsync(context);
        if (context.Principal is null)
        {
            return;
        }

        var absoluteExpiresAt = AbsoluteExpiry(context.Properties, settings);
        if (absoluteExpiresAt is null || clock.GetUtcNow() < absoluteExpiresAt)
        {
            return;
        }

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(context.Scheme.Name);
    }

    public override Task CheckSlidingExpiration(CookieSlidingExpirationContext context)
    {
        if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<SessionProbeMetadata>() is not null)
        {
            context.ShouldRenew = false;
        }

        return Task.CompletedTask;
    }

    /// <summary>The absolute limit of a ticket, or null for a ticket without a stored sign-in time.</summary>
    public static DateTimeOffset? AbsoluteExpiry(AuthenticationProperties properties, SessionSettings settings) =>
        properties.Items.TryGetValue(SignedInUtcKey, out var signedIn)
        && DateTimeOffset.TryParseExact(signedIn, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var signedInUtc)
            ? signedInUtc + settings.AbsoluteTimeout
            : null;
}

/// <summary>Marks the session probe, which must not renew the session (<see cref="SessionCookieEvents"/>).</summary>
internal sealed class SessionProbeMetadata;
