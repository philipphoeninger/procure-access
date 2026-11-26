// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using DAL.ProcureAccess.EFStructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Persistence;

/// <summary>
/// The legacy migration <c>RenamePlatformRoles</c> (S4-25, owner decisions O-S4-81, O-S4-82):
/// <c>Approver</c> and <c>Admin</c> become <c>Moderator</c> and <c>PlatformAdmin</c> with their users,
/// <c>objects:approve</c> becomes <c>moderation:review</c>, and every database ends with the three
/// platform roles. Each test uses a database of its own on the shared container. The legacy
/// context migrates; the rows are written and read through the IdentityAccess context, which maps
/// the Identity tables since S6-10 and whose role tables have the layout of every legacy migration.
/// </summary>
public sealed class PlatformRoleMigrationTests(ApiFixture api) : IAsyncDisposable
{
    private const string MigrationBefore = "20261106083424_RemoveHrMobileExclusions";
    private const string LegacyApproveClaim = "objects:approve";

    private readonly List<string> _databases = [];

    [Fact(DisplayName = "INV:TEN-06 " + nameof(Migrate_LegacyApproverAndAdmin_BecomeModeratorAndPlatformAdminWithTheirUsers))]
    [Trait("Invariant", "TEN-06")]
    public async Task Migrate_LegacyApproverAndAdmin_BecomeModeratorAndPlatformAdminWithTheirUsers()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();
        await using var legacy = Legacy(connectionString);
        await using var db = Identity(connectionString);
        await legacy.GetService<IMigrator>().MigrateAsync(MigrationBefore, cancellationToken);

        // The roles as the Development rebuild created them before S4-25, with one user each.
        var approver = new IdentityRole("Approver") { NormalizedName = "APPROVER" };
        var admin = new IdentityRole("Admin") { NormalizedName = "ADMIN" };
        var member = new IdentityRole("Member") { NormalizedName = "MEMBER" };
        db.Roles.AddRange(approver, admin, member);
        db.RoleClaims.AddRange(
            new IdentityRoleClaim<string> { RoleId = approver.Id, ClaimType = PlatformPermissions.ClaimType, ClaimValue = LegacyApproveClaim },
            new IdentityRoleClaim<string> { RoleId = admin.Id, ClaimType = PlatformPermissions.ClaimType, ClaimValue = LegacyApproveClaim });
        var reviewerId = await InsertUserAsync(legacy, "reviewer@example.test", cancellationToken);
        var ownerId = await InsertUserAsync(legacy, "owner@example.test", cancellationToken);
        db.UserRoles.AddRange(
            new IdentityUserRole<string> { UserId = reviewerId, RoleId = approver.Id },
            new IdentityUserRole<string> { UserId = ownerId, RoleId = admin.Id });
        await db.SaveChangesAsync(cancellationToken);
        db.ChangeTracker.Clear();

        await legacy.Database.MigrateAsync(cancellationToken);

        var roles = await db.Roles.AsNoTracking().ToListAsync(cancellationToken);
        roles.Select(r => r.Name).ShouldBe(["Member", .. PlatformRoles.All], ignoreOrder: true);
        roles.Single(r => r.Name == PlatformRoles.Moderator).Id.ShouldBe(approver.Id);
        roles.Single(r => r.Name == PlatformRoles.PlatformAdmin).Id.ShouldBe(admin.Id);
        roles.ShouldAllBe(r => r.NormalizedName == r.Name!.ToUpperInvariant());
        (await RoleNamesOfAsync(db, reviewerId, cancellationToken)).ShouldBe([PlatformRoles.Moderator]);
        (await RoleNamesOfAsync(db, ownerId, cancellationToken)).ShouldBe([PlatformRoles.PlatformAdmin]);
        await AssertPermissionsAsync(db, cancellationToken);
    }

    [Fact(DisplayName = "INV:TEN-06 " + nameof(Migrate_EmptyDatabase_CreatesThePlatformRolesWithoutLegacyNames))]
    [Trait("Invariant", "TEN-06")]
    public async Task Migrate_EmptyDatabase_CreatesThePlatformRolesWithoutLegacyNames()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();
        await using var legacy = Legacy(connectionString);
        await using var db = Identity(connectionString);

        await legacy.Database.MigrateAsync(cancellationToken);

        var names = await db.Roles.AsNoTracking().Select(r => r.Name).ToListAsync(cancellationToken);
        names.ShouldBe(PlatformRoles.All, ignoreOrder: true);
        await AssertPermissionsAsync(db, cancellationToken);
    }

    [Fact]
    public async Task MigrateDown_ToTheMigrationBefore_RestoresTheLegacyNames()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();
        await using var legacy = Legacy(connectionString);
        await using var db = Identity(connectionString);
        await legacy.Database.MigrateAsync(cancellationToken);

        await legacy.GetService<IMigrator>().MigrateAsync(MigrationBefore, cancellationToken);

        (await db.Roles.AsNoTracking().Select(r => r.Name).ToListAsync(cancellationToken))
            .ShouldBe(["Admin", "Approver"], ignoreOrder: true);
        (await db.RoleClaims.AsNoTracking().Select(c => c.ClaimValue).ToListAsync(cancellationToken))
            .ShouldBe([LegacyApproveClaim, LegacyApproveClaim]);
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

    /// <summary>
    /// No role and no claim keeps a legacy name (TEN-06); Moderator and PlatformAdmin carry
    /// <c>moderation:review</c> once, CatalogEditor no permission.
    /// </summary>
    private static async Task AssertPermissionsAsync(IdentityAccessDbContext db, CancellationToken cancellationToken)
    {
        var claims = await (
            from claim in db.RoleClaims.AsNoTracking()
            join role in db.Roles on claim.RoleId equals role.Id
            select new { role.Name, claim.ClaimType, claim.ClaimValue }).ToListAsync(cancellationToken);
        claims.ShouldAllBe(c => c.ClaimType == PlatformPermissions.ClaimType && c.ClaimValue == PlatformPermissions.ModerationReview);
        claims.Select(c => c.Name).ShouldBe([PlatformRoles.Moderator, PlatformRoles.PlatformAdmin], ignoreOrder: true);
        (await db.Roles.AsNoTracking().AnyAsync(r => r.NormalizedName == "APPROVER" || r.NormalizedName == "ADMIN", cancellationToken))
            .ShouldBeFalse();
    }

    private static async Task<List<string?>> RoleNamesOfAsync(IdentityAccessDbContext db, string userId, CancellationToken cancellationToken) =>
        await (
            from userRole in db.UserRoles.AsNoTracking()
            join role in db.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            select role.Name).ToListAsync(cancellationToken);

    /// <summary>
    /// Stores a user row in the schema of <see cref="MigrationBefore"/> with SQL, not through the
    /// current <see cref="User"/> model, whose later columns (e.g. <c>CreatedAt</c>, S5-27) that
    /// schema lacks. Nothing reads the row back as a <see cref="User"/>, so the JSON column holds
    /// an empty object.
    /// </summary>
    private static async Task<string> InsertUserAsync(ApplicationDBContext db, string email, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid().ToString();
        var normalized = email.ToUpperInvariant();
        const string uiCustomization = "{}";
        await db.Database.ExecuteSqlAsync(
            $"""
            INSERT INTO identity."Users" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed",
                "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "IsDeleted", "UICustomization")
            VALUES ({id}, {email}, {normalized}, {email}, {normalized}, false, false, false, false, 0, false, CAST({uiCustomization} AS jsonb))
            """,
            cancellationToken);
        return id;
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
