// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Domain;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Configurator;

/// <summary>
/// The saved criteria list as a domain object (S5-19, ARCHITECTURE §5.3): CAT-08 — a decision never
/// changes the recommended level — and the rules a list and a decision must meet.
/// </summary>
public class CriteriaConfigurationTests
{
    private static readonly Guid Organization = Guid.NewGuid();
    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 16, 9, 0, 0, TimeSpan.Zero);

    [Fact(DisplayName = "INV:CAT-08 " + nameof(Decide_OverrideSetChangedAndRemoved_KeepsTheResolvedLevel))]
    [Trait("Invariant", "CAT-08")]
    public void Decide_OverrideSetChangedAndRemoved_KeepsTheResolvedLevel()
    {
        var list = List(new ConfigurationCriterionDraft(11, PriorityLevel.A), new ConfigurationCriterionDraft(12, PriorityLevel.B));
        var later = CreatedAt.AddHours(1);

        list.Decide(11, isIncluded: true, PriorityLevel.B, overrideWeight: 7, note: null, later);
        Criterion(list, 11).ShouldSatisfyAllConditions(
            c => c.ResolvedLevel.ShouldBe(PriorityLevel.A),
            c => c.OverrideLevel.ShouldBe(PriorityLevel.B),
            c => c.OverrideWeight.ShouldBe(7),
            c => c.EffectiveLevel.ShouldBe(PriorityLevel.B));

        list.Decide(11, isIncluded: false, PriorityLevel.Info, overrideWeight: null, note: "Nicht relevant", later);
        Criterion(list, 11).ShouldSatisfyAllConditions(
            c => c.ResolvedLevel.ShouldBe(PriorityLevel.A),
            c => c.OverrideLevel.ShouldBe(PriorityLevel.Info),
            c => c.IsIncluded.ShouldBeFalse());

        list.Decide(11, isIncluded: true, overrideLevel: null, overrideWeight: null, note: null, later);
        Criterion(list, 11).ShouldSatisfyAllConditions(
            c => c.ResolvedLevel.ShouldBe(PriorityLevel.A),
            c => c.OverrideLevel.ShouldBeNull(),
            c => c.EffectiveLevel.ShouldBe(PriorityLevel.A));
        Criterion(list, 12).ResolvedLevel.ShouldBe(PriorityLevel.B);
        list.ModifiedAt.ShouldBe(later);
        list.CreatedAt.ShouldBe(CreatedAt);
    }

    [Fact(DisplayName = "INV:CAT-08 " + nameof(Create_DraftWithOverride_StoresRecommendedAndDecidedSideBySide))]
    [Trait("Invariant", "CAT-08")]
    public void Create_DraftWithOverride_StoresRecommendedAndDecidedSideBySide()
    {
        var list = List(new ConfigurationCriterionDraft(11, PriorityLevel.Info, OverrideLevel: PriorityLevel.A, Note: "Pflicht bei uns"));

        Criterion(list, 11).ShouldSatisfyAllConditions(
            c => c.ResolvedLevel.ShouldBe(PriorityLevel.Info),
            c => c.OverrideLevel.ShouldBe(PriorityLevel.A),
            c => c.NoteDe.ShouldBe("Pflicht bei uns"));
        list.ModifiedAt.ShouldBe(CreatedAt);
    }

    [Fact]
    public void Decide_LevelEqualToTheRecommendationAndEmptyNote_CountAsNoChoice()
    {
        var list = List(new ConfigurationCriterionDraft(12, PriorityLevel.B));

        list.Decide(12, isIncluded: true, PriorityLevel.B, overrideWeight: 3, note: "", CreatedAt);

        Criterion(list, 12).ShouldSatisfyAllConditions(
            c => c.OverrideLevel.ShouldBeNull(),
            c => c.OverrideWeight.ShouldBe(3),
            c => c.NoteDe.ShouldBeNull());
    }

    [Fact]
    public void Decide_ImpossibleDecision_ThrowsAndChangesNothing()
    {
        var list = List(new ConfigurationCriterionDraft(11, PriorityLevel.A));

        Should.Throw<ArgumentException>(() => list.Decide(11, true, null, overrideWeight: 5, null, CreatedAt.AddHours(1)))
              .Message.ShouldContain("only while the level is B");
        Should.Throw<ArgumentOutOfRangeException>(() => list.Decide(11, true, PriorityLevel.B, overrideWeight: 0, null, CreatedAt.AddHours(1)));
        Should.Throw<ArgumentOutOfRangeException>(() => list.Decide(11, true, (PriorityLevel)9, null, null, CreatedAt.AddHours(1)));
        Should.Throw<ArgumentException>(() => list.Decide(
            11, true, null, null, new string('n', ConfigurationCriterion.NoteMaxLength + 1), CreatedAt.AddHours(1)));
        Should.Throw<ArgumentException>(() => list.Decide(99, true, null, null, null, CreatedAt.AddHours(1)))
              .Message.ShouldContain("not a criterion of this list");

        Criterion(list, 11).ShouldSatisfyAllConditions(
            c => c.OverrideLevel.ShouldBeNull(), c => c.OverrideWeight.ShouldBeNull(), c => c.NoteDe.ShouldBeNull());
        list.ModifiedAt.ShouldBe(CreatedAt);
    }

    [Fact]
    public void Create_ValidSelection_KeepsItsValuesAndANoteOfTheLongestLength()
    {
        var note = new string('n', ConfigurationCriterion.NoteMaxLength);

        var list = CriteriaConfiguration.Create(
            Organization, new string('L', CriteriaConfiguration.NameMaxLength), "user-1", CreatedAt, "T.1", useCaseKey: null,
            ["web", "mobile"], [], ["softwareTest"], [new ConfigurationCriterionDraft(11, PriorityLevel.A, Note: note)]);

        list.ShouldSatisfyAllConditions(
            l => l.Id.ShouldNotBe(Guid.Empty),
            l => l.OrganizationId.ShouldBe(Organization),
            l => l.ReleaseVersion.ShouldBe("T.1"),
            l => l.UseCaseKey.ShouldBeNull(),
            l => l.CreatedByUserId.ShouldBe("user-1"),
            l => l.AppTypes.Select(v => v.AppTypeKey).ShouldBe(["web", "mobile"]),
            l => l.ProductParts.ShouldBeEmpty(),
            l => l.TestTypes.Select(v => v.TestTypeKey).ShouldBe(["softwareTest"]),
            l => l.Criteria.Single().NoteDe.ShouldBe(note));
    }

    [Fact]
    public void Create_InvalidList_Throws()
    {
        ConfigurationCriterionDraft[] one = [new(11, PriorityLevel.A)];

        Should.Throw<ArgumentException>(() => Create(organization: Guid.Empty));
        Should.Throw<ArgumentException>(() => Create(name: " "));
        Should.Throw<ArgumentException>(() => Create(name: new string('L', CriteriaConfiguration.NameMaxLength + 1)));
        Should.Throw<ArgumentException>(() => Create(user: ""));
        Should.Throw<ArgumentException>(() => Create(release: new string('9', CriteriaConfiguration.ReleaseVersionMaxLength + 1)));
        Should.Throw<ArgumentException>(() => Create(appTypes: [])).Message.ShouldContain("at least one app type");
        Should.Throw<ArgumentException>(() => Create(appTypes: ["web", "web"]));
        Should.Throw<ArgumentException>(() => Create(appTypes: [new string('k', CriteriaConfiguration.ValueKeyMaxLength + 1)]));
        Should.Throw<ArgumentException>(() => Create(criteria: [.. one, .. one])).Message.ShouldContain("more than once");
        Should.Throw<ArgumentException>(() => Create(criteria: [new(11, PriorityLevel.A, OverrideWeight: 4)]));

        CriteriaConfiguration Create(
            Guid? organization = null, string name = "Liste", string user = "user-1", string release = "T.1",
            string[]? appTypes = null, ConfigurationCriterionDraft[]? criteria = null) =>
            CriteriaConfiguration.Create(
                organization ?? Organization, name, user, CreatedAt, release, "hr", appTypes ?? ["web"], ["base"], [], criteria ?? one);
    }

    private static CriteriaConfiguration List(params ConfigurationCriterionDraft[] criteria) =>
        CriteriaConfiguration.Create(Organization, "Vergabe 2027", "user-1", CreatedAt, "T.1", "hr", ["web"], ["base"], [], criteria);

    private static ConfigurationCriterion Criterion(CriteriaConfiguration list, int revisionId) =>
        list.Criteria.Single(c => c.CriterionRevisionId == revisionId);
}
