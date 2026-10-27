// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using API.ProcureAccess.Extensions;
using DAL.ProcureAccess.EFStructures;
using DAL.ProcureAccess.Initialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MODELS.ProcureAccess.Entities;
using MODELS.ProcureAccess.Entities.Authorization;
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
    public async Task Startup_RebuildInDevelopmentWithSamplePassword_CreatesSampleUserWithThatPasswordAsMemberOnly()
    {
        await CreateDatabaseWithUserAsync();

        await StartApiWithRebuildFlagAsync("Development", SampleUserPassword);

        await using var db = ApiFixture.CreateDbContext(_connectionString);
        var user = await db.Users.SingleAsync(TestContext.Current.CancellationToken);
        user.Email.ShouldBe(SampleData.SampleUserEmail);
        new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash!, SampleUserPassword)
            .ShouldNotBe(PasswordVerificationResult.Failed);

        var roles = await db.UserRoles
            .Where(userRole => userRole.UserId == user.Id)
            .Join(db.Roles, userRole => userRole.RoleId, role => role.Id, (_, role) => role.Name)
            .ToListAsync(TestContext.Current.CancellationToken);
        roles.ShouldBe([Roles.Member]);
    }

    [Fact]
    public async Task Startup_RebuildInDevelopmentWithoutSamplePassword_CreatesNoUser()
    {
        await CreateDatabaseWithUserAsync();

        await StartApiWithRebuildFlagAsync("Development", samplePassword: null);

        (await UserIdsAsync()).ShouldBeEmpty();
    }

    private async Task<User> CreateDatabaseWithUserAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = ApiFixture.CreateDbContext(_connectionString);
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
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
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
        await using var db = ApiFixture.CreateDbContext(_connectionString);
        return await db.Users.IgnoreQueryFilters().Select(u => u.Id).ToListAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await using var db = ApiFixture.CreateDbContext(_connectionString);
        await db.Database.EnsureDeletedAsync();
    }
}
