// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Buffers.Text;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using API.ProcureAccess.Organizations;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MODELS.ProcureAccess.Entities;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Organizations;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Email;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Organizations;

/// <summary>
/// Invitations by e-mail (S5-17a; SPEC R-ORG-02, R-ORG-03, §8.9; ARCHITECTURE §5.2, §9.1, C72, C80):
/// inviting with the e-mail, replacing, the pending list, revoking, the page's read by token and
/// accepting — single use, 14 days 🔧 on the fake clock, stored hashed (SEC-11), confirmed address
/// (SEC-10) and bound to the invited address. Every test starts from the Tenants fixture with the
/// legacy accounts of its users (all confirmed except <c>reader-b</c>), so the addresses and the gate
/// are real. The authorisation matrix of the Owner endpoints is <c>AuthorizationMatrixTests</c>
/// (TEN-05), the rate limit <c>RateLimitTests</c> (API-05).
/// </summary>
public sealed class InvitationEndpointsTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Organizations = "/api/v1/organizations";
    private const string View = "/api/v1/invitations/view";
    private const string Accept = "/api/v1/invitations/accept";

    private Tenants? _tenants;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private CapturingEmailSender Emails => Api.Factory.Emails;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        Emails.Clear();
        _tenants = Tenants.Build(Clock.GetUtcNow());
        await StoreAsync(_tenants.Entities);
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => AccountOf(u, confirmed: u != TestUsers.ReaderB))]);
    }

    public ValueTask DisposeAsync()
    {
        Emails.Clear();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task Invite_Owner_Returns201AndSendsTheLinkToTheInvitedAddress()
    {
        using var owner = CreateClient(TestUsers.OwnerA);

        using var response = await InviteAsync(owner, "  neu@example.test  ", "editor");

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = (await response.Content.ReadFromJsonAsync<CreatedInvitation>(Cancellation)).ShouldNotBeNull();
        created.ValidDays.ShouldBe(14);
        response.Headers.Location.ShouldNotBeNull().ToString().ShouldBe(InvitationsPath(Fixture.A.Id));

        var mail = Emails.Sent.OfType<OrganisationInvitationEmail>().ShouldHaveSingleItem();
        mail.Recipient.ShouldBe("neu@example.test");
        mail.OrganisationName.ShouldBe("Organisation A");
        mail.Role.ShouldBe(InvitationRole.Editor);
        mail.ValidDays.ShouldBe(14);
        var baseUrl = Api.Factory.Services.GetRequiredService<IConfiguration>()["App:BaseUrl"]!.TrimEnd('/');
        mail.Link.GetLeftPart(UriPartial.Path).ShouldBe(baseUrl + "/organisation/einladung");
        TokenOf(mail).ShouldNotBeNullOrEmpty();

        var pending = await PendingAsync(owner);
        pending.ShouldHaveSingleItem().ShouldBe(
            new PendingInvitationResponse(created.Id, "neu@example.test", "editor", Clock.GetUtcNow().AddDays(14)));
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Invitation_IsStoredOnlyAsTheSha256OfItsToken))]
    [Trait("Invariant", "SEC-11")]
    public async Task Invitation_IsStoredOnlyAsTheSha256OfItsToken()
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        (await InviteAsync(owner, "neu@example.test", "reader")).Dispose();
        var token = TokenOf(Emails.Sent.OfType<OrganisationInvitationEmail>().Single());

        await using var db = IdentityAccess();
        var row = await db.OrganizationInvitations.IgnoreQueryFilters().AsNoTracking().SingleAsync(Cancellation);

        row.TokenHash.ShouldBe(SHA256.HashData(Base64Url.DecodeFromChars(token)));
        row.ExpiresAt.ShouldBe(row.CreatedAt.AddDays(14));
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Invite_SameAddressAgain_ReplacesTheOpenInvitationAndEndsItsLink))]
    [Trait("Invariant", "SEC-11")]
    public async Task Invite_SameAddressAgain_ReplacesTheOpenInvitationAndEndsItsLink()
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        (await InviteAsync(owner, TestUsers.Member.Email, "reader")).Dispose();
        var first = TokenOf(Emails.Sent.OfType<OrganisationInvitationEmail>().Single());

        // Another spelling of the same address counts as the same address.
        using var again = await InviteAsync(owner, TestUsers.Member.Email.ToUpperInvariant(), "editor");

        again.StatusCode.ShouldBe(HttpStatusCode.Created);
        var second = TokenOf(Emails.Sent.OfType<OrganisationInvitationEmail>().Last());
        second.ShouldNotBe(first);
        (await PendingAsync(owner)).ShouldHaveSingleItem().Role.ShouldBe("editor");

        using var member = CreateClient(TestUsers.Member);
        using var old = await AcceptAsync(member, first);
        old.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemOf(old)).ErrorKeys.ShouldBe(["token"]);
        using var current = await AcceptAsync(member, second);
        current.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await RoleOfAsync(Fixture.A.Id, TestUsers.Member)).ShouldBe(OrganizationRole.Editor);
    }

    [Theory]
    [InlineData("editor-a@example.test")]
    [InlineData("EDITOR-A@example.test")]
    public async Task Invite_AddressOfAMember_Returns400AtEmailAndSendsNothing(string email)
    {
        using var owner = CreateClient(TestUsers.OwnerA);

        using var response = await InviteAsync(owner, email, "reader");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ProblemOf(response);
        problem.Type.ShouldEndWith("/problems/" + ProblemTypes.Validation);
        problem.ErrorKeys.ShouldBe(["email"]);
        Emails.Sent.ShouldBeEmpty();
        (await PendingAsync(owner)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Invite_AddressOfAMemberOfAnotherOrganization_IsAccepted()
    {
        using var owner = CreateClient(TestUsers.OwnerA);

        using var response = await InviteAsync(owner, TestUsers.EditorB.Email, "reader");

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Theory]
    [InlineData(null, "reader", "email")]
    [InlineData("   ", "reader", "email")]
    [InlineData("kein-at-zeichen", "reader", "email")]
    [InlineData("Anna <anna@example.test>", "reader", "email")]
    [InlineData("long", "reader", "email")]
    [InlineData("neu@example.test", null, "role")]
    [InlineData("neu@example.test", "Owner", "role")]
    [InlineData("neu@example.test", "admin", "role")]
    [InlineData(null, null, "email,role")]
    public async Task Invite_InvalidBody_Returns400AtTheFieldAndStoresNothing(string? email, string? role, string expectedKeys)
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        var sent = email == "long" ? new string('a', 245) + "@example.test" : email;

        using var response = await owner.PostAsJsonAsync(InvitationsPath(Fixture.A.Id), new { email = sent, role }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemOf(response)).ErrorKeys.ShouldBe(expectedKeys.Split(','));
        Emails.Sent.ShouldBeEmpty();
        (await CountInvitationsAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Invite_EmailCannotBeSent_Returns503AndKeepsTheInvitation()
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        Emails.Fail = true;

        using var response = await InviteAsync(owner, "neu@example.test", "reader");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        (await ProblemOf(response)).Type.ShouldEndWith("/problems/" + ProblemTypes.EmailDelivery);
        Emails.Fail = false;
        (await PendingAsync(owner)).ShouldHaveSingleItem().Email.ShouldBe("neu@example.test");
    }

    [Fact]
    public async Task Pending_ListsOnlyOpenInvitationsOfTheOrganizationByAddress()
    {
        var now = Clock.GetUtcNow();
        var accepted = AnInvitation(Fixture.A.Id, "angenommen@example.test", now);
        accepted.Accept(now);
        await StoreAsync(
            AnInvitation(Fixture.A.Id, "zora@example.test", now),
            AnInvitation(Fixture.A.Id, "Bert@example.test", now),
            AnInvitation(Fixture.A.Id, "abgelaufen@example.test", now.AddDays(-14)),
            accepted,
            AnInvitation(Fixture.B.Id, "andere@example.test", now));
        using var owner = CreateClient(TestUsers.OwnerA);

        var pending = await PendingAsync(owner);

        pending.Select(i => i.Email).ShouldBe(["Bert@example.test", "zora@example.test"]);
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Revoke_OpenInvitation_Returns204AndItsLinkIsInvalid))]
    [Trait("Invariant", "SEC-11")]
    public async Task Revoke_OpenInvitation_Returns204AndItsLinkIsInvalid()
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        using var invited = await InviteAsync(owner, TestUsers.Member.Email, "reader");
        var created = (await invited.Content.ReadFromJsonAsync<CreatedInvitation>(Cancellation)).ShouldNotBeNull();
        var token = TokenOf(Emails.Sent.OfType<OrganisationInvitationEmail>().Single());

        using var revoked = await owner.DeleteAsync($"{InvitationsPath(Fixture.A.Id)}/{created.Id}", Cancellation);
        using var again = await owner.DeleteAsync($"{InvitationsPath(Fixture.A.Id)}/{created.Id}", Cancellation);

        revoked.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        again.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await PendingAsync(owner)).ShouldBeEmpty();
        using var member = CreateClient(TestUsers.Member);
        using var accept = await AcceptAsync(member, token);
        accept.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await RoleOfAsync(Fixture.A.Id, TestUsers.Member)).ShouldBeNull();
    }

    [Fact]
    public async Task Revoke_InvitationOfAnotherOrganizationOnTheOwnRoute_Returns404AndKeepsIt()
    {
        var foreign = AnInvitation(Fixture.B.Id, "andere@example.test", Clock.GetUtcNow());
        await StoreAsync(foreign);
        using var owner = CreateClient(TestUsers.OwnerA);

        using var response = await owner.DeleteAsync($"{InvitationsPath(Fixture.A.Id)}/{foreign.Id}", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CountInvitationsAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task View_Anonymous_ShowsOrganizationAndRoleButNoAddress()
    {
        var token = await InviteAndTakeTokenAsync("neu@example.test", "owner");
        using var anonymous = CreateClient();

        using var response = await anonymous.PostAsJsonAsync(View, new { token }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<InvitationViewResponse>(Cancellation)).ShouldBe(
            new InvitationViewResponse("Organisation A", "owner", null, null, false, null));
        (await response.Content.ReadAsStringAsync(Cancellation)).ShouldNotContain("neu@example.test");
    }

    [Fact]
    public async Task View_SignedInWithTheInvitedAddress_ConfirmsTheMatch()
    {
        var token = await InviteAndTakeTokenAsync(TestUsers.Member.Email, "reader");
        using var member = CreateClient(TestUsers.Member);

        var view = await ViewAsync(member, token);

        view.ShouldBe(new InvitationViewResponse("Organisation A", "reader", true, null, false, null));
    }

    [Fact]
    public async Task View_SignedInWithAnotherAddress_NamesTheInvitedAddress()
    {
        var token = await InviteAndTakeTokenAsync("neu@example.test", "reader");
        using var other = CreateClient(TestUsers.OwnerB);

        var view = await ViewAsync(other, token);

        view.ShouldBe(new InvitationViewResponse("Organisation A", "reader", false, "neu@example.test", false, null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("kein-token")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public async Task View_UnknownToken_Returns400AtToken(string? token)
    {
        using var anonymous = CreateClient();

        using var response = await anonymous.PostAsJsonAsync(View, new { token }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemOf(response)).ErrorKeys.ShouldBe(["token"]);
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Accept_InvitedConfirmedUser_BecomesMemberWithTheChosenRole))]
    [Trait("Invariant", "SEC-11")]
    public async Task Accept_InvitedConfirmedUser_BecomesMemberWithTheChosenRole()
    {
        var invitedAt = Clock.GetUtcNow();
        var token = await InviteAndTakeTokenAsync(TestUsers.Member.Email, "editor");
        Clock.Advance(TimeSpan.FromDays(3));
        using var member = CreateClient(TestUsers.Member);

        using var response = await AcceptAsync(member, token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<AcceptedInvitation>(Cancellation)).ShouldBe(new AcceptedInvitation(Fixture.A.Id));
        await using var db = IdentityAccess();
        var membership = await db.OrganizationMemberships.IgnoreQueryFilters()
                                 .SingleAsync(m => m.OrganizationId == Fixture.A.Id && m.UserId == TestUsers.Member.Id, Cancellation);
        membership.Role.ShouldBe(OrganizationRole.Editor);
        membership.InvitedAt.ShouldBe(invitedAt);
        membership.AcceptedAt.ShouldBe(Clock.GetUtcNow());

        // A member from the next request on: the members page answers.
        using var members = await member.GetAsync($"{Organizations}/{Fixture.A.Id}/members", Cancellation);
        members.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Accept_LinkUsedBefore_RefusesEveryoneButTheNewMember))]
    [Trait("Invariant", "SEC-11")]
    public async Task Accept_LinkUsedBefore_RefusesEveryoneButTheNewMember()
    {
        var token = await InviteAndTakeTokenAsync(TestUsers.Member.Email, "reader");
        using var member = CreateClient(TestUsers.Member);
        (await AcceptAsync(member, token)).StatusCode.ShouldBe(HttpStatusCode.OK);

        // A second click by the new member leads to the organisation (owner decision S5-17 D7 (a)).
        using var again = await AcceptAsync(member, token);
        var view = await ViewAsync(member, token);
        // Anyone else finds a used link, whatever their address.
        using var other = CreateClient(TestUsers.OwnerB);
        using var refused = await AcceptAsync(other, token);
        using var anonymousView = await CreateClient().PostAsJsonAsync(View, new { token }, Cancellation);

        again.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await again.Content.ReadFromJsonAsync<AcceptedInvitation>(Cancellation)).ShouldBe(new AcceptedInvitation(Fixture.A.Id));
        view.AlreadyMember.ShouldBeTrue();
        view.OrganizationId.ShouldBe(Fixture.A.Id);
        refused.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemOf(refused)).ErrorKeys.ShouldBe(["token"]);
        anonymousView.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await RoleOfAsync(Fixture.A.Id, TestUsers.OwnerB)).ShouldBeNull();
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Accept_TwoRequestsAtOnce_CreateOneMembership))]
    [Trait("Invariant", "SEC-11")]
    public async Task Accept_TwoRequestsAtOnce_CreateOneMembership()
    {
        var token = await InviteAndTakeTokenAsync(TestUsers.Member.Email, "reader");
        using var first = CreateClient(TestUsers.Member);
        using var second = CreateClient(TestUsers.Member);

        var answers = await Task.WhenAll(AcceptAsync(first, token), AcceptAsync(second, token));

        try
        {
            answers.Count(a => a.StatusCode == HttpStatusCode.OK).ShouldBeGreaterThanOrEqualTo(1);
            answers.ShouldAllBe(a => a.StatusCode == HttpStatusCode.OK || a.StatusCode == HttpStatusCode.BadRequest);
            await using var db = IdentityAccess();
            (await db.OrganizationMemberships.IgnoreQueryFilters()
                     .CountAsync(m => m.OrganizationId == Fixture.A.Id && m.UserId == TestUsers.Member.Id, Cancellation)).ShouldBe(1);
        }
        finally
        {
            foreach (var answer in answers)
            {
                answer.Dispose();
            }
        }
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Accept_JustBeforeFourteenDays_IsAccepted))]
    [Trait("Invariant", "SEC-11")]
    public async Task Accept_JustBeforeFourteenDays_IsAccepted()
    {
        var token = await InviteAndTakeTokenAsync(TestUsers.Member.Email, "reader");
        Clock.Advance(TimeSpan.FromDays(14) - TimeSpan.FromSeconds(1));
        using var member = CreateClient(TestUsers.Member);

        using var response = await AcceptAsync(member, token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "INV:SEC-11 " + nameof(Accept_AfterFourteenDays_IsRefusedAndTheLinkShowsNothing))]
    [Trait("Invariant", "SEC-11")]
    public async Task Accept_AfterFourteenDays_IsRefusedAndTheLinkShowsNothing()
    {
        var token = await InviteAndTakeTokenAsync(TestUsers.Member.Email, "reader");
        Clock.Advance(TimeSpan.FromDays(14));
        using var member = CreateClient(TestUsers.Member);

        using var response = await AcceptAsync(member, token);
        using var view = await member.PostAsJsonAsync(View, new { token }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemOf(response)).ErrorKeys.ShouldBe(["token"]);
        view.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await RoleOfAsync(Fixture.A.Id, TestUsers.Member)).ShouldBeNull();
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Accept_UnconfirmedAddress_Returns403EmailUnconfirmedAndTheInvitationStaysOpen))]
    [Trait("Invariant", "SEC-10")]
    public async Task Accept_UnconfirmedAddress_Returns403EmailUnconfirmedAndTheInvitationStaysOpen()
    {
        var token = await InviteAndTakeTokenAsync(TestUsers.ReaderB.Email, "reader");
        using var unconfirmed = CreateClient(TestUsers.ReaderB);

        using var response = await AcceptAsync(unconfirmed, token);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ProblemOf(response)).Type.ShouldEndWith("/problems/" + ProblemTypes.EmailUnconfirmed);
        (await RoleOfAsync(Fixture.A.Id, TestUsers.ReaderB)).ShouldBeNull();
        using var owner = CreateClient(TestUsers.OwnerA);
        (await PendingAsync(owner)).ShouldHaveSingleItem();
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Accept_AfterConfirmingTheAddress_Succeeds))]
    [Trait("Invariant", "SEC-10")]
    public async Task Accept_AfterConfirmingTheAddress_Succeeds()
    {
        var token = await InviteAndTakeTokenAsync(TestUsers.ReaderB.Email, "reader");
        using var invitee = CreateClient(TestUsers.ReaderB);
        (await AcceptAsync(invitee, token)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        await ConfirmAsync(TestUsers.ReaderB);
        using var response = await AcceptAsync(invitee, token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await RoleOfAsync(Fixture.A.Id, TestUsers.ReaderB)).ShouldBe(OrganizationRole.Reader);
    }

    [Fact]
    public async Task Accept_OtherAddress_Returns403InvitationMismatchWithTheInvitedAddressAndStaysOpen()
    {
        var token = await InviteAndTakeTokenAsync("neu@example.test", "reader");
        using var other = CreateClient(TestUsers.Member);

        using var response = await AcceptAsync(other, token);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        body.RootElement.GetProperty("type").GetString().ShouldEndWith("/problems/" + ProblemTypes.InvitationMismatch);
        body.RootElement.GetProperty("invitedEmail").GetString().ShouldBe("neu@example.test");
        (await RoleOfAsync(Fixture.A.Id, TestUsers.Member)).ShouldBeNull();
        using var owner = CreateClient(TestUsers.OwnerA);
        (await PendingAsync(owner)).ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Accept_Anonymous_Returns401()
    {
        var token = await InviteAndTakeTokenAsync(TestUsers.Member.Email, "reader");
        using var anonymous = CreateClient();

        using var response = await AcceptAsync(anonymous, token);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Cleanup_ExpiredInvitations_AreDeletedAndValidOnesStay()
    {
        var now = Clock.GetUtcNow();
        var acceptedValid = AnInvitation(Fixture.A.Id, "angenommen@example.test", now.AddDays(-1));
        acceptedValid.Accept(now);
        var acceptedExpired = AnInvitation(Fixture.A.Id, "alt-angenommen@example.test", now.AddDays(-20));
        acceptedExpired.Accept(now.AddDays(-19));
        var open = AnInvitation(Fixture.A.Id, "offen@example.test", now);
        await StoreAsync(open, acceptedValid, acceptedExpired, AnInvitation(Fixture.B.Id, "abgelaufen@example.test", now.AddDays(-14)));

        await using var db = IdentityAccess();
        var deleted = await InvitationCleanupService.DeleteExpiredAsync(db, now, Cancellation);

        deleted.ShouldBe(2);
        (await db.OrganizationInvitations.IgnoreQueryFilters().Select(i => i.Id).ToListAsync(Cancellation))
            .ShouldBe([open.Id, acceptedValid.Id], ignoreOrder: true);
    }

    [Fact]
    public async Task CleanupJob_NextRunOnTheClock_DeletesAnInvitationThatExpired()
    {
        var invitation = AnInvitation(Fixture.A.Id, "bald-abgelaufen@example.test", Clock.GetUtcNow().AddDays(-14).AddMinutes(30));
        await StoreAsync(invitation);

        // The job runs every hour 🔧; the invitation expires within it.
        Clock.Advance(TimeSpan.FromHours(1));

        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (await CountInvitationsAsync() > 0)
        {
            DateTime.UtcNow.ShouldBeLessThan(deadline, "the clean-up job did not delete the expired invitation");
            await Task.Delay(50, Cancellation);
        }
    }

    private static string InvitationsPath(Guid organizationId) => $"{Organizations}/{organizationId}/invitations";

    private Task<HttpResponseMessage> InviteAsync(HttpClient client, string email, string role) =>
        client.PostAsJsonAsync(InvitationsPath(Fixture.A.Id), new { email, role }, Cancellation);

    private static Task<HttpResponseMessage> AcceptAsync(HttpClient client, string token) =>
        client.PostAsJsonAsync(Accept, new { token }, Cancellation);

    private static async Task<InvitationViewResponse> ViewAsync(HttpClient client, string token)
    {
        using var response = await client.PostAsJsonAsync(View, new { token }, Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<InvitationViewResponse>(Cancellation)).ShouldNotBeNull();
    }

    private async Task<List<PendingInvitationResponse>> PendingAsync(HttpClient owner) =>
        (await owner.GetFromJsonAsync<List<PendingInvitationResponse>>(InvitationsPath(Fixture.A.Id), Cancellation)).ShouldNotBeNull();

    /// <summary>Owner A invites <paramref name="email"/> and returns the token of the e-mail's link.</summary>
    private async Task<string> InviteAndTakeTokenAsync(string email, string role)
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        using var response = await InviteAsync(owner, email, role);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return TokenOf(Emails.Sent.OfType<OrganisationInvitationEmail>().Last());
    }

    private static string TokenOf(OrganisationInvitationEmail mail) =>
        QueryHelpers.ParseQuery(mail.Link.Query)["token"].ToString();

    /// <summary>An open invitation with a random token hash, stored directly.</summary>
    private static OrganizationInvitation AnInvitation(Guid organizationId, string email, DateTimeOffset createdAt) =>
        OrganizationInvitation.Create(
            organizationId, email, OrganizationRole.Reader, RandomNumberGenerator.GetBytes(32), createdAt, TimeSpan.FromDays(14));

    /// <summary>The legacy account of a fixture user, so the addresses and the confirmation gate are real.</summary>
    private static User AccountOf(TestUser user, bool confirmed) => new()
    {
        Id = user.Id,
        UserName = user.Email,
        NormalizedUserName = user.Email.ToUpperInvariant(),
        Email = user.Email,
        NormalizedEmail = user.Email.ToUpperInvariant(),
        EmailConfirmed = confirmed,
        SecurityStamp = Guid.NewGuid().ToString(),
    };

    private async Task ConfirmAsync(TestUser user)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<User>>();
        var account = (await users.FindByIdAsync(user.Id)).ShouldNotBeNull();
        account.EmailConfirmed = true;
        (await users.UpdateAsync(account)).Succeeded.ShouldBeTrue();
    }

    private async Task<OrganizationRole?> RoleOfAsync(Guid organizationId, TestUser user)
    {
        await using var db = IdentityAccess();
        return await db.OrganizationMemberships.IgnoreQueryFilters()
                       .Where(m => m.OrganizationId == organizationId && m.UserId == user.Id)
                       .Select(m => (OrganizationRole?)m.Role)
                       .SingleOrDefaultAsync(Cancellation);
    }

    private async Task<int> CountInvitationsAsync()
    {
        await using var db = IdentityAccess();
        return await db.OrganizationInvitations.IgnoreQueryFilters().CountAsync(Cancellation);
    }

    /// <summary>A context of its own outside any request, so tests read past the filter.</summary>
    private IdentityAccessDbContext IdentityAccess() =>
        new(new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(Api.ConnectionString).Options, FixedOrganizationContext.None);

    private static async Task<(string Type, string[] ErrorKeys)> ProblemOf(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        var keys = body.RootElement.TryGetProperty("errors", out var errors)
            ? errors.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal).ToArray()
            : [];
        return (body.RootElement.GetProperty("type").GetString()!, keys);
    }
}
