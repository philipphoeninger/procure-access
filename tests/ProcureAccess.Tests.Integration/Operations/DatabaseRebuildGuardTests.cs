// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Domain;
using API.ProcureAccess.Extensions;
using DAL.ProcureAccess.EFStructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MODELS.ProcureAccess.Entities;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// SEC-06: <c>RebuildDatabase</c> (delete all users, reseed sample data) runs only in
/// <c>Development</c>, even when the flag is <c>true</c> (ARCHITECTURE §9.7). Each test starts
/// the API on a database of its own with one existing user, because a rebuild would wipe it.
/// The sample user has no fixed password or hash in the repository: its password comes from
/// configuration, and without one no sample user is created (issue #63).
/// </summary>
public sealed class DatabaseRebuildGuardTests(ApiFixture api) : IAsyncDisposable
{
    // Meets the Identity password rules; random, so no password is fixed in the repository.
    private static readonly string SampleUserPassword = "Sample-1" + Guid.NewGuid().ToString("N");

    private readonly string _connectionString = api.ConnectionStringForNewDatabase();

    [Theory(DisplayName = "INV:SEC-06 " + nameof(Startup_RebuildDatabaseTrueOutsideDevelopment_LeavesUsersUnchanged))]
    [Trait("Invariant", "SEC-06")]
    [InlineData("Testing")]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task Startup_RebuildDatabaseTrueOutsideDevelopment_LeavesUsersUnchanged(string environment)
    {
        var existing = await CreateDatabaseWithUserAsync();

        await StartApiWithRebuildFlagAsync(environment, SampleUserPassword);

        (await UserIdsAsync()).ShouldBe([existing.Id]);
    }

    /// <summary>
    /// Control for the theory above: the same setup in <c>Development</c> does rebuild, so the
    /// theory cannot pass merely because the flag never reached the API.
    /// </summary>
    [Fact]
    public async Task Startup_RebuildDatabaseTrueInDevelopment_ReplacesUsersWithSampleData()
    {
        var existing = await CreateDatabaseWithUserAsync();

        await StartApiWithRebuildFlagAsync("Development", SampleUserPassword);

        var userIds = await UserIdsAsync();
        userIds.ShouldNotContain(existing.Id);
        userIds.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Startup_RebuildInDevelopmentWithSamplePassword_CreatesSampleUserWithThatPasswordWithoutRole()
    {
        await CreateDatabaseWithUserAsync();

        await StartApiWithRebuildFlagAsync("Development", SampleUserPassword);

        await using var db = CreateIdentityAccessContext();
        var user = await db.Users.SingleAsync(TestContext.Current.CancellationToken);
        user.Email.ShouldBe(IdentityAccessModule.SampleUserEmail);
        new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash!, SampleUserPassword)
            .ShouldNotBe(PasswordVerificationResult.Failed);

        var roles = await db.UserRoles
            .Where(userRole => userRole.UserId == user.Id)
            .Join(db.Roles, userRole => userRole.RoleId, role => role.Id, (_, role) => role.Name)
            .ToListAsync(TestContext.Current.CancellationToken);
        roles.ShouldBeEmpty(); // no global Member role since S5-12a (O-S5-3 (a))
    }

    [Fact]
    public async Task Startup_RebuildInDevelopmentWithoutSamplePassword_CreatesNoUser()
    {
        await CreateDatabaseWithUserAsync();

        await StartApiWithRebuildFlagAsync("Development", samplePassword: null);

        (await UserIdsAsync()).ShouldBeEmpty();
    }

    /// <summary>
    /// The creator of an organisation is a foreign key with restrict (S6-10b): the rebuild deletes
    /// the organisations before the users, or the start would fail.
    /// </summary>
    [Fact]
    public async Task Startup_RebuildInDevelopment_DeletesTheOrganisationsBeforeTheirUsers()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var existing = await CreateDatabaseWithUserAsync();
        await using (var identity = CreateIdentityAccessContext())
        {
            await identity.Database.MigrateAsync(cancellationToken);
            var organization = Organization.Create("Bestand", OrganizationType.PublicBody, existing.Id, api.Clock.GetUtcNow());
            identity.Organizations.Add(organization);
            identity.OrganizationMemberships.Add(OrganizationMembership.Founder(organization, api.Clock.GetUtcNow()));
            await identity.SaveChangesAsync(cancellationToken);
        }

        await StartApiWithRebuildFlagAsync("Development", samplePassword: null);

        (await UserIdsAsync()).ShouldBeEmpty();
        await using var after = CreateIdentityAccessContext();
        (await after.Organizations.IgnoreQueryFilters().AnyAsync(cancellationToken)).ShouldBeFalse();
        (await after.OrganizationMemberships.IgnoreQueryFilters().AnyAsync(cancellationToken)).ShouldBeFalse();
    }

    private async Task<User> CreateDatabaseWithUserAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        await db.Database.MigrateAsync(cancellationToken);

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "existing@example.test",
            NormalizedUserName = "EXISTING@EXAMPLE.TEST",
            Email = "existing@example.test",
            NormalizedEmail = "EXISTING@EXAMPLE.TEST",
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        // The legacy migrations created the table; the users are the IdentityAccess module's (S6-10).
        await using var identity = CreateIdentityAccessContext();
        identity.Users.Add(user);
        await identity.SaveChangesAsync(cancellationToken);
        return user;
    }

    /// <summary>Starts the API (Program.cs runs up to <c>app.Run()</c>) and stops it again.</summary>
    private async Task StartApiWithRebuildFlagAsync(string environment, string? samplePassword)
    {
        await using var factory = new ProcureAccessApiFactory(
            _connectionString,
            api.Clock,
            environment,
            new Dictionary<string, string?>
            {
                ["RebuildDatabase"] = "true",
                [DatabaseRebuildExtensions.SampleUserPasswordKey] = samplePassword,
            });

        factory.Services.GetRequiredService<IConfiguration>().GetValue<bool>("RebuildDatabase")
            .ShouldBeTrue($"the flag must reach the API in {environment}");
    }

    private async Task<List<string>> UserIdsAsync()
    {
        await using var db = CreateIdentityAccessContext();
        return await db.Users.IgnoreQueryFilters().Select(u => u.Id).ToListAsync(TestContext.Current.CancellationToken);
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
