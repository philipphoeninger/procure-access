// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using API.ProcureAccess.Account;
using DAL.ProcureAccess.EFStructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MODELS.ProcureAccess.Entities;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.Configurator.Persistence;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Sharing;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Modules.Platform.Persistence;
using ProcureAccess.Modules.Platform.Sharing;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// The account deletion <c>DELETE /api/v1/account</c> (S5-28a; SPEC R-AUTH-10, §8.13; ARCHITECTURE
/// C79; TEN-11, account part). Every test starts from the Tenants fixture with the legacy accounts
/// of its users stored, one criteria list of organisation A with a share link, and the organisation
/// „Einzelbüro“ whose only member is <see cref="TestUsers.Member"/>, with a list, a share link and
/// an open invitation. The parts of R-AUTH-10 that later steps build (contributor profiles,
/// entries, experience reports, projects) are not here (owner decision S5-28 3).
/// </summary>
public sealed class AccountDeletionTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Account = "/api/v1/account";

    private const string Password = "ein langes Passwort 2026";

    private Tenants? _tenants;
    private Organization? _solo;
    private CriteriaConfiguration? _listOfA;
    private CriteriaConfiguration? _listOfSolo;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private Organization Solo => _solo ?? throw new InvalidOperationException("The fixture is not stored.");

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        var now = Clock.GetUtcNow();
        _tenants = Tenants.Build(now);
        _solo = Organization.Create("Einzelbüro", OrganizationType.Company, TestUsers.Member.Id, now);
        _listOfA = ListOf(_tenants.A.Id, TestUsers.EditorA, "Liste von A");
        _listOfSolo = ListOf(_solo.Id, TestUsers.Member, "Liste des Einzelbüros");

        await StoreAsync(
        [
            .. _tenants.Entities, _solo, OrganizationMembership.Founder(_solo, now),
            OrganizationInvitation.Create(_solo.Id, "neu@example.test", OrganizationRole.Editor, RandomHash(), now, TimeSpan.FromDays(14)),
            OrganizationInvitation.Create(_tenants.A.Id, "auch-neu@example.test", OrganizationRole.Reader, RandomHash(), now, TimeSpan.FromDays(14)),
        ]);
        await StoreAsync(_listOfA, _listOfSolo);
        await StoreAsync(LinkTo(_listOfA), LinkTo(_listOfSolo));
        // Member's address is not confirmed: the deletion needs no confirmed address.
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => LegacyAccounts.Of(u, confirmed: u != TestUsers.Member))]);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:TEN-11 " + nameof(Delete_SoleOwnerOfOrganizationsWithOtherMembers_Returns409NamingThemAndChangesNothing))]
    [Trait("Invariant", "TEN-11")]
    public async Task Delete_SoleOwnerOfOrganizationsWithOtherMembers_Returns409NamingThemAndChangesNothing()
    {
        // Owner A is the only Owner of A (two other members), of a second organisation with one
        // other member, and the only member of a third, which would end with the account.
        var now = Clock.GetUtcNow();
        var second = Organization.Create("Ämterverbund", OrganizationType.PublicBody, TestUsers.OwnerA.Id, now);
        var third = Organization.Create("Nur ich", OrganizationType.Association, TestUsers.OwnerA.Id, now);
        await StoreAsync(
            second, OrganizationMembership.Founder(second, now),
            OrganizationMembership.Create(second.Id, TestUsers.ReaderB.Id, OrganizationRole.Editor, now, now),
            third, OrganizationMembership.Founder(third, now),
            OrganizationInvitation.Create(third.Id, "gast@example.test", OrganizationRole.Reader, RandomHash(), now, TimeSpan.FromDays(14)));
        var before = await CountRowsAsync();
        using var client = CreateClient(TestUsers.OwnerA);

        using var response = await client.DeleteAsync(Account, Cancellation);

        var text = await response.Content.ReadAsStringAsync(Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict, text);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        using var body = JsonDocument.Parse(text);
        body.RootElement.GetProperty("type").GetString().ShouldNotBeNull().ShouldEndWith("/problems/" + ProblemTypes.SoleOwner);
        // By name in German collation („Ä“ with „A“); the organisation without other members is not named.
        body.RootElement.GetProperty(AccountDeletionEndpoints.OrganizationsExtension).EnumerateArray().Select(o => o.GetString())
            .ShouldBe(["Ämterverbund", "Organisation A"]);

        (await CountRowsAsync()).ShouldBe(before);
        var account = await AccountOfAsync(TestUsers.OwnerA);
        account.IsDeleted.ShouldBeFalse();
        account.Email.ShouldBe(TestUsers.OwnerA.Email);
    }

    [Fact(DisplayName = "INV:TEN-11 " + nameof(Delete_OwnerNextToAnotherOwner_Returns204AndKeepsTheOrganizationWithItsRows))]
    [Trait("Invariant", "TEN-11")]
    public async Task Delete_OwnerNextToAnotherOwner_Returns204AndKeepsTheOrganizationWithItsRows()
    {
        await SetRoleAsync(Fixture.A.Id, TestUsers.EditorA, OrganizationRole.Owner);
        using var client = CreateClient(TestUsers.OwnerA);

        using var response = await client.DeleteAsync(Account, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync(Cancellation));
        await using var identity = IdentityAccess();
        (await identity.Organizations.IgnoreQueryFilters().Select(o => o.Name).ToListAsync(Cancellation))
            .ShouldBe(["Organisation A", "Organisation B", "Einzelbüro"], ignoreOrder: true);
        (await identity.OrganizationMemberships.IgnoreQueryFilters().Where(m => m.OrganizationId == Fixture.A.Id).Select(m => m.UserId).ToListAsync(Cancellation))
            .ShouldBe([TestUsers.EditorA.Id, TestUsers.ReaderA.Id], ignoreOrder: true);
        (await identity.OrganizationInvitations.IgnoreQueryFilters().CountAsync(Cancellation)).ShouldBe(2);
        (await CountRowsAsync()).ShouldBe(new Rows(Organizations: 3, Memberships: 6, Invitations: 2, Lists: 2, Links: 2));
        (await AccountOfAsync(TestUsers.OwnerA)).IsDeleted.ShouldBeTrue();
    }

    [Fact(DisplayName = "INV:TEN-11 " + nameof(Delete_LastMember_DeletesTheOrganizationWithItsListsLinksAndInvitationsOnly))]
    [Trait("Invariant", "TEN-11")]
    public async Task Delete_LastMember_DeletesTheOrganizationWithItsListsLinksAndInvitationsOnly()
    {
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.DeleteAsync(Account, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync(Cancellation));
        await using var identity = IdentityAccess();
        (await identity.Organizations.IgnoreQueryFilters().Select(o => o.Id).ToListAsync(Cancellation))
            .ShouldBe([Fixture.A.Id, Fixture.B.Id], ignoreOrder: true);
        (await identity.OrganizationMemberships.IgnoreQueryFilters().AnyAsync(m => m.OrganizationId == Solo.Id || m.UserId == TestUsers.Member.Id, Cancellation))
            .ShouldBeFalse();
        (await identity.OrganizationInvitations.IgnoreQueryFilters().Select(i => i.OrganizationId).ToListAsync(Cancellation))
            .ShouldBe([Fixture.A.Id]);

        // The list of the organisation with its rows, and its link; those of A stay.
        await using var configurator = Configurator();
        (await configurator.CriteriaConfigurations.IgnoreQueryFilters().Select(c => c.Id).ToListAsync(Cancellation))
            .ShouldBe([_listOfA!.Id]);
        (await configurator.Database.SqlQueryRaw<int>(
            "SELECT count(*)::int AS \"Value\" FROM configurator.\"ConfigurationCriteria\"").SingleAsync(Cancellation))
            .ShouldBe(_listOfA.Criteria.Count);
        await using var platform = Platform();
        (await platform.ShareLinks.IgnoreQueryFilters().Select(l => l.ResourceId).ToListAsync(Cancellation))
            .ShouldBe([_listOfA.Id]);
        (await AccountOfAsync(TestUsers.Member)).IsDeleted.ShouldBeTrue();
    }

    [Fact(DisplayName = "INV:TEN-11 " + nameof(Delete_LastMemberAfterTheOtherMemberLeft_DeletesTheOrganization))]
    [Trait("Invariant", "TEN-11")]
    public async Task Delete_LastMemberAfterTheOtherMemberLeft_DeletesTheOrganization()
    {
        // Owner B is refused while two members are left, and deletes the organisation once they are gone.
        using var client = CreateClient(TestUsers.OwnerB);
        using var refused = await client.DeleteAsync(Account, Cancellation);
        refused.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        await using (var change = IdentityAccess())
        {
            await change.OrganizationMemberships.IgnoreQueryFilters()
                        .Where(m => m.OrganizationId == Fixture.B.Id && m.UserId != TestUsers.OwnerB.Id)
                        .ExecuteDeleteAsync(Cancellation);
        }

        using var response = await client.DeleteAsync(Account, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync(Cancellation));
        await using var identity = IdentityAccess();
        (await identity.Organizations.IgnoreQueryFilters().AnyAsync(o => o.Id == Fixture.B.Id, Cancellation)).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Delete_Member_RemovesCredentialsMembershipSettingsAndTokensOfThisUserOnly))]
    public async Task Delete_Member_RemovesCredentialsMembershipSettingsAndTokensOfThisUserOnly()
    {
        var now = Clock.GetUtcNow();
        await StoreAsync(
            UserPreferences.Defaults(TestUsers.EditorA.Id), UserPreferences.Defaults(TestUsers.OwnerA.Id),
            AccountToken.Create(TestUsers.EditorA.Id, AccountTokenPurpose.PasswordReset, RandomHash(), now, TimeSpan.FromHours(2)),
            AccountToken.Create(TestUsers.OwnerA.Id, AccountTokenPurpose.EmailConfirmation, RandomHash(), now, TimeSpan.FromHours(24)));
        var stampBefore = await SetPasswordAsync(TestUsers.EditorA);
        Clock.Advance(TimeSpan.FromMinutes(3));
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.DeleteAsync(Account, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync(Cancellation));
        // As the sign-out: a new antiforgery token, because it is bound to the user (SEC-09).
        response.Headers.GetValues("Set-Cookie").ShouldContain(c => c.StartsWith(AntiforgeryHandler.TokenCookieName + "=", StringComparison.Ordinal));

        // The row stays, soft-deleted, and holds nothing of the user any more (owner decision S5-28 2a).
        var account = await AccountOfAsync(TestUsers.EditorA);
        account.IsDeleted.ShouldBeTrue();
        account.DeletedAt.ShouldBe(Clock.GetUtcNow().UtcDateTime);
        account.PasswordHash.ShouldBeNull();
        account.EmailConfirmed.ShouldBeFalse();
        account.Email.ShouldNotBeNull().ShouldEndWith("@deleted.invalid");
        account.UserName.ShouldBe(account.Email);
        account.NormalizedEmail.ShouldBe(account.Email.ToUpperInvariant());
        account.NormalizedUserName.ShouldBe(account.NormalizedEmail);
        account.SecurityStamp.ShouldNotBe(stampBefore);

        await using var identity = IdentityAccess();
        (await identity.OrganizationMemberships.IgnoreQueryFilters().AnyAsync(m => m.UserId == TestUsers.EditorA.Id, Cancellation)).ShouldBeFalse();
        (await identity.UserPreferences.Select(p => p.UserId).ToListAsync(Cancellation)).ShouldBe([TestUsers.OwnerA.Id]);
        (await identity.AccountTokens.Select(t => t.UserId).ToListAsync(Cancellation)).ShouldBe([TestUsers.OwnerA.Id]);
        // The list the user created belongs to the organisation and stays.
        (await CountRowsAsync()).ShouldBe(new Rows(Organizations: 3, Memberships: 6, Invitations: 2, Lists: 2, Links: 2));

        // The address and the password open nothing any more.
        using var anonymous = CreateClient();
        using var login = await anonymous.PostAsJsonAsync("/api/v1/auth/login", new { email = TestUsers.EditorA.Email, password = Password }, Cancellation);
        login.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_AccountDeletedBefore_Returns404()
    {
        using (var first = CreateClient(TestUsers.EditorA))
        {
            (await first.DeleteAsync(Account, Cancellation)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        // The test scheme still signs the user in; the real session ends with the security stamp.
        using var client = CreateClient(TestUsers.EditorA);
        using var again = await client.DeleteAsync(Account, Cancellation);

        again.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "INV:SEC-03 " + nameof(Delete_Anonymous_Returns401AndChangesNothing))]
    [Trait("Invariant", "SEC-03")]
    public async Task Delete_Anonymous_Returns401AndChangesNothing()
    {
        var before = await CountRowsAsync();
        using var client = CreateClient();

        using var response = await client.DeleteAsync(Account, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await CountRowsAsync()).ShouldBe(before);
    }

    [Fact(DisplayName = "INV:SEC-02 " + nameof(Delete_WithoutAntiforgeryToken_Returns400AndKeepsTheAccount))]
    [Trait("Invariant", "SEC-02")]
    public async Task Delete_WithoutAntiforgeryToken_Returns400AndKeepsTheAccount()
    {
        using var client = Api.Factory.CreateDefaultClient(BaseAddress);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, TestUsers.Member.Name);

        using var response = await client.DeleteAsync(Account, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync(Cancellation)).ShouldContain("/problems/" + ProblemTypes.Antiforgery);
        (await AccountOfAsync(TestUsers.Member)).IsDeleted.ShouldBeFalse();
        await using var identity = IdentityAccess();
        (await identity.Organizations.IgnoreQueryFilters().AnyAsync(o => o.Id == Solo.Id, Cancellation)).ShouldBeTrue();
    }

    /// <summary>A list that needs no catalog: the deletion never reads its release.</summary>
    private CriteriaConfiguration ListOf(Guid organizationId, TestUser creator, string name) =>
        CriteriaConfiguration.Create(
            organizationId, name, creator.Id, Clock.GetUtcNow(), MiniCatalog.T1Version, "hr",
            ["web"], ["base"], ["softwareTest"],
            [new ConfigurationCriterionDraft(1, PriorityLevel.A), new ConfigurationCriterionDraft(2, PriorityLevel.B, Note: "Notiz")]);

    private ShareLink LinkTo(CriteriaConfiguration list) => ShareLink.Create(
        list.OrganizationId, ShareableResourceType.Configuration, list.Id, RandomHash(), Clock.GetUtcNow(), Clock.GetUtcNow().AddDays(30));

    private static byte[] RandomHash() => SHA256.HashData(RandomNumberGenerator.GetBytes(32));

    private async Task SetRoleAsync(Guid organizationId, TestUser user, OrganizationRole role)
    {
        await using var db = IdentityAccess();
        var membership = await db.OrganizationMemberships.IgnoreQueryFilters()
                                 .SingleAsync(m => m.OrganizationId == organizationId && m.UserId == user.Id, Cancellation);
        membership.ChangeRole(role);
        await db.SaveChangesAsync(Cancellation);
    }

    /// <summary>Gives the account a password and returns its security stamp afterwards.</summary>
    private async Task<string?> SetPasswordAsync(TestUser user)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var account = (await users.FindByIdAsync(user.Id)).ShouldNotBeNull();
        (await users.AddPasswordAsync(account, Password)).Succeeded.ShouldBeTrue();
        return account.SecurityStamp;
    }

    /// <summary>The stored account, also a soft-deleted one.</summary>
    private async Task<User> AccountOfAsync(TestUser user)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDBContext>().Users
                          .IgnoreQueryFilters().AsNoTracking().SingleAsync(u => u.Id == user.Id, Cancellation);
    }

    private async Task<Rows> CountRowsAsync()
    {
        await using var identity = IdentityAccess();
        await using var configurator = Configurator();
        await using var platform = Platform();
        return new Rows(
            await identity.Organizations.IgnoreQueryFilters().CountAsync(Cancellation),
            await identity.OrganizationMemberships.IgnoreQueryFilters().CountAsync(Cancellation),
            await identity.OrganizationInvitations.IgnoreQueryFilters().CountAsync(Cancellation),
            await configurator.CriteriaConfigurations.IgnoreQueryFilters().CountAsync(Cancellation),
            await platform.ShareLinks.IgnoreQueryFilters().CountAsync(Cancellation));
    }

    private sealed record Rows(int Organizations, int Memberships, int Invitations, int Lists, int Links);

    // Contexts of their own outside any request, so the tests read past the organisation filter.
    private IdentityAccessDbContext IdentityAccess() =>
        new(new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(Api.ConnectionString).Options, FixedOrganizationContext.None);

    private ConfiguratorDbContext Configurator() =>
        new(new DbContextOptionsBuilder<ConfiguratorDbContext>().UseNpgsql(Api.ConnectionString).Options, FixedOrganizationContext.None);

    private PlatformDbContext Platform() =>
        new(new DbContextOptionsBuilder<PlatformDbContext>().UseNpgsql(Api.ConnectionString).Options, FixedOrganizationContext.None);
}
