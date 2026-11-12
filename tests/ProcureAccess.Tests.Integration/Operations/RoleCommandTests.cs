// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using API.ProcureAccess.Commands;
using DAL.ProcureAccess.EFStructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MODELS.ProcureAccess.Entities;
using MODELS.ProcureAccess.Entities.Authorization;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// The <c>roles</c> command (SPEC R-ADM-03, S4-25) on the migrated test database, through the
/// services of the API host: it grants and revokes the platform roles of migration
/// <c>RenamePlatformRoles</c> by e-mail address and lists them. Usage errors are unit tests
/// (<c>RoleCommandLineTests</c>).
/// </summary>
public sealed class RoleCommandTests(ApiFixture api) : IntegrationTest(api)
{
    private const string Alice = "alice@example.test";
    private const string Bob = "bob@example.test";

    [Fact]
    public async Task Grant_KnownUser_AddsTheRole()
    {
        await StoreUsersAsync();

        var result = await RunAsync($"roles grant {Alice} Moderator");

        result.ExitCode.ShouldBe(RoleCommandLine.Success);
        result.Output.ShouldStartWith($"Granted the role Moderator to {Alice}.");
        result.Error.ShouldBeEmpty();
        (await RolesOfAsync(Alice)).ShouldBe([Roles.Moderator]);
    }

    [Fact]
    public async Task Grant_RoleNameInOtherCase_GrantsThePlatformRole()
    {
        await StoreUsersAsync();

        var result = await RunAsync($"roles grant {Alice} platformadmin");

        result.ExitCode.ShouldBe(RoleCommandLine.Success);
        (await RolesOfAsync(Alice)).ShouldBe([Roles.PlatformAdmin]);
    }

    [Fact]
    public async Task Grant_RoleTheUserHolds_ChangesNothingAndSucceeds()
    {
        await StoreUsersAsync();
        await RunAsync($"roles grant {Alice} CatalogEditor");

        var result = await RunAsync($"roles grant {Alice} CatalogEditor");

        result.ExitCode.ShouldBe(RoleCommandLine.Success);
        result.Output.ShouldBe($"{Alice} already has the role CatalogEditor; nothing changed.{Environment.NewLine}");
        (await RolesOfAsync(Alice)).ShouldBe([Roles.CatalogEditor]);
    }

    [Fact]
    public async Task Revoke_RoleTheUserHolds_RemovesOnlyThatRole()
    {
        await StoreUsersAsync();
        await RunAsync($"roles grant {Alice} Moderator");
        await RunAsync($"roles grant {Alice} PlatformAdmin");

        var result = await RunAsync($"roles revoke {Alice} Moderator");

        result.ExitCode.ShouldBe(RoleCommandLine.Success);
        result.Output.ShouldStartWith($"Revoked the role Moderator from {Alice}.");
        (await RolesOfAsync(Alice)).ShouldBe([Roles.PlatformAdmin]);
    }

    [Fact]
    public async Task Revoke_RoleTheUserLacks_ChangesNothingAndSucceeds()
    {
        await StoreUsersAsync();

        var result = await RunAsync($"roles revoke {Alice} Moderator");

        result.ExitCode.ShouldBe(RoleCommandLine.Success);
        result.Output.ShouldBe($"{Alice} does not have the role Moderator; nothing changed.{Environment.NewLine}");
    }

    [Theory]
    [InlineData("grant")]
    [InlineData("revoke")]
    public async Task GrantOrRevoke_UnknownEmail_RefusesWithExit1(string action)
    {
        await StoreUsersAsync();

        var result = await RunAsync($"roles {action} nobody@example.test Moderator");

        result.ExitCode.ShouldBe(RoleCommandLine.Failure);
        result.Error.ShouldBe($"There is no user with the e-mail address nobody@example.test.{Environment.NewLine}");
        result.Output.ShouldBeEmpty();
    }

    [Fact]
    public async Task List_UsersWithPlatformRoles_ListsEachWithItsRolesButNotMember()
    {
        await StoreUsersAsync(memberRoleFor: Bob);
        await RunAsync($"roles grant {Bob} Moderator");
        await RunAsync($"roles grant {Alice} PlatformAdmin");
        await RunAsync($"roles grant {Alice} CatalogEditor");

        var result = await RunAsync("roles list");

        result.ExitCode.ShouldBe(RoleCommandLine.Success);
        result.Output.ShouldBe(
            $"{Alice}: CatalogEditor, PlatformAdmin{Environment.NewLine}" +
            $"{Bob}: Moderator{Environment.NewLine}");
    }

    [Fact]
    public async Task List_NoUserWithAPlatformRole_SaysSo()
    {
        await StoreUsersAsync(memberRoleFor: Bob);

        var result = await RunAsync("roles list");

        result.ExitCode.ShouldBe(RoleCommandLine.Success);
        result.Output.ShouldBe($"No user has a platform role.{Environment.NewLine}");
    }

    [Fact]
    public async Task ListOne_UserWithAndWithoutPlatformRole_PrintsItsPlatformRolesOnly()
    {
        await StoreUsersAsync(memberRoleFor: Bob);
        await RunAsync($"roles grant {Alice} Moderator");

        (await RunAsync($"roles list {Alice}")).Output.ShouldBe($"{Alice}: Moderator{Environment.NewLine}");
        (await RunAsync($"roles list {Bob}")).Output.ShouldBe($"{Bob} has no platform role.{Environment.NewLine}");
    }

    /// <summary>Stores Alice and Bob; the Member role is not in a fresh database, so it is added when asked for.</summary>
    private async Task StoreUsersAsync(string? memberRoleFor = null)
    {
        await Api.ResetDatabaseAsync();
        await StoreAsync(UserRow(Alice), UserRow(Bob));
        if (memberRoleFor is not null)
        {
            await using var scope = Api.Factory.Services.CreateAsyncScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roleManager.RoleExistsAsync(Roles.Member))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Member));
            }

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByEmailAsync(memberRoleFor);
            await userManager.AddToRoleAsync(user!, Roles.Member);
        }
    }

    private static User UserRow(string email) => new()
    {
        UserName = email,
        NormalizedUserName = email.ToUpperInvariant(),
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        SecurityStamp = Guid.NewGuid().ToString(),
    };

    private async Task<(int ExitCode, string Output, string Error)> RunAsync(string commandLine)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var exitCode = await RoleCommandLine.RunAsync(
            Api.Factory.Services,
            commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries),
            output,
            error,
            TestContext.Current.CancellationToken);
        return (exitCode, output.ToString(), error.ToString());
    }

    private async Task<List<string?>> RolesOfAsync(string email)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        return await (
            from user in db.Users
            join userRole in db.UserRoles on user.Id equals userRole.UserId
            join role in db.Roles on userRole.RoleId equals role.Id
            where user.NormalizedEmail == email.ToUpperInvariant()
            orderby role.Name
            select role.Name).ToListAsync(TestContext.Current.CancellationToken);
    }
}
