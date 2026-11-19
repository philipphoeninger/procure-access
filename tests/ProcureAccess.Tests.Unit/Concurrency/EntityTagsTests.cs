// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.Platform.Concurrency;
using ProcureAccess.Modules.Platform.Contracts.Concurrency;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Concurrency;

/// <summary>
/// The ETag of a shared resource (ARCHITECTURE §7.1; API-03, API-04; S6-20): the 🔧 epoch and the
/// version, compared with <c>If-Match</c> as one strong tag.
/// </summary>
public class EntityTagsTests
{
    [Fact(DisplayName = "INV:API-04 " + nameof(Of_Version_IsAQuotedTagOfEpochAndVersion))]
    [Trait("Invariant", "API-04")]
    public void Of_Version_IsAQuotedTagOfEpochAndVersion()
    {
        new EntityTags(1).Of(1).ShouldBe("\"1-1\"");
        new EntityTags(12).Of(345).ShouldBe("\"12-345\"");
    }

    [Fact(DisplayName = "INV:API-04 " + nameof(Matches_TagIssuedUnderAnEarlierEpoch_IsStaleForEveryVersion))]
    [Trait("Invariant", "API-04")]
    public void Matches_TagIssuedUnderAnEarlierEpoch_IsStaleForEveryVersion()
    {
        var before = new EntityTags(1);
        var after = new EntityTags(2);

        foreach (var version in new[] { 1, 2, 12, 21 })
        {
            before.Matches(before.Of(version), version).ShouldBeTrue();
            after.Matches(before.Of(version), version).ShouldBeFalse();
            after.Matches(after.Of(version), version).ShouldBeTrue();
        }

        // Epoch and version do not run into each other: "1-21" is not "12-1".
        new EntityTags(12).Matches(before.Of(21), 1).ShouldBeFalse();
    }

    [Theory(DisplayName = "INV:API-03 " + nameof(Matches_OnlyTheOneStrongTagOfTheVersion))]
    [Trait("Invariant", "API-03")]
    [InlineData("\"1-3\"", true)]
    [InlineData("  \"1-3\" ", true)]
    [InlineData("\"1-2\"", false)]
    [InlineData("\"1-4\"", false)]
    [InlineData("1-3", false)]
    [InlineData("W/\"1-3\"", false)]
    [InlineData("\"1-3\", \"1-4\"", false)]
    [InlineData("*", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Matches_OnlyTheOneStrongTagOfTheVersion(string? ifMatch, bool expected) =>
        new EntityTags(1).Matches(ifMatch, 3).ShouldBe(expected);

    [Fact(DisplayName = "INV:API-04 " + nameof(EntityTagsFrom_ConfiguredEpoch_IsPartOfEveryTag))]
    [Trait("Invariant", "API-04")]
    public void EntityTagsFrom_ConfiguredEpoch_IsPartOfEveryTag() =>
        ConcurrencySettings.EntityTagsFrom(Configuration("7")).Of(1).ShouldBe("\"7-1\"");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-1")]
    public void EntityTagsFrom_MissingEpochOrBelowOne_StopsTheStart(string? epoch) =>
        Should.Throw<InvalidOperationException>(() => ConcurrencySettings.EntityTagsFrom(Configuration(epoch)))
              .Message.ShouldContain("Concurrency:ETagEpoch");

    private static IConfiguration Configuration(string? epoch) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Concurrency:ETagEpoch"] = epoch })
            .Build();
}
