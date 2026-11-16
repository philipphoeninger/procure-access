// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.Platform.Sharing;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Sharing;

/// <summary>The 🔧 settings of share links (SPEC SD15; S5-23, S5-24 owner decision 1a).</summary>
public sealed class ShareLinkSettingsTests
{
    [Fact]
    public void From_ConfiguredValues_AreRead()
    {
        var settings = ShareLinkSettings.From(Configuration("12", "90", "Europe/Berlin"));

        settings.MaxExpiryMonths.ShouldBe(12);
        settings.DefaultExpiryDays.ShouldBe(90);
        settings.TimeZone.ShouldBe(TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin"));
    }

    [Theory]
    [InlineData(null, "90", "Europe/Berlin")]
    [InlineData("0", "90", "Europe/Berlin")]
    [InlineData("12", null, "Europe/Berlin")]
    [InlineData("12", "0", "Europe/Berlin")]
    [InlineData("3", "85", "Europe/Berlin")]
    [InlineData("12", "90", null)]
    [InlineData("12", "90", "Europe/Nowhere")]
    public void From_MissingOrImpossibleValue_FailsTheStart(string? months, string? defaultDays, string? zone) =>
        Should.Throw<InvalidOperationException>(() => ShareLinkSettings.From(Configuration(months, defaultDays, zone)))
            .Message.ShouldContain(ShareLinkSettings.SectionName);

    private static IConfiguration Configuration(string? months, string? defaultDays, string? zone) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ShareLinks:MaxExpiryMonths"] = months,
                ["ShareLinks:DefaultExpiryDays"] = defaultDays,
                ["ShareLinks:TimeZone"] = zone,
            })
            .Build();
}
