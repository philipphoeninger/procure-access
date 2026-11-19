// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Cryptography;
using DAL.ProcureAccess.EFStructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Persistence;

/// <summary>
/// OPS-03 for the IdentityAccess migration <c>AddUserForeignKeys</c> (S6-10b; owner decisions
/// S6-10 3 and 4): a database from before it, with the rows a Development rebuild left without
/// their user and with a soft-deleted account that still holds roles, migrates; the orphans are
/// gone, the rows of existing users stay, and the five keys exist with their delete rules; and for
/// <c>AssessmentLegendPreference</c> (S6-19b) the settings stored before it.
/// Each test uses a database of its own on the shared container.
/// </summary>
public sealed class UserForeignKeysMigrationTests(ApiFixture api) : IAsyncDisposable
{
    /// <summary>The last IdentityAccess migration before the foreign keys.</summary>
    private const string Before = "20261118015819_AdoptIdentityTables";

    private const string Gone = "user-that-no-longer-exists";

    /// <summary>The last migration before the legend state joined the settings (S6-19b).</summary>
    private const string BeforeAssessmentLegend = "20261118123722_RevokePlatformAdminModerationReview";

    private readonly List<string> _databases = [];

    [Fact(DisplayName = "INV:OPS-03 " + nameof(Migrate_DatabaseWithOrphansAndADeletedAccount_CleansThemAndAddsTheKeys))]
    [Trait("Invariant", "OPS-03")]
    public async Task Migrate_DatabaseWithOrphansAndADeletedAccount_CleansThemAndAddsTheKeys()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();
        var now = api.Clock.GetUtcNow();
        await using (var legacy = Legacy(connectionString))
        {
            await legacy.Database.MigrateAsync(cancellationToken);
        }

        var kept = TestAccounts.WithId("kept-user");
        var deleted = TestAccounts.WithId("deleted-user");
        deleted.IsDeleted = true;
        deleted.AccessFailedCount = 4;
        deleted.LockoutEnd = now.AddMinutes(10);
        var keptOrganization = Organization.Create("Bleibt", OrganizationType.PublicBody, kept.Id, now);
        var orphanOrganization = Organization.Create("Verwaist", OrganizationType.Company, Gone, now);

        await using (var identity = IdentityAccess(connectionString))
        {
            await identity.GetService<IMigrator>().MigrateAsync(Before, cancellationToken);

            var moderator = await identity.Roles.SingleAsync(r => r.Name == PlatformRoles.Moderator, cancellationToken);
            identity.Users.AddRange(kept, deleted);
            identity.UserRoles.AddRange(
                new IdentityUserRole<string> { UserId = kept.Id, RoleId = moderator.Id },
                new IdentityUserRole<string> { UserId = deleted.Id, RoleId = moderator.Id });
            identity.UserClaims.Add(new IdentityUserClaim<string> { UserId = deleted.Id, ClaimType = "sample", ClaimValue = "value" });
            identity.UserLogins.Add(new IdentityUserLogin<string> { UserId = deleted.Id, LoginProvider = "provider", ProviderKey = "key" });
            identity.UserTokens.Add(new IdentityUserToken<string> { UserId = deleted.Id, LoginProvider = "provider", Name = "name", Value = "value" });

            identity.Organizations.AddRange(keptOrganization, orphanOrganization);
            identity.OrganizationMemberships.AddRange(
                OrganizationMembership.Founder(keptOrganization, now),
                OrganizationMembership.Create(keptOrganization.Id, Gone, OrganizationRole.Reader, now, now),
                OrganizationMembership.Founder(orphanOrganization, now),
                OrganizationMembership.Create(orphanOrganization.Id, kept.Id, OrganizationRole.Editor, now, now));
            identity.OrganizationInvitations.Add(OrganizationInvitation.Create(
                orphanOrganization.Id, "invited@example.test", OrganizationRole.Reader, Hash(), now, TimeSpan.FromDays(14)));
            identity.AccountTokens.AddRange(
                AccountToken.Create(kept.Id, AccountTokenPurpose.EmailConfirmation, Hash(), now, TimeSpan.FromHours(24)),
                AccountToken.Create(Gone, AccountTokenPurpose.PasswordReset, Hash(), now, TimeSpan.FromHours(2)));
            var keptProfile = ContributorProfile.Create("Bleibt", ContributorKind.Individual, now);
            var orphanProfile = ContributorProfile.Create("Verwaist", ContributorKind.Vendor, now);
            identity.ContributorProfiles.AddRange(keptProfile, orphanProfile);
            identity.UserContributorLinks.AddRange(
                UserContributorLink.Create(kept.Id, keptProfile), UserContributorLink.Create(Gone, orphanProfile));
            await identity.SaveChangesAsync(cancellationToken);

            // In the columns of this migration: the entity has more since (S6-19b)
            await InsertPreferencesAsync(identity, kept.Id, cancellationToken);
            await InsertPreferencesAsync(identity, Gone, cancellationToken);
        }

        await using (var identity = IdentityAccess(connectionString))
        {
            await identity.Database.MigrateAsync(cancellationToken);
            (await identity.Database.GetPendingMigrationsAsync(cancellationToken)).ShouldBeEmpty();
        }

        await using var after = IdentityAccess(connectionString);

        // Rows of an existing user stay; rows without their user are gone, an organisation whose
        // creator is gone with its memberships and invitations.
        (await after.Organizations.IgnoreQueryFilters().Select(o => o.Id).ToListAsync(cancellationToken)).ShouldBe([keptOrganization.Id]);
        (await after.OrganizationMemberships.IgnoreQueryFilters().Select(m => m.UserId).ToListAsync(cancellationToken)).ShouldBe([kept.Id]);
        (await after.OrganizationInvitations.IgnoreQueryFilters().AnyAsync(cancellationToken)).ShouldBeFalse();
        (await after.AccountTokens.Select(t => t.UserId).ToListAsync(cancellationToken)).ShouldBe([kept.Id]);
        (await after.UserPreferences.Select(p => p.UserId).ToListAsync(cancellationToken)).ShouldBe([kept.Id]);
        (await after.UserContributorLinks.Select(l => l.UserId).ToListAsync(cancellationToken)).ShouldBe([kept.Id]);

        // The soft-deleted account keeps its row and nothing else (S5-28a R5); the other account its role.
        (await after.UserRoles.Select(r => r.UserId).ToListAsync(cancellationToken)).ShouldBe([kept.Id]);
        (await after.UserClaims.AnyAsync(cancellationToken)).ShouldBeFalse();
        (await after.UserLogins.AnyAsync(cancellationToken)).ShouldBeFalse();
        (await after.UserTokens.AnyAsync(cancellationToken)).ShouldBeFalse();
        var deletedAfter = await after.Users.IgnoreQueryFilters().AsNoTracking().SingleAsync(u => u.Id == deleted.Id, cancellationToken);
        deletedAfter.AccessFailedCount.ShouldBe(0);
        deletedAfter.LockoutEnd.ShouldBeNull();

        // The five keys with their delete rules ('c' cascade, 'r' restrict).
        (await ForeignKeysOntoUsersAsync(connectionString)).ShouldBe(
        [
            "AccountTokens.FK_AccountTokens_Users_UserId c",
            "OrganizationMemberships.FK_OrganizationMemberships_Users_UserId c",
            "Organizations.FK_Organizations_Users_CreatedByUserId r",
            "UserContributorLinks.FK_UserContributorLinks_Users_UserId c",
            "UserPreferences.FK_UserPreferences_Users_UserId c",
        ]);
    }

    /// <summary>
    /// S6-19b (SPEC R-CAN-08, owner decision S6-19 Q1 (b)): the settings stored before
    /// <c>AssessmentLegendPreference</c> keep their values and get the legend state <c>Auto</c>,
    /// the value of a user who never saved any.
    /// </summary>
    [Fact(DisplayName = "INV:OPS-03 " + nameof(AssessmentLegendPreference_SettingsStoredBefore_KeepTheirValuesAndStartAtAuto))]
    [Trait("Invariant", "OPS-03")]
    public async Task AssessmentLegendPreference_SettingsStoredBefore_KeepTheirValuesAndStartAtAuto()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();
        await using (var legacy = Legacy(connectionString))
        {
            await legacy.Database.MigrateAsync(cancellationToken);
        }

        var user = TestAccounts.WithId("settings-user");
        await using (var identity = IdentityAccess(connectionString))
        {
            await identity.GetService<IMigrator>().MigrateAsync(BeforeAssessmentLegend, cancellationToken);
            identity.Users.Add(user);
            await identity.SaveChangesAsync(cancellationToken);
            await identity.Database.ExecuteSqlAsync($"""
                INSERT INTO identity."UserPreferences" ("UserId", "Language", "Appearance", "Motion", "Contrast")
                VALUES ({user.Id}, 'de', 'Dark', 'Reduced', 'System')
                """, cancellationToken);
        }

        await using (var identity = IdentityAccess(connectionString))
        {
            await identity.Database.MigrateAsync(cancellationToken);
        }

        await using var after = IdentityAccess(connectionString);
        var stored = await after.UserPreferences.AsNoTracking().SingleAsync(p => p.UserId == user.Id, cancellationToken);
        stored.Appearance.ShouldBe(AppearancePreference.Dark);
        stored.Motion.ShouldBe(MotionPreference.Reduced);
        stored.AssessmentLegend.ShouldBe(AssessmentLegendPreference.Auto);
    }

    [Fact]
    public async Task DeleteUserRow_WithAnOrganisationItCreated_IsRefusedAndWithoutOneTakesItsRowsAlong()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();
        var now = api.Clock.GetUtcNow();
        await using (var legacy = Legacy(connectionString))
        {
            await legacy.Database.MigrateAsync(cancellationToken);
        }

        var creator = TestAccounts.WithId("creator");
        var member = TestAccounts.WithId("member");
        var organization = Organization.Create("Organisation", OrganizationType.PublicBody, creator.Id, now);
        await using (var identity = IdentityAccess(connectionString))
        {
            await identity.Database.MigrateAsync(cancellationToken);
            identity.Users.AddRange(creator, member);
            identity.Organizations.Add(organization);
            identity.OrganizationMemberships.AddRange(
                OrganizationMembership.Founder(organization, now),
                OrganizationMembership.Create(organization.Id, member.Id, OrganizationRole.Reader, now, now));
            identity.AccountTokens.Add(AccountToken.Create(member.Id, AccountTokenPurpose.PasswordReset, Hash(), now, TimeSpan.FromHours(2)));
            identity.UserPreferences.Add(UserPreferences.Defaults(member.Id));
            await identity.SaveChangesAsync(cancellationToken);
        }

        await using var db = IdentityAccess(connectionString);

        // Restrict: the tenant never ends because a user row is deleted.
        var refused = await Should.ThrowAsync<PostgresException>(
            () => db.Database.ExecuteSqlAsync($"""DELETE FROM identity."Users" WHERE "Id" = {creator.Id}""", cancellationToken));
        refused.SqlState.ShouldBe(PostgresErrorCodes.ForeignKeyViolation);

        // Cascade: membership, token and settings go with the user row.
        await db.Database.ExecuteSqlAsync($"""DELETE FROM identity."Users" WHERE "Id" = {member.Id}""", cancellationToken);
        (await db.OrganizationMemberships.IgnoreQueryFilters().Select(m => m.UserId).ToListAsync(cancellationToken)).ShouldBe([creator.Id]);
        (await db.AccountTokens.AnyAsync(cancellationToken)).ShouldBeFalse();
        (await db.UserPreferences.AnyAsync(cancellationToken)).ShouldBeFalse();
        (await db.Organizations.IgnoreQueryFilters().CountAsync(cancellationToken)).ShouldBe(1);
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        foreach (var connectionString in _databases)
        {
            await using var context = IdentityAccess(connectionString);
            await context.Database.EnsureDeletedAsync();
        }
    }

    /// <summary>Default settings of <paramref name="userId"/> in the four columns of S5-25a.</summary>
    private static Task<int> InsertPreferencesAsync(IdentityAccessDbContext identity, string userId, CancellationToken cancellationToken) =>
        identity.Database.ExecuteSqlAsync($"""
            INSERT INTO identity."UserPreferences" ("UserId", "Language", "Appearance", "Motion", "Contrast")
            VALUES ({userId}, 'de', 'System', 'System', 'System')
            """, cancellationToken);

    private static byte[] Hash() => SHA256.HashData(RandomNumberGenerator.GetBytes(32));

    private string NewDatabase()
    {
        var connectionString = api.ConnectionStringForNewDatabase();
        _databases.Add(connectionString);
        return connectionString;
    }

    private static ApplicationDBContext Legacy(string connectionString) =>
        new(new DbContextOptionsBuilder<ApplicationDBContext>().UseNpgsql(connectionString).Options);

    private static IdentityAccessDbContext IdentityAccess(string connectionString) =>
        new(new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(connectionString).Options, FixedOrganizationContext.None);

    private static async Task<List<string>> ForeignKeysOntoUsersAsync(string connectionString)
    {
        const string sql = """
            SELECT t.relname || '.' || c.conname || ' ' || c.confdeltype::text
            FROM pg_constraint c
            JOIN pg_class t ON t.oid = c.conrelid
            WHERE c.contype = 'f' AND c.confrelid = 'identity."Users"'::regclass
              AND t.relname NOT IN ('UserClaims', 'UserLogins', 'UserRoles', 'UserTokens')
            ORDER BY 1
            """;
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        var lines = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lines.Add(reader.GetString(0));
        }

        return lines;
    }
}
