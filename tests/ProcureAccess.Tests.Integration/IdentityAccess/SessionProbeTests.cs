// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// <c>GET /api/v1/auth/session</c> on the shared host with the test scheme (#87, SEC-09): it
/// answers 200 for everyone — anonymous, a fixture user and an unknown <c>X-Test-User</c>, whose
/// authentication fails — and issues the <c>XSRF-TOKEN</c> cookie, so the antiforgery hook of the
/// test client (<see cref="AntiforgeryHandler"/>) works for every test. The real cookie session is
/// in <see cref="CookieSessionTests"/>.
/// </summary>
public sealed class SessionProbeTests(ApiFixture api) : IntegrationTest(api)
{
    private const string Probe = AntiforgeryHandler.SessionProbePath;

    [Fact(DisplayName = "INV:SEC-09 " + nameof(SessionProbe_Anonymous_Returns200UserNullAndIssuesTokenCookie))]
    [Trait("Invariant", "SEC-09")]
    public async Task SessionProbe_Anonymous_Returns200UserNullAndIssuesTokenCookie()
    {
        using var client = Api.Factory.CreateDefaultClient(BaseAddress);

        using var response = await client.GetAsync(Probe, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldBe("""{"user":null}""");
        TokenCookie(response).ShouldNotBeNull().ShouldContain("secure", Case.Insensitive);
        TokenCookie(response)!.ShouldContain("samesite=strict", Case.Insensitive);
        TokenCookie(response)!.ShouldNotContain("httponly", Case.Insensitive);
        response.Headers.CacheControl!.NoStore.ShouldBeTrue();
    }

    [Fact]
    public async Task SessionProbe_UnknownTestUser_Returns200UserNull()
    {
        using var client = Api.Factory.CreateDefaultClient(BaseAddress);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, "nobody");

        using var response = await client.GetAsync(Probe, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldBe("""{"user":null}""");
        TokenCookie(response).ShouldNotBeNull();
    }

    [Fact]
    public async Task SessionProbe_FixtureUser_ReturnsUserWithoutExpiryTimes()
    {
        using var client = CreateClient(TestUsers.Moderator);

        using var response = await client.GetAsync(Probe, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var user = body.RootElement.GetProperty("user");
        user.GetProperty("id").GetString().ShouldBe(TestUsers.Moderator.Id);
        user.GetProperty("email").GetString().ShouldBe(TestUsers.Moderator.Email);
        user.GetProperty("roles").EnumerateArray().Select(r => r.GetString()).ShouldBe(TestUsers.Moderator.Roles);

        // The test scheme has no session limits, like the legacy JWT: the times are left out.
        body.RootElement.TryGetProperty("expiresAt", out _).ShouldBeFalse();
        body.RootElement.TryGetProperty("absoluteExpiresAt", out _).ShouldBeFalse();
    }

    private static string? TokenCookie(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var cookies)
            ? cookies.FirstOrDefault(c => c.StartsWith(AntiforgeryHandler.TokenCookieName + "=", StringComparison.Ordinal))
            : null;
}
