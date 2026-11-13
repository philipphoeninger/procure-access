// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.IdentityAccess.Session;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.IdentityAccess;

/// <summary>
/// <see cref="SessionSettings.From"/> (S5-07, ARCHITECTURE §9.1): the 🔧 session lifetimes are
/// read from <c>Session:*</c>, and a missing or unusable value stops the start.
/// </summary>
public class SessionSettingsTests
{
    [Fact]
    public void From_Defaults_Reads30MinutesIdleAnd8HoursAbsolute()
    {
        var settings = SessionSettings.From(Configuration(idle: "00:30:00", absolute: "08:00:00"));

        settings.IdleTimeout.ShouldBe(TimeSpan.FromMinutes(30));
        settings.AbsoluteTimeout.ShouldBe(TimeSpan.FromHours(8));
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

    private static IConfiguration Configuration(string? idle, string? absolute) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Session:IdleTimeout"] = idle,
            ["Session:AbsoluteTimeout"] = absolute,
        }).Build();
}
