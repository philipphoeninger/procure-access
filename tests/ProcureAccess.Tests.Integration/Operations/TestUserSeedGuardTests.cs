// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using API.ProcureAccess.Extensions;
using DAL.ProcureAccess.EFStructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MODELS.ProcureAccess.Entities;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// The E2E users (S5-14, TESTING §6.3) are seeded at startup only in <c>Development</c> and
/// <c>E2E</c> and only when their password is set (owner decision S5-14 1 (a)), never in Staging or
/// Production. Each test starts the API on a database of its own.
/// </summary>
public sealed class TestUserSeedGuardTests(ApiFixture api) : IAsyncDisposable
{
    // Test data like the one in docker-compose.e2e.yml, not a secret; meets the password rule.
    private const string Password = "e2e-Seed-Passwort-lang";

    private readonly string _connectionString = api.ConnectionStringForNewDatabase();

    [Theory]
    [InlineData("Testing")]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task Startup_TestUsersConfiguredOutsideDevelopmentAndE2E_SeedsNothing(string environment)
    {
        await StartApiAsync(environment, Password);

        (await UsersAsync()).ShouldBeEmpty();
        (await OrganizationsAsync()).ShouldBeEmpty();
    }

    /// <summary>
    /// Control for the theory above: the same settings do seed in Development and E2E, so the
    /// theory cannot pass merely because the settings never reached the API.
    /// </summary>
    [Theory]
    [InlineData("Development")]
    [InlineData(TestCatalogSeedExtensions.E2EEnvironmentName)]
    public async Task Startup_TestUsersConfiguredInDevelopmentOrE2E_CreatesBothWithThatPasswordWithoutRole(string environment)
    {
        await StartApiAsync(environment, Password);

        var users = await UsersAsync();
        users.Select(u => (u.Email!, u.EmailConfirmed)).OrderBy(u => u.Item1)
            .ShouldBe(TestUserSeedExtensions.Users.OrderBy(u => u.Email));
        foreach (var user in users)
        {
            user.UserName.ShouldBe(user.Email);
            new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash!, Password)
                .ShouldNotBe(PasswordVerificationResult.Failed);
        }

        await using var db = CreateDbContext();
        (await db.UserRoles.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    /// <summary>The E2E organisation with its Owner, Editor and Reader (S5-16b, owner decision S5-16 7a).</summary>
    [Fact]
    public async Task Startup_TestUsersConfigured_CreatesTheE2EOrganizationWithItsThreeMembers()
    {
        await StartApiAsync(TestCatalogSeedExtensions.E2EEnvironmentName, Password);

        var users = (await UsersAsync()).ToDictionary(u => u.Email!, u => u.Id);
        var organization = (await OrganizationsAsync()).ShouldHaveSingleItem();
        organization.Id.ShouldBe(TestUserSeedExtensions.OrganizationId);
        organization.Name.ShouldBe(TestUserSeedExtensions.OrganizationName);
        organization.CreatedByUserId.ShouldBe(users[TestUserSeedExtensions.OrganizationOwnerEmail]);
        await using var identity = CreateIdentityAccessContext();
        var memberships = await identity.OrganizationMemberships.IgnoreQueryFilters().AsNoTracking()
            .Select(m => new { m.UserId, m.Role })
            .ToListAsync(TestContext.Current.CancellationToken);
        memberships.Select(m => (m.UserId, m.Role)).OrderBy(m => m.Role)
            .ShouldBe(TestUserSeedExtensions.OrganizationMembers.Select(m => (users[m.Email], m.Role)).OrderBy(m => m.Item2));
    }

    [Fact]
    public async Task Startup_NoTestUserPassword_SeedsNothing()
    {
        await StartApiAsync("Development", password: null);

        (await UsersAsync()).ShouldBeEmpty();
    }

    /// <summary>A second start leaves the users as they are (idempotent, TESTING §6.3).</summary>
    [Fact]
    public async Task Startup_TestUsersExistAlready_LeavesThemUnchanged()
    {
        await StartApiAsync(TestCatalogSeedExtensions.E2EEnvironmentName, Password);
        var first = await UsersAsync();

        await StartApiAsync(TestCatalogSeedExtensions.E2EEnvironmentName, Password);

        var second = await UsersAsync();
        second.Select(u => (u.Id, u.PasswordHash, u.SecurityStamp)).OrderBy(u => u.Id)
            .ShouldBe(first.Select(u => (u.Id, u.PasswordHash, u.SecurityStamp)).OrderBy(u => u.Id));
        (await OrganizationsAsync()).ShouldHaveSingleItem();
    }

    /// <summary>
    /// A rebuild deletes the users and the seeder creates them again with new ids: the E2E
    /// organisation is created again for them, so its memberships never name a deleted user.
    /// </summary>
    [Fact]
    public async Task Startup_RebuildDatabaseTwice_TheE2EOrganizationNamesTheCurrentUsers()
    {
        await StartApiAsync("Development", Password, rebuildDatabase: true);
        await StartApiAsync("Development", Password, rebuildDatabase: true);

        var users = (await UsersAsync()).ToDictionary(u => u.Email!, u => u.Id);
        (await OrganizationsAsync()).ShouldHaveSingleItem().CreatedByUserId
            .ShouldBe(users[TestUserSeedExtensions.OrganizationOwnerEmail]);
        await using var identity = CreateIdentityAccessContext();
        var memberIds = await identity.OrganizationMemberships.IgnoreQueryFilters().AsNoTracking()
            .Select(m => m.UserId).ToListAsync(TestContext.Current.CancellationToken);
        memberIds.Order().ShouldBe(TestUserSeedExtensions.OrganizationMembers.Select(m => users[m.Email]).Order());
    }

    /// <summary>
    /// The E2E stack runs in Development with <c>RebuildDatabase</c>, which deletes every user:
    /// the seeder runs after it, so both users are there when the API serves requests.
    /// </summary>
    [Fact]
    public async Task Startup_RebuildDatabaseInDevelopment_SeedsTestUsersAfterTheRebuild()
    {
        await StartApiAsync("Development", Password, rebuildDatabase: true);

        (await UsersAsync()).Select(u => u.Email).Order()
            .ShouldBe(TestUserSeedExtensions.Users.Select(u => u.Email).Order());
    }

    /// <summary>Starts the API (Program.cs runs up to <c>app.Run()</c>) and stops it again.</summary>
    private async Task StartApiAsync(string environment, string? password, bool rebuildDatabase = false)
    {
        await using var factory = new ProcureAccessApiFactory(
            _connectionString,
            api.Clock,
            environment,
            new Dictionary<string, string?>
            {
                [TestUserSeedExtensions.PasswordKey] = password,
                ["RebuildDatabase"] = rebuildDatabase ? "true" : "false",
            });
        _ = factory.Services; // builds the host, which runs the startup code
    }

    private async Task<List<User>> UsersAsync()
    {
        await using var db = CreateDbContext();
        return await db.Users.IgnoreQueryFilters().AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);
    }

    private async Task<List<Organization>> OrganizationsAsync()
    {
        await using var db = CreateIdentityAccessContext();
        return await db.Organizations.IgnoreQueryFilters().AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);
    }

    private IdentityAccessDbContext CreateIdentityAccessContext() =>
        new(new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(_connectionString).Options, FixedOrganizationContext.None);

    private ApplicationDBContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseNpgsql(_connectionString)
            .Options);

    public async ValueTask DisposeAsync()
    {
        await using var db = CreateDbContext();
        await db.Database.EnsureDeletedAsync();
    }
}
