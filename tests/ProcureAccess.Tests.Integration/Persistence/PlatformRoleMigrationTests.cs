// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using DAL.ProcureAccess.EFStructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MODELS.ProcureAccess.Entities;
using MODELS.ProcureAccess.Entities.Authorization;
using Npgsql;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Persistence;

/// <summary>
/// The legacy migration <c>RenamePlatformRoles</c> (S4-25, owner decisions O-S4-81, O-S4-82):
/// <c>Approver</c> and <c>Admin</c> become <c>Moderator</c> and <c>PlatformAdmin</c> with their users,
/// <c>objects:approve</c> becomes <c>moderation:review</c>, and every database ends with the three
/// platform roles. Each test uses a database of its own on the shared container.
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
        await using var db = Legacy(NewDatabase());
        await db.GetService<IMigrator>().MigrateAsync(MigrationBefore, cancellationToken);

        // The roles as the Development rebuild created them before S4-25, with one user each.
        var approver = new IdentityRole("Approver") { NormalizedName = "APPROVER" };
        var admin = new IdentityRole("Admin") { NormalizedName = "ADMIN" };
        var member = new IdentityRole("Member") { NormalizedName = "MEMBER" };
        db.Roles.AddRange(approver, admin, member);
        db.RoleClaims.AddRange(
            new IdentityRoleClaim<string> { RoleId = approver.Id, ClaimType = CustomClaimTypes.Permission, ClaimValue = LegacyApproveClaim },
            new IdentityRoleClaim<string> { RoleId = admin.Id, ClaimType = CustomClaimTypes.Permission, ClaimValue = LegacyApproveClaim });
        var reviewer = UserRow("reviewer@example.test");
        var owner = UserRow("owner@example.test");
        db.Users.AddRange(reviewer, owner);
        db.UserRoles.AddRange(
            new IdentityUserRole<string> { UserId = reviewer.Id, RoleId = approver.Id },
            new IdentityUserRole<string> { UserId = owner.Id, RoleId = admin.Id });
        await db.SaveChangesAsync(cancellationToken);
        db.ChangeTracker.Clear();

        await db.Database.MigrateAsync(cancellationToken);

        var roles = await db.Roles.AsNoTracking().ToListAsync(cancellationToken);
        roles.Select(r => r.Name).ShouldBe(["Member", .. Roles.PlatformRoles], ignoreOrder: true);
        roles.Single(r => r.Name == Roles.Moderator).Id.ShouldBe(approver.Id);
        roles.Single(r => r.Name == Roles.PlatformAdmin).Id.ShouldBe(admin.Id);
        roles.ShouldAllBe(r => r.NormalizedName == r.Name!.ToUpperInvariant());
        (await RoleNamesOfAsync(db, reviewer.Id, cancellationToken)).ShouldBe([Roles.Moderator]);
        (await RoleNamesOfAsync(db, owner.Id, cancellationToken)).ShouldBe([Roles.PlatformAdmin]);
        await AssertPermissionsAsync(db, cancellationToken);
    }

    [Fact(DisplayName = "INV:TEN-06 " + nameof(Migrate_EmptyDatabase_CreatesThePlatformRolesWithoutLegacyNames))]
    [Trait("Invariant", "TEN-06")]
    public async Task Migrate_EmptyDatabase_CreatesThePlatformRolesWithoutLegacyNames()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = Legacy(NewDatabase());

        await db.Database.MigrateAsync(cancellationToken);

        var names = await db.Roles.AsNoTracking().Select(r => r.Name).ToListAsync(cancellationToken);
        names.ShouldBe(Roles.PlatformRoles, ignoreOrder: true);
        await AssertPermissionsAsync(db, cancellationToken);
    }

    [Fact]
    public async Task MigrateDown_ToTheMigrationBefore_RestoresTheLegacyNames()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = Legacy(NewDatabase());
        await db.Database.MigrateAsync(cancellationToken);

        await db.GetService<IMigrator>().MigrateAsync(MigrationBefore, cancellationToken);

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
    private static async Task AssertPermissionsAsync(ApplicationDBContext db, CancellationToken cancellationToken)
    {
        var claims = await (
            from claim in db.RoleClaims.AsNoTracking()
            join role in db.Roles on claim.RoleId equals role.Id
            select new { role.Name, claim.ClaimType, claim.ClaimValue }).ToListAsync(cancellationToken);
        claims.ShouldAllBe(c => c.ClaimType == CustomClaimTypes.Permission && c.ClaimValue == Permissions.ModerationReview);
        claims.Select(c => c.Name).ShouldBe([Roles.Moderator, Roles.PlatformAdmin], ignoreOrder: true);
        (await db.Roles.AsNoTracking().AnyAsync(r => r.NormalizedName == "APPROVER" || r.NormalizedName == "ADMIN", cancellationToken))
            .ShouldBeFalse();
    }

    private static async Task<List<string?>> RoleNamesOfAsync(ApplicationDBContext db, string userId, CancellationToken cancellationToken) =>
        await (
            from userRole in db.UserRoles.AsNoTracking()
            join role in db.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == userId
            select role.Name).ToListAsync(cancellationToken);

    private static User UserRow(string email) => new()
    {
        UserName = email,
        NormalizedUserName = email.ToUpperInvariant(),
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
    };

    private string NewDatabase()
    {
        var connectionString = api.ConnectionStringForNewDatabase();
        _databases.Add(connectionString);
        return connectionString;
    }

    private static ApplicationDBContext Legacy(string connectionString) =>
        new(new DbContextOptionsBuilder<ApplicationDBContext>().UseNpgsql(connectionString).Options);
}
