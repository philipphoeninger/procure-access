// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Domain;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using API.ProcureAccess.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using MODELS.ProcureAccess.Entities;
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.IdentityAccess.Session;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// The movable clock of the E2E stack (S5-30, owner decisions D4 (a), Q2 (a); TESTING §3.3): it is
/// registered only in <c>Development</c> and <c>E2E</c> with its switch, never in Staging or
/// Production, and it moves the time only for a request with the cookie <c>pa-test-clock</c>.
/// </summary>
public sealed class TestClockGuardTests(ApiFixture api) : IAsyncLifetime
{
    private const string Email = "clock@example.test";
    private const string Password = "Clock-Test-1!";
    private const string Account = "/api/v1/account";

    private readonly FakeTimeProvider _clock = new(ApiFixture.ClockStart);
    private readonly List<ProcureAccessApiFactory> _hosts = [];
    private readonly string _connectionString = api.ConnectionStringForNewDatabase();

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    [Theory]
    [InlineData("Testing")]
    [InlineData("Staging")]
    [InlineData("Production")]
    public void Startup_ClockSwitchedOnOutsideDevelopmentAndE2E_RegistersNoTestClock(string environment)
    {
        NewHost(environment, enabled: true).Services.GetService<TestClock>().ShouldBeNull();
    }

    /// <summary>Control for the theory above: the same switch registers the clock where it is allowed.</summary>
    [Theory]
    [InlineData("Development")]
    [InlineData(TestCatalogSeedExtensions.E2EEnvironmentName)]
    public void Startup_ClockSwitchedOnInDevelopmentOrE2E_RegistersTheTestClock(string environment)
    {
        NewHost(environment, enabled: true).Services.GetService<TestClock>().ShouldNotBeNull();
    }

    [Fact]
    public void Startup_ClockNotSwitchedOn_RegistersNoTestClock()
    {
        NewHost("Development", enabled: false).Services.GetService<TestClock>().ShouldBeNull();
    }

    /// <summary>
    /// The session reads the moved time: a request whose cookie asks for ten minutes more renews
    /// the idle limit from there, the next request without the cookie reads the real time again,
    /// and an offset past the idle limit ends the session.
    /// </summary>
    [Fact]
    public async Task Request_WithClockCookie_MovesTheSessionTimeOfThatRequestOnly()
    {
        var host = NewHost(TestCatalogSeedExtensions.E2EEnvironmentName, enabled: true, realAuthentication: true, fakeInner: true);
        await CreateUserAsync(host);
        var cookies = new CookieContainer();
        using var client = host.CreateDefaultClient(
            IntegrationTest.BaseAddress, new AntiforgeryHandler(cookies), new CookieContainerHandler(cookies));
        (await client.PostAsJsonAsync("/api/v1/auth/login", new { email = Email, password = Password }, Cancellation))
            .StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var idle = TimeSpan.FromMinutes(30);

        SetOffset(cookies, TimeSpan.FromMinutes(10));
        using (var moved = await client.GetAsync(Account, Cancellation))
        {
            moved.StatusCode.ShouldBe(HttpStatusCode.OK);
            ExpiresAt(moved).ShouldBe(_clock.GetUtcNow() + TimeSpan.FromMinutes(10) + idle);
        }

        SetOffset(cookies, null);
        using (var real = await client.GetAsync(Account, Cancellation))
        {
            real.StatusCode.ShouldBe(HttpStatusCode.OK);
            ExpiresAt(real).ShouldBe(_clock.GetUtcNow() + idle);
        }

        SetOffset(cookies, idle + TimeSpan.FromSeconds(1));
        using var ended = await client.GetAsync(Account, Cancellation);
        ended.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>An offset that is not a whole number of seconds between 0 and two days is ignored.</summary>
    [Theory]
    [InlineData("-600")]
    [InlineData("600.5")]
    [InlineData("abc")]
    [InlineData("172801")]
    public async Task Request_WithInvalidClockCookie_ReadsTheRealTime(string value)
    {
        var host = NewHost(TestCatalogSeedExtensions.E2EEnvironmentName, enabled: true, realAuthentication: true, fakeInner: true);
        await CreateUserAsync(host);
        var cookies = new CookieContainer();
        using var client = host.CreateDefaultClient(
            IntegrationTest.BaseAddress, new AntiforgeryHandler(cookies), new CookieContainerHandler(cookies));
        (await client.PostAsJsonAsync("/api/v1/auth/login", new { email = Email, password = Password }, Cancellation))
            .StatusCode.ShouldBe(HttpStatusCode.NoContent);

        cookies.Add(IntegrationTest.BaseAddress, new Cookie(TestClockExtensions.CookieName, value));
        using var response = await client.GetAsync(Account, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        ExpiresAt(response).ShouldBe(_clock.GetUtcNow() + TimeSpan.FromMinutes(30));
    }

    /// <summary>
    /// A host of <paramref name="environment"/> with the clock switched on or off. With
    /// <paramref name="fakeInner"/> the registered clock moves this class's fake clock instead of the
    /// real one, so the times are exact; the test host's own fake clock would otherwise replace it.
    /// </summary>
    private ProcureAccessApiFactory NewHost(string environment, bool enabled, bool realAuthentication = false, bool fakeInner = false)
    {
        var host = new ProcureAccessApiFactory(
            _connectionString,
            _clock,
            environment,
            new Dictionary<string, string?> { [TestClockExtensions.EnabledKey] = enabled ? "true" : "false" },
            configureServices: fakeInner
                ? services =>
                {
                    services.RemoveAll<TestClock>();
                    services.AddSingleton(p => new TestClock(_clock, p.GetRequiredService<IHttpContextAccessor>()));
                    services.Replace(ServiceDescriptor.Singleton<TimeProvider>(p => p.GetRequiredService<TestClock>()));
                }
        : null,
            realAuthentication: realAuthentication);
        _hosts.Add(host);
        return host;
    }

    private static async Task CreateUserAsync(ProcureAccessApiFactory host)
    {
        using var scope = host.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = new User { UserName = Email, Email = Email, EmailConfirmed = true };
        (await users.CreateAsync(user, Password)).Succeeded.ShouldBeTrue();
    }

    private static void SetOffset(CookieContainer cookies, TimeSpan? offset) =>
        cookies.Add(IntegrationTest.BaseAddress, new Cookie(
            TestClockExtensions.CookieName,
            ((long)(offset ?? TimeSpan.Zero).TotalSeconds).ToString(CultureInfo.InvariantCulture)));

    private static DateTimeOffset ExpiresAt(HttpResponseMessage response) =>
        DateTimeOffset.Parse(
            response.Headers.GetValues(SessionCookieEvents.ExpiresAtHeader).ShouldHaveSingleItem(),
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);

    public async ValueTask DisposeAsync()
    {
        foreach (var host in _hosts)
        {
            await host.DisposeAsync();
        }

        NpgsqlConnection.ClearAllPools();
        await using var context = new IdentityAccessDbContext(
            new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(_connectionString).Options, FixedOrganizationContext.None);
        await context.Database.EnsureDeletedAsync();
    }
}
