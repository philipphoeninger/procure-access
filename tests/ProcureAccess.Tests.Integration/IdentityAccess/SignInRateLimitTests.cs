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
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// The sign-in limit per e-mail address of <c>POST /api/v1/auth/login</c> (SPEC §7.3, API-05; S6-26,
/// owner decision O-S6-2): the endpoint counts the address before it looks it up, so an address
/// without an account gets the same 429 as one with. Runs on a host with the API's own
/// authentication and antiforgery, on a database of this class, with a per-address limit of
/// <see cref="AddressPermitLimit"/>; the limit per client IP is in <c>RateLimitTests</c>, the
/// lockout in <c>PasswordResetTests</c>.
/// </summary>
/// <remarks>
/// The limiter keeps its own clock, not the fake one (<c>RateLimitingRegistration</c>), so the
/// tests check the count and the window's length, not its passing (owner decision S6-26 Q1 (a)).
/// </remarks>
public sealed class SignInRateLimitTests(ApiFixture api) : IAsyncLifetime
{
    private const string Login = "/api/v1/auth/login";
    private const string Address = "anmelden@example.test";
    private const string OtherAddress = "andere@example.test";
    private const string Password = "richtiges-passwort-1";
    private const int AddressPermitLimit = 2;

    private readonly FakeTimeProvider _clock = new(ApiFixture.ClockStart);
    private ProcureAccessApiFactory? _host;
    private string? _connectionString;

    private ProcureAccessApiFactory Host => _host ?? throw new InvalidOperationException("The host is not started.");

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync()
    {
        _connectionString = api.ConnectionStringForNewDatabase();
        _host = new ProcureAccessApiFactory(
            _connectionString,
            _clock,
            settings: new Dictionary<string, string?>
            {
                ["RateLimits:SignInAddress:PermitLimit"] = $"{AddressPermitLimit}",
            },
            realAuthentication: true);
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }

        NpgsqlConnection.ClearAllPools();
        await using var context = new IdentityAccessDbContext(
            new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(_connectionString!).Options, FixedOrganizationContext.None);
        await context.Database.EnsureDeletedAsync();
    }

    [Fact(DisplayName = "INV:API-05 " + nameof(Login_OneMoreThanTheLimitForOneAddress_Returns429AndLeavesOtherAddressesFree))]
    [Trait("Invariant", "API-05")]
    public async Task Login_OneMoreThanTheLimitForOneAddress_Returns429AndLeavesOtherAddressesFree()
    {
        await CreateUserAsync(Address);
        await CreateUserAsync(OtherAddress);
        using var client = CreateClient();

        // A failed and a successful sign-in both count.
        using (var failed = await LoginAsync(client, Address, "falsches-passwort-1"))
        {
            failed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        using (var signedIn = await LoginAsync(client, Address, Password))
        {
            signedIn.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        // The same address in another case and with spaces counts for the same partition; the right
        // password does not help.
        using var limited = await LoginAsync(client, $" {Address.ToUpperInvariant()} ", Password);
        using var otherAddress = await LoginAsync(client, OtherAddress, Password);

        limited.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        using (var body = JsonDocument.Parse(await limited.Content.ReadAsStringAsync(Cancellation)))
        {
            body.RootElement.GetProperty("type").GetString().ShouldEndWith("/problems/" + ProblemTypes.RateLimit);
            body.RootElement.GetProperty("retryAfterMinutes").GetInt32().ShouldBe(60);
        }

        limited.Headers.RetryAfter.ShouldNotBeNull().Delta.ShouldBe(TimeSpan.FromHours(1));
        otherAddress.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "INV:API-05 " + nameof(Login_AddressWithAndWithoutAccountAtTheLimit_AnswerIdentically))]
    [Trait("Invariant", "API-05")]
    public async Task Login_AddressWithAndWithoutAccountAtTheLimit_AnswerIdentically()
    {
        await CreateUserAsync(Address);
        using var client = CreateClient();
        const string unknownAddress = "niemand@example.test";
        for (var i = 0; i < AddressPermitLimit; i++)
        {
            using var known = await LoginAsync(client, Address, "falsches-passwort-1");
            using var unknown = await LoginAsync(client, unknownAddress, "falsches-passwort-1");
            known.StatusCode.ShouldBe(HttpStatusCode.BadRequest, $"request {i + 1}");
            unknown.StatusCode.ShouldBe(HttpStatusCode.BadRequest, $"request {i + 1}");
        }

        using var knownLimited = await LoginAsync(client, Address, "falsches-passwort-1");
        using var unknownLimited = await LoginAsync(client, unknownAddress, "falsches-passwort-1");

        knownLimited.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        unknownLimited.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        (await WithoutTraceIdAsync(unknownLimited)).ShouldBe(await WithoutTraceIdAsync(knownLimited));
    }

    [Fact]
    public async Task Login_RefusedByTheAddressLimit_DoesNotCountTowardsTheLockout()
    {
        await CreateUserAsync(Address);
        using var client = CreateClient();
        for (var i = 0; i < AddressPermitLimit; i++)
        {
            using var failed = await LoginAsync(client, Address, "falsches-passwort-1");
        }

        for (var i = 0; i < 5; i++)
        {
            using var limited = await LoginAsync(client, Address, "falsches-passwort-1");
            limited.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        }

        // Only the two attempts that reached the password check count (SEC-11 stays as it is).
        (await FindUserAsync(Address)).ShouldNotBeNull().AccessFailedCount.ShouldBe(AddressPermitLimit);
    }

    [Fact]
    public async Task Login_WithoutAPassword_IsRefusedAndDoesNotCountForTheAddress()
    {
        await CreateUserAsync(Address);
        using var client = CreateClient();
        for (var i = 0; i <= AddressPermitLimit; i++)
        {
            using var incomplete = await LoginAsync(client, Address, "");
            incomplete.StatusCode.ShouldBe(HttpStatusCode.BadRequest, $"request {i + 1}");
        }

        using var signedIn = await LoginAsync(client, Address, Password);

        signedIn.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    private HttpClient CreateClient()
    {
        var cookies = new CookieContainer();
        return Host.CreateDefaultClient(IntegrationTest.BaseAddress, new AntiforgeryHandler(cookies), new CookieContainerHandler(cookies));
    }

    private async Task CreateUserAsync(string email)
    {
        using var scope = Host.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = new User { UserName = email, Email = email, EmailConfirmed = true };
        (await users.CreateAsync(user, Password)).Succeeded.ShouldBeTrue();
    }

    private async Task<User?> FindUserAsync(string email)
    {
        using var scope = Host.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager<User>>().FindByEmailAsync(email);
    }

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password) =>
        client.PostAsJsonAsync(Login, new { email, password }, Cancellation);

    /// <summary>The answer without its <c>traceId</c>, which differs per request.</summary>
    private static async Task<string> WithoutTraceIdAsync(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return JsonSerializer.Serialize(body.RootElement.EnumerateObject()
            .Where(p => p.Name != "traceId")
            .ToDictionary(p => p.Name, p => p.Value));
    }
}
