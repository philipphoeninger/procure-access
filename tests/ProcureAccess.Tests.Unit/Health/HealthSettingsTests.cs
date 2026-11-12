// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.Platform.Health;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Health;

/// <summary>The 🔧 settings of <c>/health/ready</c> (#112, S4-21a, O-S4-70 (a)).</summary>
public sealed class HealthSettingsTests
{
    [Fact]
    public void From_ConfiguredValues_AreRead() =>
        HealthSettings.From(Configuration("00:00:05", "2")).ShouldBe(new HealthSettings(TimeSpan.FromSeconds(5), 2));

    [Theory]
    [InlineData(null, "2")]
    [InlineData("00:00:00", "2")]
    [InlineData("00:00:05", null)]
    [InlineData("00:00:05", "0")]
    public void From_MissingOrImpossibleValue_FailsTheStart(string? timeout, string? concurrentChecks) =>
        Should.Throw<InvalidOperationException>(() => HealthSettings.From(Configuration(timeout, concurrentChecks)))
            .Message.ShouldContain(HealthSettings.SectionName);

    private static IConfiguration Configuration(string? timeout, string? concurrentChecks) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Health:ReadyTimeout"] = timeout,
                ["Health:ReadyConcurrentChecks"] = concurrentChecks,
            })
            .Build();
}
