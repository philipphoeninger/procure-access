// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Persistence;

/// <summary>
/// The retirement of the legacy proposals (S6-23; owner decision O-S6-4 (a)): the legacy migration
/// <c>RetireProposals</c> drops <c>public."Proposals"</c> with its rows and leaves products and
/// criteria alone, and the IdentityAccess migration <c>RevokePlatformAdminModerationReview</c>
/// takes <c>moderation:review</c> from PlatformAdmin while Moderator keeps it. Each test uses a
/// database of its own on the shared container.
/// </summary>
public sealed class ProposalRetirementMigrationTests(ApiFixture api) : IAsyncDisposable
{
    /// <summary>The last legacy migration before <c>RetireProposals</c>.</summary>
    private const string LegacyBefore = "20261118015734_HandOverIdentityTables";

    /// <summary>The last IdentityAccess migration before <c>RevokePlatformAdminModerationReview</c>.</summary>
    private const string IdentityBefore = "20261118023759_AddUserForeignKeys";

    private readonly List<string> _databases = [];

    [Fact]
    public async Task RetireProposals_DatabaseWithProposals_DropsTheTableAndKeepsProductsAndCriteria()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();
        await using var legacy = Legacy(connectionString);
        await legacy.GetService<IMigrator>().MigrateAsync(LegacyBefore, cancellationToken);

        // A pending product proposal and a pending criterion proposal, as the legacy forms stored them.
        await ExecuteAsync(connectionString, """
            INSERT INTO public."Proposals" ("ProposerId", "ProductSnapshot", "Status", "IsDeleted")
            VALUES ('proposer-1', '{"name":"Beispiel"}', 0, false);
            INSERT INTO public."Proposals" ("ProposerId", "CriterionSnapshot", "Status", "IsDeleted")
            VALUES ('proposer-2', '{"name":"Kriterium"}', 0, false);
            """);
        var products = await ScalarAsync(connectionString, """SELECT count(*) FROM public."Products" """);
        var criteria = await ScalarAsync(connectionString, """SELECT count(*) FROM public."Criteria" """);

        await legacy.Database.MigrateAsync(cancellationToken);

        (await ScalarAsync(connectionString, ProposalTables)).ShouldBe(0);
        (await ScalarAsync(connectionString, """SELECT count(*) FROM public."Products" """)).ShouldBe(products);
        (await ScalarAsync(connectionString, """SELECT count(*) FROM public."Criteria" """)).ShouldBe(criteria);
        (await legacy.Database.GetPendingMigrationsAsync(cancellationToken)).ShouldBeEmpty();

        // Down creates the empty table again; the rows do not come back.
        await legacy.GetService<IMigrator>().MigrateAsync(LegacyBefore, cancellationToken);
        (await ScalarAsync(connectionString, ProposalTables)).ShouldBe(1);
        (await ScalarAsync(connectionString, """SELECT count(*) FROM public."Proposals" """)).ShouldBe(0);
    }

    [Fact(DisplayName = "INV:TEN-06 " + nameof(RevokePlatformAdminModerationReview_BothRoles_OnlyModeratorKeepsThePermission))]
    [Trait("Invariant", "TEN-06")]
    public async Task RevokePlatformAdminModerationReview_BothRoles_OnlyModeratorKeepsThePermission()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();
        await using (var legacy = Legacy(connectionString))
        {
            await legacy.Database.MigrateAsync(cancellationToken);
        }
        await using var identity = Identity(connectionString);
        var migrator = identity.GetService<IMigrator>();
        await migrator.MigrateAsync(IdentityBefore, cancellationToken);
        (await RolesWithPermissionAsync(identity, cancellationToken)).ShouldBe([PlatformRoles.Moderator, PlatformRoles.PlatformAdmin]);

        await identity.Database.MigrateAsync(cancellationToken);

        (await RolesWithPermissionAsync(identity, cancellationToken)).ShouldBe([PlatformRoles.Moderator]);
        (await identity.Roles.AsNoTracking().Select(r => r.Name).ToListAsync(cancellationToken))
            .ShouldBe(PlatformRoles.All, ignoreOrder: true);

        // Down gives the permission back, once.
        await migrator.MigrateAsync(IdentityBefore, cancellationToken);
        (await RolesWithPermissionAsync(identity, cancellationToken)).ShouldBe([PlatformRoles.Moderator, PlatformRoles.PlatformAdmin]);
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        foreach (var connectionString in _databases)
        {
            await using var context = Legacy(connectionString);
            await context.Database.EnsureDeletedAsync();
        }
    }

    private const string ProposalTables = """
        SELECT count(*) FROM pg_tables WHERE schemaname = 'public' AND tablename = 'Proposals'
        """;

    /// <summary>The roles that carry <c>moderation:review</c>, one entry per claim row, sorted.</summary>
    private static async Task<List<string?>> RolesWithPermissionAsync(IdentityAccessDbContext db, CancellationToken cancellationToken) =>
        await (
            from claim in db.RoleClaims.AsNoTracking()
            join role in db.Roles on claim.RoleId equals role.Id
            where claim.ClaimType == "permission" && claim.ClaimValue == "moderation:review"
            orderby role.Name
            select role.Name).ToListAsync(cancellationToken);

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<long> ScalarAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }

    private string NewDatabase()
    {
        var connectionString = api.ConnectionStringForNewDatabase();
        _databases.Add(connectionString);
        return connectionString;
    }

    private static ApplicationDBContext Legacy(string connectionString) =>
        new(new DbContextOptionsBuilder<ApplicationDBContext>().UseNpgsql(connectionString).Options);

    private static IdentityAccessDbContext Identity(string connectionString) =>
        new(new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(connectionString).Options, FixedOrganizationContext.None);
}
