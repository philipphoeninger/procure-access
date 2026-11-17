// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.IdentityAccess.Session;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.IdentityAccess;

/// <summary>
/// <see cref="SessionSettings.From"/> (S5-07, S5-29, ARCHITECTURE §9.1): the 🔧 session lifetimes are
/// read from <c>Session:*</c>, and a missing or unusable value stops the start.
/// </summary>
public class SessionSettingsTests
{
    [Fact]
    public void From_Defaults_ReadsTheFourValues()
    {
        var settings = SessionSettings.From(Configuration());

        settings.IdleTimeout.ShouldBe(TimeSpan.FromMinutes(30));
        settings.AbsoluteTimeout.ShouldBe(TimeSpan.FromHours(8));
        settings.WarningLead.ShouldBe(TimeSpan.FromMinutes(5));
        settings.SecurityStampValidationInterval.ShouldBe(TimeSpan.Zero);
    }

    [Theory]
    [InlineData(null, "08:00:00")]
    [InlineData("", "08:00:00")]
    [InlineData("thirty", "08:00:00")]
    [InlineData("00:00:00", "08:00:00")]
    [InlineData("-00:30:00", "08:00:00")]
    [InlineData("00:30:00", null)]
    public void From_MissingOrUnusableValue_Throws(string? idle, string? absolute) =>
        Should.Throw<InvalidOperationException>(() => SessionSettings.From(Configuration(idle, absolute)))
              .Message.ShouldContain("Session:");

    [Fact]
    public void From_AbsoluteShorterThanIdle_Throws() =>
        Should.Throw<InvalidOperationException>(() => SessionSettings.From(Configuration(idle: "01:00:00", absolute: "00:30:00")));

    [Theory]
    [InlineData(null)]
    [InlineData("soon")]
    [InlineData("00:00:00")]
    [InlineData("-00:05:00")]
    [InlineData("00:30:00")] // not shorter than the idle limit: the warning would show at once
    public void From_UnusableWarningLead_Throws(string? warningLead) =>
        Should.Throw<InvalidOperationException>(() => SessionSettings.From(Configuration(warningLead: warningLead)))
              .Message.ShouldContain("Session:WarningLead");

    [Theory]
    [InlineData(null)]
    [InlineData("often")]
    [InlineData("-00:00:01")]
    public void From_UnusableSecurityStampInterval_Throws(string? interval) =>
        Should.Throw<InvalidOperationException>(() => SessionSettings.From(Configuration(stampInterval: interval)))
              .Message.ShouldContain("Session:SecurityStampValidationInterval");

    [Fact]
    public void From_SecurityStampIntervalAboveZero_IsRead() =>
        SessionSettings.From(Configuration(stampInterval: "00:01:00")).SecurityStampValidationInterval.ShouldBe(TimeSpan.FromMinutes(1));

    private static IConfiguration Configuration(
        string? idle = "00:30:00", string? absolute = "08:00:00", string? warningLead = "00:05:00", string? stampInterval = "00:00:00") =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Session:IdleTimeout"] = idle,
            ["Session:AbsoluteTimeout"] = absolute,
            ["Session:WarningLead"] = warningLead,
            ["Session:SecurityStampValidationInterval"] = stampInterval,
        }).Build();
}
