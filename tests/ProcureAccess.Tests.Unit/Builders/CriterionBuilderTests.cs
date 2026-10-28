// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using MODELS.ProcureAccess.Entities;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Shared.Builders.TestData;

namespace ProcureAccess.Tests.Unit.Builders;

public class CriterionBuilderTests
{
    [Fact]
    public void Build_WithoutOverrides_ReturnsValidDeterministicDefaults()
    {
        var first = ACriterion().Build();
        var second = ACriterion().Build();

        first.Name.ShouldNotBeNullOrWhiteSpace();
        first.IsDeleted.ShouldBeFalse();
        first.CreatedAt.ShouldBe(DefaultCreatedAt);
        var firstFilter = first.CriteriaFilter.ShouldNotBeNull();
        firstFilter.FilterType.ShouldNotBeNull();
        second.ShouldSatisfyAllConditions(
            () => second.Name.ShouldBe(first.Name),
            () => second.Description.ShouldBe(first.Description),
            () => second.CriteriaFilter.ShouldNotBeNull().Name.ShouldBe(firstFilter.Name));
    }

    [Fact]
    public void Build_WithOverrides_ChangesOnlyTheGivenValues()
    {
        var defaults = ACriterion().Build();

        Criterion criterion = ACriterion().WithName("Kontrast").InCriteriaFilter(7).Deleted();

        criterion.Name.ShouldBe("Kontrast");
        criterion.CriteriaFilterId.ShouldBe(7);
        criterion.CriteriaFilter.ShouldBeNull();
        criterion.IsDeleted.ShouldBeTrue();
        criterion.Description.ShouldBe(defaults.Description);
    }
}
