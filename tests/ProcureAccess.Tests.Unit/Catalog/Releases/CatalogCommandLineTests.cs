// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Catalog.Releases;

/// <summary>
/// The command mode of the API host (owner, S3-16 decision 1): which arguments start a catalog
/// command, and the usage error for every other form. Import itself is tested in
/// <c>CatalogImportTests</c> (integration).
/// </summary>
public class CatalogCommandLineTests
{
    [Theory]
    [InlineData("catalog", true)]
    [InlineData("catalog import 2026.1", true)]
    [InlineData("", false)]
    [InlineData("--urls http://localhost:5000", false)]
    [InlineData("Catalog import 2026.1", false)]
    public void IsCommand_Arguments_StartTheCommandOnlyWithCatalogFirst(string commandLine, bool expected) =>
        CatalogModule.IsCommand(Split(commandLine)).ShouldBe(expected);

    [Theory]
    [InlineData("catalog")]
    [InlineData("catalog import")]
    [InlineData("catalog import 2026.1 2026.2")]
    [InlineData("catalog activate 2026.1")]
    public async Task RunCommand_WrongUsage_PrintsUsageAndExitsWith2(string commandLine)
    {
        await using var services = new ServiceCollection().BuildServiceProvider();
        var output = new StringWriter();
        var error = new StringWriter();

        var exitCode = await CatalogModule.RunCommandAsync(
            services, Split(commandLine), output, error, TestContext.Current.CancellationToken);

        exitCode.ShouldBe(2);
        error.ToString().ShouldStartWith("Usage: catalog import <version>");
        output.ToString().ShouldBeEmpty();
    }

    private static string[] Split(string commandLine) => commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
}
