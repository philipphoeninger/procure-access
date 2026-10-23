// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MODELS.ProcureAccess.Entities.Authorization;

namespace ProcureAccess.Tests.Shared.Auth;

/// <summary>
/// Authentication handler for integration tests (TESTING §5.2). It signs a request in as the
/// fixture user named in the <see cref="UserHeader"/> header, so most tests do not pay for the
/// real sign-in flow. The real session and antiforgery flow is tested on its own (SEC-01/02/09).
/// </summary>
/// <remarks>
/// No header → no result, i.e. the request stays anonymous (e.g. the anonymous export).
/// An unknown user name fails authentication, so a typo in a test cannot pass as anonymous.
/// </remarks>
public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>Name of the authentication scheme.</summary>
    public const string SchemeName = "Test";

    /// <summary>Request header that carries the <see cref="TestUser.Name"/>.</summary>
    public const string UserHeader = "X-Test-User";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserHeader, out var values))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var name = values.ToString();
        if (!TestUsers.ByName.TryGetValue(name, out var user))
        {
            return Task.FromResult(AuthenticateResult.Fail($"Unknown test user '{name}'."));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Email, user.Email),
        };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.Permissions.Select(p => new Claim(CustomClaimTypes.Permission, p)));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
