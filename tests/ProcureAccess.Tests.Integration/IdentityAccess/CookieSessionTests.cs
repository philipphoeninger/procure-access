// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using MODELS.ProcureAccess.Entities;
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.IdentityAccess.Session;
using ProcureAccess.Modules.Platform.Contracts.Email;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;

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
    private const string Extend = "/api/v1/auth/session/extend";
    private const string Account = "/api/v1/account";
    private const string ExpiresAtHeader = SessionCookieEvents.ExpiresAtHeader;
    private const string AbsoluteExpiresAtHeader = SessionCookieEvents.AbsoluteExpiresAtHeader;
    private const string Email = "session@example.test";
    private const string Password = "Session-Test-1!";
    private const string SessionCookie = SessionRegistration.SessionCookieName;

    private readonly FakeTimeProvider _clock = new(ApiFixture.ClockStart);
    private readonly List<ProcureAccessApiFactory> _hosts = [];
    private string? _connectionString;

    private ProcureAccessApiFactory Host => _hosts[0];

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

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
            new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(_connectionString!).Options, FixedOrganizationContext.None);
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

        using var response = await client.GetAsync("/api/v1/account", TestContext.Current.CancellationToken);

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

    [Fact(DisplayName = "INV:SEC-07 " + nameof(Session_IdleLongerThanTheIdleLimit_Yields401))]
    [Trait("Invariant", "SEC-07")]
    public async Task Session_IdleLongerThanTheIdleLimit_Yields401()
    {
        using var client = CreateClient(Host);
        await LoginAsync(client);
        (await SessionUserAsync(client)).ShouldNotBeNull();

        _clock.Advance(TimeSpan.FromMinutes(30));

        using var response = await client.GetAsync(Account, Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.Contains(ExpiresAtHeader).ShouldBeFalse();
        (await SessionUserAsync(client)).ShouldBeNull();
    }

    [Fact(DisplayName = "INV:SEC-07 " + nameof(Session_RequestAtMinute20_IsStillValidAtMinute49AndEndsAtMinute50))]
    [Trait("Invariant", "SEC-07")]
    public async Task Session_RequestAtMinute20_IsStillValidAtMinute49AndEndsAtMinute50()
    {
        using var client = CreateClient(Host);
        await LoginAsync(client);

        // Before half of the idle window has passed and after it: every request renews, not only
        // one in the second half as the cookie handler's own rule would.
        _clock.Advance(TimeSpan.FromMinutes(10));
        using var atMinute10 = await client.GetAsync(Account, Cancellation);
        _clock.Advance(TimeSpan.FromMinutes(10));
        using var atMinute20 = await client.GetAsync(Account, Cancellation);
        _clock.Advance(TimeSpan.FromMinutes(29));
        using var atMinute49 = await client.GetAsync(Account, Cancellation);

        atMinute10.StatusCode.ShouldBe(HttpStatusCode.OK);
        SetCookie(atMinute10, SessionCookie).ShouldNotBeNull();
        atMinute20.StatusCode.ShouldBe(HttpStatusCode.OK);
        atMinute49.StatusCode.ShouldBe(HttpStatusCode.OK);

        // The request at minute 49 renewed to minute 79.
        _clock.Advance(TimeSpan.FromMinutes(30));
        using var atMinute79 = await client.GetAsync(Account, Cancellation);
        atMinute79.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "INV:SEC-07 " + nameof(SessionProbe_AskedLate_ReportsTheLimitsAndDoesNotRenew))]
    [Trait("Invariant", "SEC-07")]
    public async Task SessionProbe_AskedLate_ReportsTheLimitsAndDoesNotRenew()
    {
        using var client = CreateClient(Host);
        var signedInAt = _clock.GetUtcNow();
        await LoginAsync(client);

        _clock.Advance(TimeSpan.FromMinutes(20));
        using var probe = await client.GetAsync(Probe, Cancellation);

        using var body = JsonDocument.Parse(await probe.Content.ReadAsStringAsync(Cancellation));
        body.RootElement.GetProperty("expiresAt").GetDateTimeOffset().ShouldBe(signedInAt + TimeSpan.FromMinutes(30));
        body.RootElement.GetProperty("absoluteExpiresAt").GetDateTimeOffset().ShouldBe(signedInAt + TimeSpan.FromHours(8));
        body.RootElement.GetProperty("warningLeadSeconds").GetInt32().ShouldBe(300);
        SetCookie(probe, SessionCookie).ShouldBeNull();
        ExpiryHeader(probe, ExpiresAtHeader).ShouldBe(signedInAt + TimeSpan.FromMinutes(30));

        // Minute 30: the probe at minute 20 kept nothing alive.
        _clock.Advance(TimeSpan.FromMinutes(10));
        (await SessionUserAsync(client)).ShouldBeNull();
    }

    [Fact(DisplayName = "INV:SEC-07 " + nameof(Extend_SignedIn_RenewsOnlyTheIdleLimitAndKeepsTheSignInTime))]
    [Trait("Invariant", "SEC-07")]
    public async Task Extend_SignedIn_RenewsOnlyTheIdleLimitAndKeepsTheSignInTime()
    {
        using var client = CreateClient(Host);
        var signedInAt = _clock.GetUtcNow();
        await LoginAsync(client);

        _clock.Advance(TimeSpan.FromMinutes(25));
        using var extended = await client.PostAsync(Extend, null, Cancellation);

        extended.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        ExpiryHeader(extended, ExpiresAtHeader).ShouldBe(signedInAt + TimeSpan.FromMinutes(55));
        ExpiryHeader(extended, AbsoluteExpiresAtHeader).ShouldBe(signedInAt + TimeSpan.FromHours(8));

        // The probe reads the renewed cookie: the same two times.
        using var probe = await client.GetAsync(Probe, Cancellation);
        using var body = JsonDocument.Parse(await probe.Content.ReadAsStringAsync(Cancellation));
        body.RootElement.GetProperty("expiresAt").GetDateTimeOffset().ShouldBe(signedInAt + TimeSpan.FromMinutes(55));
        body.RootElement.GetProperty("absoluteExpiresAt").GetDateTimeOffset().ShouldBe(signedInAt + TimeSpan.FromHours(8));
    }

    [Fact(DisplayName = "INV:SEC-07 " + nameof(Extend_WithoutSessionOrToken_IsRefused))]
    [Trait("Invariant", "SEC-07")]
    public async Task Extend_WithoutSessionOrToken_IsRefused()
    {
        using var anonymous = CreateClient(Host);
        using var noSession = await anonymous.PostAsync(Extend, null, Cancellation);
        noSession.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        noSession.Headers.Contains(ExpiresAtHeader).ShouldBeFalse();

        // Signed in, but the request carries no antiforgery token (SEC-02).
        var cookies = new CookieContainer();
        using (var client = CreateClient(Host, cookies))
        {
            await LoginAsync(client);
        }

        using var forged = Host.CreateDefaultClient(IntegrationTest.BaseAddress, new CookieContainerHandler(cookies));
        using var noToken = await forged.PostAsync(Extend, null, Cancellation);
        noToken.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemTypeOf(noToken)).ShouldEndWith("/problems/" + ProblemTypes.Antiforgery);
    }

    [Fact(DisplayName = "INV:SEC-07 " + nameof(Responses_SignedIn_CarryBothExpiryHeadersAndOthersNone))]
    [Trait("Invariant", "SEC-07")]
    public async Task Responses_SignedIn_CarryBothExpiryHeadersAndOthersNone()
    {
        using var client = CreateClient(Host);
        using var anonymous = await client.GetAsync(Probe, Cancellation);
        var signedInAt = _clock.GetUtcNow();
        using var login = await LoginAsync(client);

        _clock.Advance(TimeSpan.FromMinutes(5));
        using var module = await client.GetAsync(Account, Cancellation);
        _clock.Advance(TimeSpan.FromMinutes(5));
        using var legacy = await client.GetAsync("/api/me", Cancellation);
        using var missing = await client.GetAsync("/api/v1/nothing-here", Cancellation);
        using var logout = await client.PostAsync(Logout, null, Cancellation);

        anonymous.Headers.Contains(ExpiresAtHeader).ShouldBeFalse();
        anonymous.Headers.Contains(AbsoluteExpiresAtHeader).ShouldBeFalse();
        ExpiryHeader(login, ExpiresAtHeader).ShouldBe(signedInAt + TimeSpan.FromMinutes(30));
        ExpiryHeader(login, AbsoluteExpiresAtHeader).ShouldBe(signedInAt + TimeSpan.FromHours(8));
        ExpiryHeader(module, ExpiresAtHeader).ShouldBe(signedInAt + TimeSpan.FromMinutes(35));
        ExpiryHeader(module, AbsoluteExpiresAtHeader).ShouldBe(signedInAt + TimeSpan.FromHours(8));
        ExpiryHeader(legacy, ExpiresAtHeader).ShouldBe(signedInAt + TimeSpan.FromMinutes(40));
        ExpiryHeader(legacy, AbsoluteExpiresAtHeader).ShouldBe(signedInAt + TimeSpan.FromHours(8));
        missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ExpiryHeader(missing, ExpiresAtHeader).ShouldBe(signedInAt + TimeSpan.FromMinutes(40));
        logout.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        logout.Headers.Contains(ExpiresAtHeader).ShouldBeFalse();
        logout.Headers.Contains(AbsoluteExpiresAtHeader).ShouldBeFalse();
    }

    [Fact(DisplayName = "INV:SEC-07 " + nameof(Session_ActiveEveryTwentyMinutes_EndsAtTheAbsoluteLimit))]
    [Trait("Invariant", "SEC-07")]
    public async Task Session_ActiveEveryTwentyMinutes_EndsAtTheAbsoluteLimit()
    {
        using var client = CreateClient(Host);
        var signedInAt = _clock.GetUtcNow();
        await LoginAsync(client);

        // 23 × 20 minutes = 7 h 40 min of activity: every request renews the idle limit.
        for (var i = 0; i < 23; i++)
        {
            _clock.Advance(TimeSpan.FromMinutes(20));
            using var active = await client.GetAsync("/api/me", Cancellation);
            active.StatusCode.ShouldBe(HttpStatusCode.OK, $"request after {(i + 1) * 20} minutes");
        }

        // 7 h 50 min: the idle limit would be 8 h 20 min; it stops at the absolute limit.
        _clock.Advance(TimeSpan.FromMinutes(10));
        using var extended = await client.PostAsync(Extend, null, Cancellation);
        extended.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        ExpiryHeader(extended, ExpiresAtHeader).ShouldBe(signedInAt + TimeSpan.FromHours(8));
        ExpiryHeader(extended, AbsoluteExpiresAtHeader).ShouldBe(signedInAt + TimeSpan.FromHours(8));

        // 8 hours after sign-in the session ends although the idle limit was never reached.
        _clock.Advance(TimeSpan.FromMinutes(10));
        using var late = await client.GetAsync(Account, Cancellation);
        late.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        SetCookie(late, SessionCookie).ShouldNotBeNull().ShouldContain("expires=Thu, 01 Jan 1970", Case.Insensitive);
        (await SessionUserAsync(client)).ShouldBeNull();
    }

    [Fact(DisplayName = "INV:SEC-08 " + nameof(PasswordReset_Completed_EndsTheOtherSessionsWithTheirNextRequest))]
    [Trait("Invariant", "SEC-08")]
    public async Task PasswordReset_Completed_EndsTheOtherSessionsWithTheirNextRequest()
    {
        using var laptop = CreateClient(Host);
        using var phone = CreateClient(Host);
        await LoginAsync(laptop);
        await LoginAsync(phone);
        _clock.Advance(TimeSpan.FromMinutes(1));
        using var before = await phone.GetAsync(Account, Cancellation);
        before.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var somebody = CreateClient(Host);
        using var requested = await somebody.PostAsJsonAsync("/api/v1/auth/password-reset", new { email = Email }, Cancellation);
        requested.StatusCode.ShouldBe(HttpStatusCode.OK);
        var token = QueryHelpers.ParseQuery((await Host.Emails.WaitForAsync<PasswordResetEmail>()).Link.Query)["token"].ToString();
        using var completed = await somebody.PostAsJsonAsync(
            "/api/v1/auth/password-reset/complete", new { token, newPassword = "Ein-neues-Passwort-2026" }, Cancellation);
        completed.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // One second later, far inside the idle limit: both sessions are over.
        _clock.Advance(TimeSpan.FromSeconds(1));
        using var onLaptop = await laptop.GetAsync(Account, Cancellation);
        using var onPhone = await phone.PostAsync(Extend, null, Cancellation);

        onLaptop.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        onPhone.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        SetCookie(onLaptop, SessionCookie).ShouldNotBeNull().ShouldContain("expires=Thu, 01 Jan 1970", Case.Insensitive);
        onLaptop.Headers.Contains(ExpiresAtHeader).ShouldBeFalse();
        (await SessionUserAsync(phone)).ShouldBeNull();
    }

    [Fact(DisplayName = "INV:SEC-08 " + nameof(AccountDeletion_InOneSession_EndsTheOtherSessionsWithTheirNextRequest))]
    [Trait("Invariant", "SEC-08")]
    public async Task AccountDeletion_InOneSession_EndsTheOtherSessionsWithTheirNextRequest()
    {
        using var laptop = CreateClient(Host);
        using var phone = CreateClient(Host);
        await LoginAsync(laptop);
        await LoginAsync(phone);
        _clock.Advance(TimeSpan.FromMinutes(1));
        using var before = await phone.GetAsync(Account, Cancellation);
        before.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var deleted = await laptop.DeleteAsync(Account, Cancellation);
        deleted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        deleted.Headers.Contains(ExpiresAtHeader).ShouldBeFalse();

        _clock.Advance(TimeSpan.FromSeconds(1));
        using var onPhone = await phone.GetAsync(Account, Cancellation);

        onPhone.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await SessionUserAsync(phone)).ShouldBeNull();
    }

    [Fact]
    public async Task Session_RoleTakenAway_LosesItsPermissionWithTheNextRequest()
    {
        using var client = CreateClient(Host);
        await LoginAsync(client);
        (await SessionUserAsync(client)).ShouldNotBeNull().GetProperty("roles").GetArrayLength().ShouldBe(1);

        using (var scope = Host.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = (await users.FindByEmailAsync(Email)).ShouldNotBeNull();
            (await users.RemoveFromRoleAsync(user, "Moderator")).Succeeded.ShouldBeTrue();
        }

        // The stamp check rebuilds the principal from the database (owner decision S5-29 4a).
        _clock.Advance(TimeSpan.FromSeconds(1));
        var after = (await SessionUserAsync(client)).ShouldNotBeNull();
        after.GetProperty("roles").GetArrayLength().ShouldBe(0);
        after.GetProperty("permissions").GetArrayLength().ShouldBe(0);
    }

    // Owner decision S5-29 3a: an answer that renews the session cookie is never stored, so a
    // signed-in user reads the catalog uncached; an anonymous one keeps the catalog's own header.
    [Fact]
    public async Task Catalog_SignedIn_IsNotStoredAndAnonymousKeepsItsCacheControl()
    {
        await ImportBothAndActivateAsync(Host.Services, _clock, MiniCatalog.T1Version, Cancellation);
        const string criteria = "/api/v1/catalog/criteria?release=T.1&useCase=hr&appTypeIds[]=web&productPartIds[]=base";
        using var client = CreateClient(Host);

        using var anonymous = await client.GetAsync(criteria, Cancellation);
        await LoginAsync(client);
        using var signedIn = await client.GetAsync(criteria, Cancellation);

        anonymous.StatusCode.ShouldBe(HttpStatusCode.OK);
        anonymous.Headers.CacheControl.ShouldNotBeNull().Public.ShouldBeTrue();
        anonymous.Headers.Contains(ExpiresAtHeader).ShouldBeFalse();
        signedIn.StatusCode.ShouldBe(HttpStatusCode.OK);
        SetCookie(signedIn, SessionCookie).ShouldNotBeNull();
        signedIn.Headers.CacheControl.ShouldNotBeNull().ShouldSatisfyAllConditions(
            c => c.NoStore.ShouldBeTrue(),
            c => c.Public.ShouldBeFalse());
        signedIn.Headers.Contains(ExpiresAtHeader).ShouldBeTrue();
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

    /// <summary>The time in an expiry header, which must be ISO 8601 in UTC.</summary>
    private static DateTimeOffset ExpiryHeader(HttpResponseMessage response, string name)
    {
        var value = response.Headers.GetValues(name).ShouldHaveSingleItem();
        value.ShouldEndWith("Z");
        return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
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
