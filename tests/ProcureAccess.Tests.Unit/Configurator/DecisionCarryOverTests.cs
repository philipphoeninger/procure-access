// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Configurations;
using ProcureAccess.Modules.Configurator.Domain;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Configurator;

/// <summary>
/// How a decision moves to a new selection or a copy on the active release (S5-21a; CAT-22, owner
/// decision S5-21 a): as it is, under the new recommendation; a weight without a chosen level
/// becomes „B chosen“ when the new recommendation is not B.
/// </summary>
public class DecisionCarryOverTests
{
    [Fact(DisplayName = "INV:CAT-22 " + nameof(Carry_WithoutPreviousRow_IsIncludedUnderTheNewRecommendation))]
    [Trait("Invariant", "CAT-22")]
    public void Carry_WithoutPreviousRow_IsIncludedUnderTheNewRecommendation()
    {
        DecisionCarryOver.Carry(21, PriorityLevel.A, previous: null)
            .ShouldBe(new ConfigurationCriterionDraft(21, PriorityLevel.A));
    }

    [Fact(DisplayName = "INV:CAT-22 " + nameof(Carry_Decision_MovesAsItIsUnderTheNewRevisionAndRecommendation))]
    [Trait("Invariant", "CAT-22")]
    public void Carry_Decision_MovesAsItIsUnderTheNewRevisionAndRecommendation()
    {
        var previous = Row(PriorityLevel.B, isIncluded: false, PriorityLevel.Info, weight: null, note: "Mit Anbieter klären");

        DecisionCarryOver.Carry(21, PriorityLevel.A, previous)
            .ShouldBe(new ConfigurationCriterionDraft(21, PriorityLevel.A, false, PriorityLevel.Info, null, "Mit Anbieter klären"));
    }

    [Theory(DisplayName = "INV:CAT-22 " + nameof(Carry_WeightWithoutChosenLevel_IsKeptAsBChosenWhenTheNewRecommendationIsNotB))]
    [Trait("Invariant", "CAT-22")]
    [InlineData(PriorityLevel.A, PriorityLevel.B)]
    [InlineData(PriorityLevel.Info, PriorityLevel.B)]
    [InlineData(PriorityLevel.B, null)]
    public void Carry_WeightWithoutChosenLevel_IsKeptAsBChosenWhenTheNewRecommendationIsNotB(
        PriorityLevel recommendation, PriorityLevel? expectedLevel)
    {
        var previous = Row(PriorityLevel.B, isIncluded: true, level: null, weight: 7, note: null);

        var carried = DecisionCarryOver.Carry(21, recommendation, previous);

        carried.ShouldBe(new ConfigurationCriterionDraft(21, recommendation, true, expectedLevel, 7, null));
        // The draft is one the list accepts.
        Should.NotThrow(() => CriteriaConfiguration.Create(
            Guid.NewGuid(), "Liste", "user-1", DateTimeOffset.UnixEpoch, "T.2", null, ["web"], ["base"], [], [carried]));
    }

    [Fact(DisplayName = "INV:CAT-22 " + nameof(Carry_ChosenLevelEqualToTheNewRecommendation_CountsAsNoChoice))]
    [Trait("Invariant", "CAT-22")]
    public void Carry_ChosenLevelEqualToTheNewRecommendation_CountsAsNoChoice()
    {
        var previous = Row(PriorityLevel.A, isIncluded: true, PriorityLevel.B, weight: 4, note: null);
        var carried = DecisionCarryOver.Carry(21, PriorityLevel.B, previous);

        var list = CriteriaConfiguration.Create(
            Guid.NewGuid(), "Liste", "user-1", DateTimeOffset.UnixEpoch, "T.2", null, ["web"], ["base"], [], [carried]);

        list.Criteria[0].ShouldSatisfyAllConditions(
            c => c.ResolvedLevel.ShouldBe(PriorityLevel.B), c => c.OverrideLevel.ShouldBeNull(), c => c.OverrideWeight.ShouldBe(4));
    }

    private static ConfigurationCriterion Row(PriorityLevel recommendation, bool isIncluded, PriorityLevel? level, int? weight, string? note) =>
        CriteriaConfiguration.Create(
                Guid.NewGuid(), "Liste", "user-1", DateTimeOffset.UnixEpoch, "T.1", null, ["web"], ["base"], [],
                [new ConfigurationCriterionDraft(11, recommendation, isIncluded, level, weight, note)])
            .Criteria[0];
}
