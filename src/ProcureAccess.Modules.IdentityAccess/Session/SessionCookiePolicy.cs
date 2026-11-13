// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using ProcureAccess.Modules.IdentityAccess.DataProtection;

namespace ProcureAccess.Modules.IdentityAccess.Session;

/// <summary>
/// The <c>Secure</c> rule of the session cookie, the antiforgery cookie and <c>XSRF-TOKEN</c>
/// (ARCHITECTURE §9.1; S5-07, owner decision 3a). <see cref="CookieSecurePolicy.Always"/>
/// everywhere — ASP.NET then refuses to issue or check the cookies on a non-HTTPS request — except
/// in <c>Development</c> and <c>E2E</c>, whose stacks run plain HTTP end to end: there the cookies
/// are <c>Secure</c> only on an HTTPS request, so sign-in works over <c>http://localhost</c>.
/// The same two environments may run without a key-ring certificate (<see cref="KeyRingCertificates"/>).
/// </summary>
internal sealed class SessionCookiePolicy(CookieSecurePolicy securePolicy)
{
    public CookieSecurePolicy SecurePolicy { get; } = securePolicy;

    public static SessionCookiePolicy For(IHostEnvironment environment) =>
        new(environment.IsDevelopment() || environment.IsEnvironment(KeyRingCertificates.E2EEnvironmentName)
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always);

    /// <summary>Whether a cookie issued for <paramref name="context"/> carries the <c>Secure</c> flag.</summary>
    public bool SecureFor(HttpContext context) =>
        SecurePolicy == CookieSecurePolicy.Always || context.Request.IsHttps;
}
