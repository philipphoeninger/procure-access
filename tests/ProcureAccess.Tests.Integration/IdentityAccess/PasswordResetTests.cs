// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Domain;
using System.Buffers.Text;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
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
using ProcureAccess.Modules.Platform.Contracts.Email;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// Password reset and sign-in lockout (S5-13a; SPEC R-AUTH-03, R-AUTH-04, §7.3, §8.8; ARCHITECTURE
/// §9.1, C72, C141): <c>POST /api/v1/auth/password-reset</c>, <c>/password-reset/complete</c> and the
/// lockout of <c>POST /api/v1/auth/login</c> on the fake clock (SEC-11). Runs on a host with the API's
/// own authentication on a database of this class, with a clock of its own; the per-address limit is
/// 2 here, so its test needs only three requests.
/// </summary>
public sealed class PasswordResetTests(ApiFixture api) : IAsyncLifetime
{
    private const string RequestReset = "/api/v1/auth/password-reset";
    private const string CompleteReset = "/api/v1/auth/password-reset/complete";
    private const string Login = "/api/v1/auth/login";
    private const string Address = "vergessen@example.test";
    private const string Password = "altes-passwort-1";
    private const string NewPassword = "neues-passwort-12";
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
                ["RateLimits:PasswordResetAddress:PermitLimit"] = $"{AddressPermitLimit}",
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

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Request_KnownAndUnknownAddress_AnswerIdenticallyAndOnlyTheKnownGetsALink))]
    [Trait("Invariant", "SEC-11")]
    public async Task Request_KnownAndUnknownAddress_AnswerIdenticallyAndOnlyTheKnownGetsALink()
    {
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();

        using var unknown = await RequestResetAsync(client, "niemand@example.test");
        using var known = await RequestResetAsync(client, Address);

        known.StatusCode.ShouldBe(HttpStatusCode.OK);
        unknown.StatusCode.ShouldBe(known.StatusCode);
        var knownBody = await known.Content.ReadAsStringAsync(Cancellation);
        (await unknown.Content.ReadAsStringAsync(Cancellation)).ShouldBe(knownBody);
        JsonDocument.Parse(knownBody).RootElement.GetProperty("validHours").GetInt32().ShouldBe(2);

        // One background reader in queue order: once the known address has its mail, the unknown
        // one was handled before it and sent nothing.
        var mail = await Host.Emails.WaitForAsync<PasswordResetEmail>();
        mail.Recipient.ShouldBe(Address);
        mail.ValidHours.ShouldBe(2);
        mail.Link.GetLeftPart(UriPartial.Path).ShouldBe("http://procureaccess.tech:4200/passwort/neu");
        Host.Emails.Sent.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Request_UnconfirmedAccount_GetsALinkAndTheResetConfirmsTheAddress()
    {
        await CreateUserAsync(Address, confirmed: false);
        using var client = CreateClient();

        await RequestResetAsync(client, Address);
        var token = TokenOf(await Host.Emails.WaitForAsync<PasswordResetEmail>());
        using var completed = await CompleteAsync(client, token, NewPassword);

        completed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await FindUserAsync(Address)).ShouldNotBeNull().EmailConfirmed.ShouldBeTrue();
    }

    [Fact]
    public async Task Request_AddressInOtherCase_SendsToTheStoredAddress()
    {
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();

        await RequestResetAsync(client, "  " + Address.ToUpperInvariant() + " ");

        (await Host.Emails.WaitForAsync<PasswordResetEmail>()).Recipient.ShouldBe(Address);
    }

    [Fact]
    public async Task Request_DeletedAccount_SendsNothing()
    {
        await CreateUserAsync("geloescht@example.test", confirmed: true, deleted: true);
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();

        await RequestResetAsync(client, "geloescht@example.test");
        await RequestResetAsync(client, Address);

        (await Host.Emails.WaitForAsync<PasswordResetEmail>()).Recipient.ShouldBe(Address);
        Host.Emails.Sent.ShouldHaveSingleItem();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("kein-at-zeichen")]
    [InlineData("Name <name@example.test>")]
    public async Task Request_MissingOrInvalidAddress_Returns400AtEmail(string? email)
    {
        using var client = CreateClient();

        using var response = await client.PostAsJsonAsync(RequestReset, new { email }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ErrorKeysOf(response)).ShouldBe(["email"]);
    }

    [Fact]
    public async Task Request_WithoutAntiforgeryToken_Returns400Antiforgery()
    {
        using var client = Host.CreateDefaultClient(IntegrationTest.BaseAddress);

        using var response = await client.PostAsJsonAsync(RequestReset, new { email = Address }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemTypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.Antiforgery);
    }

    [Fact(DisplayName = "INV:API-05 " + nameof(Request_OneMoreThanTheLimitForOneAddress_Returns429AndLeavesOtherAddressesFree))]
    [Trait("Invariant", "API-05")]
    public async Task Request_OneMoreThanTheLimitForOneAddress_Returns429AndLeavesOtherAddressesFree()
    {
        using var client = CreateClient();
        for (var i = 0; i < AddressPermitLimit; i++)
        {
            using var allowed = await RequestResetAsync(client, Address);
            allowed.StatusCode.ShouldBe(HttpStatusCode.OK, $"request {i + 1}");
        }

        // The same address in another case counts for the same partition.
        using var limited = await RequestResetAsync(client, Address.ToUpperInvariant());
        using var otherAddress = await RequestResetAsync(client, "andere@example.test");

        limited.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        (await ProblemTypeOf(limited)).ShouldEndWith("/problems/" + ProblemTypes.RateLimit);
        using (var body = JsonDocument.Parse(await limited.Content.ReadAsStringAsync(Cancellation)))
        {
            body.RootElement.GetProperty("retryAfterMinutes").GetInt32().ShouldBe(60);
        }

        limited.Headers.RetryAfter.ShouldNotBeNull().Delta.ShouldBe(TimeSpan.FromHours(1));
        otherAddress.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Complete_ValidLink_SetsThePasswordAndTheOldOneNoLongerSignsIn()
    {
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();
        await RequestResetAsync(client, Address);
        var token = TokenOf(await Host.Emails.WaitForAsync<PasswordResetEmail>());

        using var completed = await CompleteAsync(client, token, NewPassword);

        completed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var oldPassword = await LoginAsync(client, Password);
        oldPassword.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var newPassword = await LoginAsync(client, NewPassword);
        newPassword.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Complete_LinkUsedTwice_IsRefusedTheSecondTime))]
    [Trait("Invariant", "SEC-11")]
    public async Task Complete_LinkUsedTwice_IsRefusedTheSecondTime()
    {
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();
        await RequestResetAsync(client, Address);
        var token = TokenOf(await Host.Emails.WaitForAsync<PasswordResetEmail>());
        using var first = await CompleteAsync(client, token, NewPassword);

        using var second = await CompleteAsync(client, token, "noch-ein-passwort");

        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        second.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ErrorKeysOf(second)).ShouldBe(["token"]);
        using var login = await LoginAsync(client, NewPassword);
        login.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Complete_LinkOlderThanTwoHours_IsRefused))]
    [Trait("Invariant", "SEC-11")]
    public async Task Complete_LinkOlderThanTwoHours_IsRefused()
    {
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();
        await RequestResetAsync(client, Address);
        var token = TokenOf(await Host.Emails.WaitForAsync<PasswordResetEmail>());

        _clock.Advance(TimeSpan.FromHours(2));
        using var response = await CompleteAsync(client, token, NewPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ErrorKeysOf(response)).ShouldBe(["token"]);
        using var login = await LoginAsync(client, Password);
        login.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Complete_LinkJustBeforeTwoHours_IsAccepted))]
    [Trait("Invariant", "SEC-11")]
    public async Task Complete_LinkJustBeforeTwoHours_IsAccepted()
    {
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();
        await RequestResetAsync(client, Address);
        var token = TokenOf(await Host.Emails.WaitForAsync<PasswordResetEmail>());

        _clock.Advance(TimeSpan.FromHours(2) - TimeSpan.FromSeconds(1));
        using var response = await CompleteAsync(client, token, NewPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(ResetToken_IsStoredOnlyAsTheSha256OfItsBytes))]
    [Trait("Invariant", "SEC-11")]
    public async Task ResetToken_IsStoredOnlyAsTheSha256OfItsBytes()
    {
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();
        await RequestResetAsync(client, Address);
        var token = TokenOf(await Host.Emails.WaitForAsync<PasswordResetEmail>());

        await using var db = IdentityAccess();
        var row = await db.AccountTokens.AsNoTracking().SingleAsync(Cancellation);

        row.TokenHash.ShouldBe(SHA256.HashData(Base64Url.DecodeFromChars(token)));
        row.ExpiresAt.ShouldBe(row.CreatedAt.AddHours(2));
    }

    [Theory(DisplayName = "INV:SEC-11 " + nameof(Complete_PasswordOutsideTheRule_IsRefusedAndTheLinkStaysValid))]
    [Trait("Invariant", "SEC-11")]
    [InlineData(11)]
    [InlineData(129)]
    public async Task Complete_PasswordOutsideTheRule_IsRefusedAndTheLinkStaysValid(int length)
    {
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();
        await RequestResetAsync(client, Address);
        var token = TokenOf(await Host.Emails.WaitForAsync<PasswordResetEmail>());

        using var refused = await CompleteAsync(client, token, new string('ä', length));
        using var accepted = await CompleteAsync(client, token, NewPassword);

        refused.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ErrorKeysOf(refused)).ShouldBe(["newPassword"]);
        accepted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("kein-token")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public async Task Complete_UnknownOrMalformedToken_Returns400AtToken(string token)
    {
        using var client = CreateClient();

        using var response = await CompleteAsync(client, token, NewPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ErrorKeysOf(response)).ShouldBe(["token"]);
    }

    [Fact]
    public async Task Complete_OneLinkUsed_RevokesTheOtherLinksOfTheUser()
    {
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();
        await RequestResetAsync(client, Address);
        var first = TokenOf(await Host.Emails.WaitForAsync<PasswordResetEmail>());
        await RequestResetAsync(client, Address);
        var second = TokenOf(await Host.Emails.WaitForAsync<PasswordResetEmail>(m => TokenOf(m) != first));

        using var completed = await CompleteAsync(client, second, NewPassword);
        using var older = await CompleteAsync(client, first, "noch-ein-passwort");

        completed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        older.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ErrorKeysOf(older)).ShouldBe(["token"]);
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Login_FiveFailedAttempts_LockTheAccountForFifteenMinutesOnTheClock))]
    [Trait("Invariant", "SEC-11")]
    public async Task Login_FiveFailedAttempts_LockTheAccountForFifteenMinutesOnTheClock()
    {
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();
        using var wrong = await LoginAsync(client, "falsches-passwort");
        var wrongBody = await WithoutTraceIdAsync(wrong);
        for (var i = 1; i < 5; i++)
        {
            using var again = await LoginAsync(client, "falsches-passwort");
            again.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        // Locked: the right password answers exactly like a wrong one (decision S5-13 1 (a), #319).
        _clock.Advance(TimeSpan.FromMinutes(15) - TimeSpan.FromSeconds(1));
        using var locked = await LoginAsync(client, Password);
        locked.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await WithoutTraceIdAsync(locked)).ShouldBe(wrongBody);

        _clock.Advance(TimeSpan.FromSeconds(2));
        using var free = await LoginAsync(client, Password);
        free.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Login_FourFailedAttempts_DoNotLock))]
    [Trait("Invariant", "SEC-11")]
    public async Task Login_FourFailedAttempts_DoNotLock()
    {
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();
        for (var i = 0; i < 4; i++)
        {
            using var wrong = await LoginAsync(client, "falsches-passwort");
            wrong.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        using var right = await LoginAsync(client, Password);

        right.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Login_SuccessAfterFailures_StartsTheCountAgain()
    {
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();
        for (var i = 0; i < 4; i++)
        {
            using var wrong = await LoginAsync(client, "falsches-passwort");
        }

        using (var right = await LoginAsync(client, Password))
        {
            right.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        using var other = CreateClient();
        for (var i = 0; i < 4; i++)
        {
            using var wrong = await LoginAsync(other, "falsches-passwort");
        }

        using var stillFree = await LoginAsync(other, Password);
        stillFree.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Complete_LockedAccount_EndsTheLockout()
    {
        await CreateUserAsync(Address, confirmed: true);
        using var client = CreateClient();
        for (var i = 0; i < 5; i++)
        {
            using var wrong = await LoginAsync(client, "falsches-passwort");
        }

        await RequestResetAsync(client, Address);
        using var completed = await CompleteAsync(client, TokenOf(await Host.Emails.WaitForAsync<PasswordResetEmail>()), NewPassword);
        using var login = await LoginAsync(client, NewPassword);

        completed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        login.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    private HttpClient CreateClient()
    {
        var cookies = new CookieContainer();
        return Host.CreateDefaultClient(IntegrationTest.BaseAddress, new AntiforgeryHandler(cookies), new CookieContainerHandler(cookies));
    }

    private async Task CreateUserAsync(string email, bool confirmed, bool deleted = false)
    {
        using var scope = Host.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var user = new User { UserName = email, Email = email, EmailConfirmed = confirmed, IsDeleted = deleted };
        (await users.CreateAsync(user, Password)).Succeeded.ShouldBeTrue();
    }

    private async Task<User?> FindUserAsync(string email)
    {
        using var scope = Host.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager<User>>().FindByEmailAsync(email);
    }

    private static Task<HttpResponseMessage> RequestResetAsync(HttpClient client, string email) =>
        client.PostAsJsonAsync(RequestReset, new { email }, Cancellation);

    private static Task<HttpResponseMessage> CompleteAsync(HttpClient client, string token, string newPassword) =>
        client.PostAsJsonAsync(CompleteReset, new { token, newPassword }, Cancellation);

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client, string password) =>
        client.PostAsJsonAsync(Login, new { email = Address, password }, Cancellation);

    private static string TokenOf(PasswordResetEmail mail) =>
        QueryHelpers.ParseQuery(mail.Link.Query)["token"].ToString();

    private IdentityAccessDbContext IdentityAccess() =>
        new(new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(_connectionString!).Options, FixedOrganizationContext.None);

    /// <summary>The answer without its <c>traceId</c>, which differs per request.</summary>
    private static async Task<string> WithoutTraceIdAsync(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return JsonSerializer.Serialize(body.RootElement.EnumerateObject()
            .Where(p => p.Name != "traceId")
            .ToDictionary(p => p.Name, p => p.Value));
    }

    private static async Task<string> ProblemTypeOf(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return body.RootElement.GetProperty("type").GetString()!;
    }

    private static async Task<string[]> ErrorKeysOf(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return [.. body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal)];
    }
}
