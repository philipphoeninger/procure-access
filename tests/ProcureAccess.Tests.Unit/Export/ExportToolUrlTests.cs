// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.Export.Formatting;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Export;

/// <summary>🔧 <c>Export:ToolUrl</c> (owner decision O-S4-64 (a)).</summary>
public class ExportToolUrlTests
{
    [Theory]
    [InlineData("https://procureaccess.de")]
    [InlineData("http://localhost:4200")]
    public void From_AbsoluteHttpUrl_IsTaken(string value) =>
        ExportToolUrl.From(Settings(value)).Value.ShouldBe(new Uri(value));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("procureaccess.de")]
    [InlineData("mailto:info@example.invalid")]
    [InlineData("javascript:alert(1)")]
    public void From_MissingOrNotAnHttpUrl_StopsTheStart(string? value) =>
        Should.Throw<InvalidOperationException>(() => ExportToolUrl.From(Settings(value)))
            .Message.ShouldContain(ExportToolUrl.SettingName);

    private static IConfiguration Settings(string? value) =>
        new ConfigurationBuilder().AddInMemoryCollection([new(ExportToolUrl.SettingName, value)]).Build();
}
