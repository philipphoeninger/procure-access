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
/// Events of the Identity application cookie (ARCHITECTURE §9.1, C60; SEC-01, SEC-03, SEC-07,
/// SEC-08). Resolved from the request services (<c>EventsType</c>), so the clock is the API's
/// <see cref="TimeProvider"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>An API never redirects: a request without a valid session gets 401, a signed-in user
/// without the right 403, both as Problem Details through the Platform status-code pages.</item>
/// <item>The sign-in time is stored in the ticket (<see cref="SignedInUtcKey"/>), because the
/// cookie handler rewrites <c>IssuedUtc</c> on every renewal. A ticket past
/// <see cref="SessionSettings.AbsoluteTimeout"/> is rejected and the cookie deleted. The
/// security-stamp validator then ends a session whose user reset the password or deleted the
/// account since (SEC-08; 🔧 <see cref="SessionSettings.SecurityStampValidationInterval"/>).</item>
/// <item>Every authenticated request renews the idle limit to now plus
/// <see cref="SessionSettings.IdleTimeout"/>, never past the absolute limit (SEC-07). The handler's
/// own rule (renew after half of the window) would let the idle limit vary between 15 and 30
/// minutes. The session probe never renews, so a shell that only asks how long the session still
/// lasts does not keep it alive.</item>
/// <item>Every answer to an authenticated request carries the two limits in
/// <see cref="ExpiresAtHeader"/> and <see cref="AbsoluteExpiresAtHeader"/>, also the answer of a
/// sign-in; a sign-out takes them away.</item>
/// </list>
/// </remarks>
internal sealed class SessionCookieEvents(TimeProvider clock, SessionSettings settings) : CookieAuthenticationEvents
{
    /// <summary>Key of the sign-in time in <see cref="AuthenticationProperties.Items"/> (UTC, round-trip format).</summary>
    public const string SignedInUtcKey = "pa:signedInUtc";

    /// <summary>Response header with the idle limit of the session (ISO 8601 UTC, whole seconds).</summary>
    public const string ExpiresAtHeader = "X-Session-Expires-At";

    /// <summary>Response header with the absolute limit of the session (ISO 8601 UTC, whole seconds).</summary>
    public const string AbsoluteExpiresAtHeader = "X-Session-Absolute-Expires-At";

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
        WriteExpiryHeaders(context.Response, context.Properties);
        return base.SigningIn(context);
    }

    public override Task SigningOut(CookieSigningOutContext context)
    {
        context.Response.Headers.Remove(ExpiresAtHeader);
        context.Response.Headers.Remove(AbsoluteExpiresAtHeader);
        return base.SigningOut(context);
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        // A session ends when a limit is reached. The cookie handler itself still accepts a ticket
        // at the instant of its expiry, which the client was told as the end.
        var now = clock.GetUtcNow();
        var absoluteExpiresAt = AbsoluteExpiry(context.Properties, settings);
        if (now >= context.Properties.ExpiresUtc || now >= absoluteExpiresAt)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(context.Scheme.Name);
            return;
        }

        // Before the renewal below, which rewrites the issue time the validator reads. It signs
        // out itself when the stamp no longer matches.
        await SecurityStampValidator.ValidatePrincipalAsync(context);
        if (context.Principal is null)
        {
            return;
        }

        if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<SessionProbeMetadata>() is not null)
        {
            // The security-stamp validator asks for a renewal whenever it checked.
            context.ShouldRenew = false;
        }
        else
        {
            // The handler renews by the ticket's own length (expiry minus issue time), so both are set.
            var expiresAt = now + settings.IdleTimeout;
            context.Properties.IssuedUtc = now;
            context.Properties.ExpiresUtc = absoluteExpiresAt < expiresAt ? absoluteExpiresAt : expiresAt;
            context.ShouldRenew = true;
        }

        WriteExpiryHeaders(context.Response, context.Properties);
    }

    /// <summary>
    /// Never by the handler's rule: <see cref="ValidatePrincipal"/> decides the renewal, because
    /// only there the new expiry can be capped at the absolute limit.
    /// </summary>
    public override Task CheckSlidingExpiration(CookieSlidingExpirationContext context)
    {
        context.ShouldRenew = false;
        return Task.CompletedTask;
    }

    private void WriteExpiryHeaders(HttpResponse response, AuthenticationProperties properties)
    {
        Write(ExpiresAtHeader, properties.ExpiresUtc);
        Write(AbsoluteExpiresAtHeader, AbsoluteExpiry(properties, settings));

        void Write(string name, DateTimeOffset? value)
        {
            if (value is null)
            {
                response.Headers.Remove(name);
                return;
            }

            // Whole seconds, as the cookie ticket stores its times and the probe reports them.
            response.Headers[name] = value.Value.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// The absolute limit of a ticket in whole seconds, or null for a ticket without a stored
    /// sign-in time. Whole seconds as the ticket's idle limit and the headers: the probe reports
    /// both limits, and the idle limit capped at this one must come out equal to it, or the
    /// client takes the idle limit for the next one (#379, S6-01).
    /// </summary>
    public static DateTimeOffset? AbsoluteExpiry(AuthenticationProperties properties, SessionSettings settings)
    {
        if (!properties.Items.TryGetValue(SignedInUtcKey, out var signedIn)
            || !DateTimeOffset.TryParseExact(signedIn, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var signedInUtc))
        {
            return null;
        }

        var limit = signedInUtc + settings.AbsoluteTimeout;
        return limit.AddTicks(-(limit.Ticks % TimeSpan.TicksPerSecond));
    }
}

/// <summary>Marks the session probe, which must not renew the session (<see cref="SessionCookieEvents"/>).</summary>
internal sealed class SessionProbeMetadata;
