// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using API.ProcureAccess.Commands;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Commands;

/// <summary>
/// The <c>roles</c> command mode of the API host (SPEC R-ADM-03, S4-25): which arguments start it,
/// and the usage error for every other form, before any database access. Granting, revoking and
/// listing are tested in <c>RoleCommandTests</c> (integration).
/// </summary>
public class RoleCommandLineTests
{
    [Theory]
    [InlineData("roles", true)]
    [InlineData("roles grant a@example.test Moderator", true)]
    [InlineData("roles list", true)]
    [InlineData("catalog import 2026.1", false)]
    [InlineData("", false)]
    [InlineData("Roles list", false)]
    public void IsCommand_Arguments_StartTheCommandOnlyWithRolesFirst(string commandLine, bool expected) =>
        RoleCommandLine.IsCommand(Split(commandLine)).ShouldBe(expected);

    [Theory]
    [InlineData("roles")]
    [InlineData("roles list a@example.test extra")]
    [InlineData("roles grant a@example.test")]
    [InlineData("roles grant a@example.test Moderator extra")]
    [InlineData("roles revoke Moderator")]
    [InlineData("roles remove a@example.test Moderator")]
    public async Task Run_WrongUsage_PrintsUsageAndExitsWith2(string commandLine)
    {
        var (exitCode, output, error) = await RunAsync(commandLine);

        exitCode.ShouldBe(RoleCommandLine.UsageError);
        error.ShouldStartWith("Usage: roles list [<email>]");
        error.ShouldContain("Platform roles: PlatformAdmin, CatalogEditor, Moderator.");
        output.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("roles grant a@example.test Member")]
    [InlineData("roles grant a@example.test Approver")]
    [InlineData("roles revoke a@example.test Admin")]
    public async Task Run_RoleThatIsNoPlatformRole_NamesItAndExitsWith2(string commandLine)
    {
        var (exitCode, output, error) = await RunAsync(commandLine);

        exitCode.ShouldBe(RoleCommandLine.UsageError);
        error.ShouldStartWith($"'{Split(commandLine)[3]}' is not a platform role.");
        output.ShouldBeEmpty();
    }

    /// <summary>An empty service provider: a usage error must stop before the database.</summary>
    private static async Task<(int ExitCode, string Output, string Error)> RunAsync(string commandLine)
    {
        await using var services = new ServiceCollection().BuildServiceProvider();
        var output = new StringWriter();
        var error = new StringWriter();
        var exitCode = await RoleCommandLine.RunAsync(
            services, Split(commandLine), output, error, TestContext.Current.CancellationToken);
        return (exitCode, output.ToString(), error.ToString());
    }

    private static string[] Split(string commandLine) => commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
}
