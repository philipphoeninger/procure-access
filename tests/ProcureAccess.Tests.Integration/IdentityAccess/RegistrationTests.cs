// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Buffers.Text;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using MODELS.ProcureAccess.Entities;
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Email;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// Registration, address confirmation and the confirmation gate (S5-12a; SPEC R-AUTH-01, R-AUTH-02,
/// §8.8; ARCHITECTURE §9.1, C72): <c>POST /api/v1/auth/register</c>, <c>/confirm-email</c>,
/// <c>/confirm-email/resend</c> and the policy <c>ConfirmedEmail</c> (SEC-10) on a test endpoint,
/// because no gated action exists yet. Runs on a host with the API's own authentication
/// (<c>realAuthentication</c>) on a database of this class that was only migrated, so it has no
/// legacy <c>Member</c> role (#301), with a clock of its own.
/// </summary>
public sealed class RegistrationTests(ApiFixture api) : IAsyncLifetime
{
    private const string Register = "/api/v1/auth/register";
    private const string Confirm = "/api/v1/auth/confirm-email";
    private const string Resend = "/api/v1/auth/confirm-email/resend";
    private const string Login = "/api/v1/auth/login";
    private const string Address = "neu@example.test";
    private const string Password = "zwoelf-zeichen";

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
            configureServices: services => services.AddControllers().AddApplicationPart(typeof(ConfirmedEmailProbeController).Assembly),
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
            new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(_connectionString!).Options);
        await context.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task Register_NewAddressOnAMigratedDatabase_CreatesAnUnconfirmedAccountWithoutRoleAndSendsTheLink()
    {
        using var client = CreateClient();

        using var response = await RegisterAsync(client, Address);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var user = await FindUserAsync(Address);
        user.ShouldNotBeNull().EmailConfirmed.ShouldBeFalse();
        (await RolesOfAsync(user)).ShouldBeEmpty(); // #301: no Member role, so no failure without its row

        var mail = Host.Emails.Sent.ShouldHaveSingleItem().ShouldBeOfType<AddressConfirmationEmail>();
        mail.Recipient.ShouldBe(Address);
        mail.ValidHours.ShouldBe(24);
        mail.Link.GetLeftPart(UriPartial.Path).ShouldBe("http://procureaccess.tech:4200/registrieren/bestaetigen");
        TokenOf(mail).ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_KnownAddress_AnswersAsForANewOneAndSendsAlreadyRegistered()
    {
        using var client = CreateClient();
        using var first = await RegisterAsync(client, Address);
        Host.Emails.Clear();

        using var second = await RegisterAsync(client, Address, password: "ein-anderes-passwort");

        second.StatusCode.ShouldBe(first.StatusCode);
        (await second.Content.ReadAsStringAsync(Cancellation)).ShouldBe(await first.Content.ReadAsStringAsync(Cancellation));
        var mail = Host.Emails.Sent.ShouldHaveSingleItem().ShouldBeOfType<AlreadyRegisteredEmail>();
        mail.Recipient.ShouldBe(Address);
        mail.Link.AbsoluteUri.ShouldBe("http://procureaccess.tech:4200/(login:anmelden)");
        (await CountUsersAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task Register_KnownAddressInOtherCase_IsTheSameAccount()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Address);
        Host.Emails.Clear();

        using var response = await RegisterAsync(client, Address.ToUpperInvariant());

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        Host.Emails.Sent.ShouldHaveSingleItem().ShouldBeOfType<AlreadyRegisteredEmail>();
        (await CountUsersAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task Register_AddressTakenOnlyAsAUserName_AnswersAsForANewOne()
    {
        using (var scope = Host.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = new User { UserName = Address, Email = "andere@example.test" };
            (await users.CreateAsync(user, Password)).Succeeded.ShouldBeTrue();
        }

        using var client = CreateClient();

        using var response = await RegisterAsync(client, Address);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync(Cancellation)).ShouldBeEmpty();
        (await CountUsersAsync()).ShouldBe(1);
        Host.Emails.Sent.ShouldHaveSingleItem().ShouldBeOfType<AlreadyRegisteredEmail>().Recipient.ShouldBe("andere@example.test");
    }

    [Fact]
    public async Task Register_InvalidFields_Returns400ValidationKeyedByField()
    {
        using var client = CreateClient();

        using var response = await client.PostAsJsonAsync(
            Register, new { email = "keine-adresse", password = new string('x', 11), privacyAcknowledged = false }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemTypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.Validation);
        (await ErrorKeysOf(response)).ShouldBe(["email", "password", "privacyAcknowledged"]);
        (await CountUsersAsync()).ShouldBe(0);
        Host.Emails.Sent.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(11, false)]
    [InlineData(12, true)]
    [InlineData(128, true)]
    [InlineData(129, false)]
    public async Task Register_PasswordLength_AcceptsTwelveTo128Characters(int length, bool accepted)
    {
        using var client = CreateClient();

        using var response = await RegisterAsync(client, $"laenge{length}@example.test", password: new string('ä', length));

        response.StatusCode.ShouldBe(accepted ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest);
        if (!accepted)
        {
            (await ErrorKeysOf(response)).ShouldBe(["password"]);
        }
    }

    [Fact]
    public async Task Register_AddressWithAnApostrophe_IsAccepted()
    {
        using var client = CreateClient();

        using var response = await RegisterAsync(client, "o'brien@example.test");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await FindUserAsync("o'brien@example.test")).ShouldNotBeNull();
    }

    [Fact]
    public async Task Register_MailCannotBeSent_AnswersTheSameAndKeepsTheAccount()
    {
        using var client = CreateClient();
        Host.Emails.Fail = true;

        using var response = await RegisterAsync(client, Address);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await FindUserAsync(Address)).ShouldNotBeNull();
    }

    [Fact]
    public async Task Register_WithoutAntiforgeryToken_Returns400Antiforgery()
    {
        using var client = Host.CreateDefaultClient(IntegrationTest.BaseAddress);

        using var response = await RegisterAsync(client, Address);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemTypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.Antiforgery);
        (await CountUsersAsync()).ShouldBe(0);
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Gate_UnconfirmedAccount_SignsInButGetsEmailUnconfirmedUntilConfirmed))]
    [Trait("Invariant", "SEC-10")]
    public async Task Gate_UnconfirmedAccount_SignsInButGetsEmailUnconfirmedUntilConfirmed()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Address);

        using var login = await LoginAsync(client);
        login.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var refused = await client.PostAsync(ConfirmedEmailProbeController.Path, content: null, Cancellation);
        refused.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        refused.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        (await ProblemTypeOf(refused)).ShouldEndWith("/problems/" + ProblemTypes.EmailUnconfirmed);

        // Confirmed in another browser (no session there): the open session passes at once.
        using var other = CreateClient();
        using var confirmed = await ConfirmAsync(other, TokenOf(SingleConfirmation()));
        confirmed.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var allowed = await client.PostAsync(ConfirmedEmailProbeController.Path, content: null, Cancellation);
        allowed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Gate_Anonymous_Returns401))]
    [Trait("Invariant", "SEC-10")]
    public async Task Gate_Anonymous_Returns401()
    {
        using var client = CreateClient();

        using var response = await client.PostAsync(ConfirmedEmailProbeController.Path, content: null, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Confirm_TokenUsedTwice_IsRedeemedOnceAndTheSecondClickShowsConfirmed))]
    [Trait("Invariant", "SEC-10")]
    public async Task Confirm_TokenUsedTwice_IsRedeemedOnceAndTheSecondClickShowsConfirmed()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Address);
        var token = TokenOf(SingleConfirmation());

        using var first = await ConfirmAsync(client, token);
        var usedAt = await UsedAtAsync(token);
        _clock.Advance(TimeSpan.FromMinutes(5));
        using var second = await ConfirmAsync(client, token);

        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        second.StatusCode.ShouldBe(HttpStatusCode.NoContent); // owner decision S5-12 4 (b)
        usedAt.ShouldNotBeNull();
        (await UsedAtAsync(token)).ShouldBe(usedAt);
        (await FindUserAsync(Address)).ShouldNotBeNull().EmailConfirmed.ShouldBeTrue();
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Confirm_TokenOlderThan24Hours_IsRefusedAndTheAddressStaysUnconfirmed))]
    [Trait("Invariant", "SEC-10")]
    public async Task Confirm_TokenOlderThan24Hours_IsRefusedAndTheAddressStaysUnconfirmed()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Address);
        var token = TokenOf(SingleConfirmation());

        _clock.Advance(TimeSpan.FromHours(24));
        using var expired = await ConfirmAsync(client, token);

        expired.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ErrorKeysOf(expired)).ShouldBe(["token"]);
        (await FindUserAsync(Address)).ShouldNotBeNull().EmailConfirmed.ShouldBeFalse();
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Confirm_TokenJustBefore24Hours_IsAccepted))]
    [Trait("Invariant", "SEC-10")]
    public async Task Confirm_TokenJustBefore24Hours_IsAccepted()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Address);
        var token = TokenOf(SingleConfirmation());

        _clock.Advance(TimeSpan.FromHours(24) - TimeSpan.FromSeconds(1));
        using var response = await ConfirmAsync(client, token);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Redeem_TwoConcurrentCalls_OnlyOneRedeems))]
    [Trait("Invariant", "SEC-10")]
    public async Task Redeem_TwoConcurrentCalls_OnlyOneRedeems()
    {
        IssuedAccountToken issued;
        using (var scope = Host.Services.CreateScope())
        {
            issued = await scope.ServiceProvider.GetRequiredService<IAccountTokens>()
                .IssueAsync("user-of-a-race", AccountTokenPurpose.EmailConfirmation, Cancellation);
        }

        var outcomes = await Task.WhenAll(Enumerable.Range(0, 2).Select(async _ =>
        {
            using var scope = Host.Services.CreateScope();
            var tokens = scope.ServiceProvider.GetRequiredService<IAccountTokens>();
            return (await tokens.RedeemAsync(issued.Token, AccountTokenPurpose.EmailConfirmation, Cancellation)).Outcome;
        }));

        outcomes.Order().ShouldBe([AccountTokenOutcome.Redeemed, AccountTokenOutcome.AlreadyUsed]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("kein-token")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public async Task Confirm_UnknownOrMalformedToken_Returns400AtToken(string token)
    {
        using var client = CreateClient();

        using var response = await ConfirmAsync(client, token);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemTypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.Validation);
        (await ErrorKeysOf(response)).ShouldBe(["token"]);
    }

    [Fact]
    public async Task Token_IsStoredOnlyAsTheSha256OfItsBytes()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Address);
        var token = TokenOf(SingleConfirmation());

        await using var db = IdentityAccess();
        var row = await db.AccountTokens.AsNoTracking().SingleAsync(Cancellation);

        row.TokenHash.ShouldBe(SHA256.HashData(Base64Url.DecodeFromChars(token)));
        row.ExpiresAt.ShouldBe(row.CreatedAt.AddHours(24));
    }

    [Fact]
    public async Task Resend_SignedInUnconfirmed_SendsANewLinkAndBothLinksWork()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Address);
        var firstToken = TokenOf(SingleConfirmation());
        await LoginAsync(client);
        Host.Emails.Clear();

        using var response = await client.PostAsync(Resend, content: null, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var secondToken = TokenOf(SingleConfirmation());
        secondToken.ShouldNotBe(firstToken);
        using var confirmed = await ConfirmAsync(client, firstToken);
        confirmed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Resend_AlreadyConfirmed_SendsNothing()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Address);
        await ConfirmAsync(client, TokenOf(SingleConfirmation()));
        await LoginAsync(client);
        Host.Emails.Clear();

        using var response = await client.PostAsync(Resend, content: null, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        Host.Emails.Sent.ShouldBeEmpty();
    }

    [Fact]
    public async Task Resend_MailCannotBeSent_Returns503EmailDelivery()
    {
        using var client = CreateClient();
        await RegisterAsync(client, Address);
        await LoginAsync(client);
        Host.Emails.Fail = true;

        using var response = await client.PostAsync(Resend, content: null, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        (await ProblemTypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.EmailDelivery);
    }

    [Fact]
    public async Task Resend_Anonymous_Returns401()
    {
        using var client = CreateClient();

        using var response = await client.PostAsync(Resend, content: null, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        Host.Emails.Sent.ShouldBeEmpty();
    }

    private HttpClient CreateClient()
    {
        var cookies = new CookieContainer();
        return Host.CreateDefaultClient(IntegrationTest.BaseAddress, new AntiforgeryHandler(cookies), new CookieContainerHandler(cookies));
    }

    private static Task<HttpResponseMessage> RegisterAsync(HttpClient client, string email, string password = Password) =>
        client.PostAsJsonAsync(Register, new { email, password, privacyAcknowledged = true }, Cancellation);

    private static Task<HttpResponseMessage> ConfirmAsync(HttpClient client, string token) =>
        client.PostAsJsonAsync(Confirm, new { token }, Cancellation);

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client) =>
        client.PostAsJsonAsync(Login, new { email = Address, password = Password }, Cancellation);

    private AddressConfirmationEmail SingleConfirmation() =>
        Host.Emails.Sent.OfType<AddressConfirmationEmail>().ShouldHaveSingleItem();

    private static string TokenOf(AddressConfirmationEmail mail) =>
        QueryHelpers.ParseQuery(mail.Link.Query)["token"].ToString();

    private async Task<User?> FindUserAsync(string email)
    {
        using var scope = Host.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager<User>>().FindByEmailAsync(email);
    }

    private async Task<IList<string>> RolesOfAsync(User user)
    {
        using var scope = Host.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager<User>>().GetRolesAsync(user);
    }

    private async Task<int> CountUsersAsync()
    {
        using var scope = Host.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager<User>>().Users.CountAsync(Cancellation);
    }

    private async Task<DateTimeOffset?> UsedAtAsync(string token)
    {
        var hash = SHA256.HashData(Base64Url.DecodeFromChars(token));
        await using var db = IdentityAccess();
        return await db.AccountTokens.AsNoTracking().Where(t => t.TokenHash == hash).Select(t => t.UsedAt).SingleAsync(Cancellation);
    }

    private IdentityAccessDbContext IdentityAccess() =>
        new(new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(_connectionString!).Options);

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

/// <summary>
/// A gated action for the tests of the confirmation gate (SEC-10) until the first real one exists
/// (saving a list, S5-20; organisations, S5-16).
/// </summary>
[ApiController]
[Route(Path)]
[Authorize(Policy = IdentityAccessPolicies.ConfirmedEmail)]
public sealed class ConfirmedEmailProbeController : ControllerBase
{
    public const string Path = "/test/confirmed-email";

    [HttpPost]
    public IActionResult Post() => NoContent();
}
