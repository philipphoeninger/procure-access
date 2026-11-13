// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using MODELS.ProcureAccess.Entities;
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.IdentityAccess.Session;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// The real cookie session (S5-07; ARCHITECTURE §9.1, C60; SPEC R-AUTH-05): sign-in and sign-out
/// through <c>POST /api/v1/auth/login</c> and <c>/logout</c>, the cookie flags (SEC-01), 401
/// without a redirect (SEC-03), the antiforgery token rotation (SEC-09), a cookie accepted by a
/// second host on the same database (OPS-04), and the idle and absolute limits on the API's clock.
/// Runs on hosts with the API's own authentication
/// (<c>realAuthentication</c>, #88) on a database of this class, with a clock of its own.
/// </summary>
public sealed class CookieSessionTests(ApiFixture api) : IAsyncLifetime
{
    private const string Probe = AntiforgeryHandler.SessionProbePath;
    private const string Login = "/api/v1/auth/login";
    private const string Logout = "/api/v1/auth/logout";
    private const string Email = "session@example.test";
    private const string Password = "Session-Test-1!";
    private const string SessionCookie = SessionRegistration.SessionCookieName;

    private readonly FakeTimeProvider _clock = new(ApiFixture.ClockStart);
    private readonly List<ProcureAccessApiFactory> _hosts = [];
    private string? _connectionString;

    private ProcureAccessApiFactory Host => _hosts[0];

    public async ValueTask InitializeAsync()
    {
        _connectionString = api.ConnectionStringForNewDatabase();
        var host = NewHost();

        using var scope = host.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = new User { UserName = Email, Email = Email, EmailConfirmed = true };
        (await users.CreateAsync(user, Password)).Succeeded.ShouldBeTrue();
        (await users.AddToRoleAsync(user, "Moderator")).Succeeded.ShouldBeTrue();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var host in _hosts)
        {
            await host.DisposeAsync();
        }

        NpgsqlConnection.ClearAllPools();
        await using var context = new IdentityAccessDbContext(
            new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(_connectionString!).Options);
        await context.Database.EnsureDeletedAsync();
    }

    [Fact(DisplayName = "INV:SEC-01 " + nameof(Login_ValidCredentials_SetsHttpOnlySecureStrictSessionCookie))]
    [Trait("Invariant", "SEC-01")]
    public async Task Login_ValidCredentials_SetsHttpOnlySecureStrictSessionCookie()
    {
        using var client = CreateClient(Host);

        using var response = await LoginAsync(client);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var cookie = SetCookie(response, SessionCookie).ShouldNotBeNull();
        cookie.ShouldContain("httponly", Case.Insensitive);
        cookie.ShouldContain("secure", Case.Insensitive);
        cookie.ShouldContain("samesite=strict", Case.Insensitive);

        // The only readable cookie is the antiforgery token (SEC-01).
        var readable = SetCookies(response).Where(c => !c.Contains("httponly", StringComparison.OrdinalIgnoreCase)).ToList();
        readable.ShouldAllBe(c => c.StartsWith(AntiforgeryHandler.TokenCookieName + "=", StringComparison.Ordinal));
        readable.ShouldNotBeEmpty();
    }

    [Fact(DisplayName = "INV:SEC-03 " + nameof(ProtectedEndpoint_Anonymous_Returns401ProblemDetailsWithoutRedirect))]
    [Trait("Invariant", "SEC-03")]
    public async Task ProtectedEndpoint_Anonymous_Returns401ProblemDetailsWithoutRedirect()
    {
        using var client = CreateClient(Host);

        using var response = await client.PostAsync("/api/Users/revoke-sessions", content: null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.Location.ShouldBeNull();
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
    }

    [Fact(DisplayName = "INV:SEC-03 " + nameof(ProtectedEndpoint_WithSessionCookie_IsAnswered))]
    [Trait("Invariant", "SEC-03")]
    public async Task ProtectedEndpoint_WithSessionCookie_IsAnswered()
    {
        using var client = CreateClient(Host);
        await LoginAsync(client);

        using var response = await client.GetAsync("/api/me", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldContain(Email);
    }

    [Fact(DisplayName = "INV:SEC-09 " + nameof(Login_AndLogout_RotateTheTokenAndRejectTheOldOne))]
    [Trait("Invariant", "SEC-09")]
    public async Task Login_AndLogout_RotateTheTokenAndRejectTheOldOne()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var cookies = new CookieContainer();
        using var client = CreateClient(Host, cookies);

        using var probe = await client.GetAsync(Probe, cancellationToken);
        var anonymousToken = TokenOf(cookies);
        SetCookie(probe, AntiforgeryHandler.TokenCookieName).ShouldNotBeNull();

        using var login = await LoginAsync(client);
        login.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var signedInToken = TokenOf(cookies);
        signedInToken.ShouldNotBe(anonymousToken);

        // The old token was issued for the anonymous user: refused for the signed-in one.
        using var withOldToken = new HttpRequestMessage(HttpMethod.Post, Logout);
        withOldToken.Headers.Add(AntiforgeryHandler.TokenHeaderName, anonymousToken);
        using var refused = await client.SendAsync(withOldToken, cancellationToken);
        refused.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemTypeOf(refused)).ShouldEndWith("/problems/" + ProblemTypes.Antiforgery);

        using var logout = await client.PostAsync(Logout, content: null, cancellationToken);
        logout.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        TokenOf(cookies).ShouldNotBe(signedInToken);
        SetCookie(logout, SessionCookie).ShouldNotBeNull().ShouldContain("expires=Thu, 01 Jan 1970", Case.Insensitive);

        (await SessionUserAsync(client)).ShouldBeNull();
    }

    [Fact(DisplayName = "INV:SEC-09 " + nameof(SessionProbe_SignedIn_ReturnsUserAndBothExpiryTimes))]
    [Trait("Invariant", "SEC-09")]
    public async Task SessionProbe_SignedIn_ReturnsUserAndBothExpiryTimes()
    {
        using var client = CreateClient(Host);
        var signedInAt = _clock.GetUtcNow();
        await LoginAsync(client);

        using var response = await client.GetAsync(Probe, TestContext.Current.CancellationToken);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var user = body.RootElement.GetProperty("user");
        user.GetProperty("email").GetString().ShouldBe(Email);
        user.GetProperty("roles").EnumerateArray().Select(r => r.GetString()).ShouldBe(["Moderator"]);
        // The permissions the roles grant, from the role claims in the cookie principal (S5-08, #320).
        user.GetProperty("permissions").EnumerateArray().Select(r => r.GetString()).ShouldBe(["moderation:review"]);
        body.RootElement.GetProperty("expiresAt").GetDateTimeOffset().ShouldBe(signedInAt + TimeSpan.FromMinutes(30));
        body.RootElement.GetProperty("absoluteExpiresAt").GetDateTimeOffset().ShouldBe(signedInAt + TimeSpan.FromHours(8));
    }

    [Fact(DisplayName = "INV:OPS-04 " + nameof(SessionCookie_IssuedByOneHost_IsAcceptedByASecondHostOnTheSameDatabase))]
    [Trait("Invariant", "OPS-04")]
    public async Task SessionCookie_IssuedByOneHost_IsAcceptedByASecondHostOnTheSameDatabase()
    {
        var cookies = new CookieContainer();
        using (var first = CreateClient(Host, cookies))
        {
            await LoginAsync(first);
        }

        // A new container after a deployment: another key-ring cache, the same database.
        using var second = CreateClient(NewHost(), cookies);

        var user = await SessionUserAsync(second);
        user.ShouldNotBeNull().GetProperty("email").GetString().ShouldBe(Email);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns400ValidationForTheBodyAsAWhole()
    {
        using var client = CreateClient(Host);

        using var response = await client.PostAsJsonAsync(Login, new { email = Email, password = "wrong" }, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemTypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.Validation);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name).ShouldBe(["$"]);
        SetCookie(response, SessionCookie).ShouldBeNull();
    }

    [Fact]
    public async Task Login_UnknownAddressOrMissingFields_Returns400Validation()
    {
        using var client = CreateClient(Host);

        using var unknown = await client.PostAsJsonAsync(Login, new { email = "nobody@example.test", password = Password }, TestContext.Current.CancellationToken);
        unknown.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemTypeOf(unknown)).ShouldEndWith("/problems/" + ProblemTypes.Validation);

        using var empty = await client.PostAsJsonAsync(Login, new { email = "", password = "" }, TestContext.Current.CancellationToken);
        empty.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await empty.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name).Order().ShouldBe(["email", "password"]);
    }

    [Fact]
    public async Task Session_IdleLongerThanTheIdleLimit_Ends()
    {
        using var client = CreateClient(Host);
        await LoginAsync(client);
        (await SessionUserAsync(client)).ShouldNotBeNull();

        _clock.Advance(TimeSpan.FromMinutes(31));

        (await SessionUserAsync(client)).ShouldBeNull();
    }

    [Fact]
    public async Task Session_ActiveEveryTwentyMinutes_EndsAtTheAbsoluteLimit()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = CreateClient(Host);
        await LoginAsync(client);

        // 23 × 20 minutes = 7 h 40 min of activity: every request renews the idle limit.
        for (var i = 0; i < 23; i++)
        {
            _clock.Advance(TimeSpan.FromMinutes(20));
            using var active = await client.GetAsync("/api/me", cancellationToken);
            active.StatusCode.ShouldBe(HttpStatusCode.OK, $"request after {(i + 1) * 20} minutes");
        }

        (await SessionUserAsync(client)).ShouldNotBeNull();

        _clock.Advance(TimeSpan.FromMinutes(20));

        // 8 hours after sign-in the session ends although the idle limit was never reached.
        (await SessionUserAsync(client)).ShouldBeNull();
    }

    private ProcureAccessApiFactory NewHost()
    {
        var host = new ProcureAccessApiFactory(_connectionString!, _clock, realAuthentication: true);
        _hosts.Add(host);
        return host;
    }

    private static HttpClient CreateClient(ProcureAccessApiFactory host, CookieContainer? cookies = null)
    {
        cookies ??= new CookieContainer();
        return host.CreateDefaultClient(
            IntegrationTest.BaseAddress,
            new AntiforgeryHandler(cookies),
            new CookieContainerHandler(cookies));
    }

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client) =>
        client.PostAsJsonAsync(Login, new { email = Email, password = Password }, TestContext.Current.CancellationToken);

    /// <summary>The <c>user</c> of the probe, or null for an anonymous answer.</summary>
    private static async Task<JsonElement?> SessionUserAsync(HttpClient client)
    {
        using var response = await client.GetAsync(Probe, TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var user = body.RootElement.GetProperty("user");
        return user.ValueKind == JsonValueKind.Null ? null : user.Clone();
    }

    private static string TokenOf(CookieContainer cookies) =>
        cookies.GetCookies(IntegrationTest.BaseAddress)[AntiforgeryHandler.TokenCookieName]!.Value;

    private static IEnumerable<string> SetCookies(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies : [];

    private static string? SetCookie(HttpResponseMessage response, string name) =>
        SetCookies(response).FirstOrDefault(c => c.StartsWith(name + "=", StringComparison.Ordinal));

    private static async Task<string> ProblemTypeOf(HttpResponseMessage response)
    {
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        return body.RootElement.GetProperty("type").GetString()!;
    }
}
