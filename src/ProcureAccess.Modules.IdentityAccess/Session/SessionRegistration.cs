// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ProcureAccess.Modules.IdentityAccess.Session;

/// <summary>
/// Registers the cookie session and antiforgery (ARCHITECTURE §9.1, C50, C52, C60; SPEC
/// R-AUTH-05; SEC-01, SEC-02, SEC-09). Called from <see cref="IdentityAccessModule"/>.
/// </summary>
internal static class SessionRegistration
{
    /// <summary>Name of the session cookie. Fixed, so a reverse proxy or a test can name it.</summary>
    public const string SessionCookieName = ".ProcureAccess.Session";

    /// <summary>Name of the HttpOnly half of the antiforgery token pair.</summary>
    public const string AntiforgeryCookieName = ".ProcureAccess.Antiforgery";

    public static IServiceCollection AddSession(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Read now, so a bad value stops the start before the host is built (like the rate limits).
        var settings = SessionSettings.From(configuration);
        services.AddSingleton(settings);

        // Secure everywhere; Secure only on HTTPS requests in Development and E2E (plain-HTTP stacks).
        var cookiePolicy = SessionCookiePolicy.For(environment);
        services.AddSingleton(cookiePolicy);
        services.AddScoped<SessionCookieEvents>();

        // The three Identity cookie schemes (application, external, two-factor). SignInManager
        // signs out of all three, so they must exist; only the application cookie is used.
        // The default scheme stays the host's choice (legacy JWT next to the cookie until S5-08).
        services.AddAuthentication().AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = SessionCookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = cookiePolicy.SecurePolicy;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.IsEssential = true;
            options.ExpireTimeSpan = settings.IdleTimeout;
            options.SlidingExpiration = true;
            options.EventsType = typeof(SessionCookieEvents);
        });

        // Every time read goes through TimeProvider (CLAUDE §7): the cookie handler (ticket expiry,
        // renewal) and the security-stamp validator (its validation interval) take the API's clock.
        services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
                .Configure<TimeProvider>((options, clock) => options.TimeProvider = clock);
        services.AddOptions<SecurityStampValidatorOptions>()
                .Configure<TimeProvider>((options, clock) => options.TimeProvider = clock);

        services.AddAntiforgery(options =>
        {
            options.HeaderName = AntiforgeryTokenCookie.HeaderName;
            options.Cookie.Name = AntiforgeryCookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = cookiePolicy.SecurePolicy;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.IsEssential = true;
            // The header is the only way the token arrives: the API has no forms.
            options.FormFieldName = "__pa_unused_antiforgery_field";
            options.SuppressXFrameOptionsHeader = false;
        });

        return services;
    }
}
