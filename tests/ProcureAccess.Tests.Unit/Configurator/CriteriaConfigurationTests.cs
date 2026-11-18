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

    [Fact]
    public void ChangeSelection_NewSelection_KeepsStayingRowsDropsLeavingOnesAndAddsNewOnes()
    {
        var list = List(new ConfigurationCriterionDraft(11, PriorityLevel.A, Note: "bleibt"), new ConfigurationCriterionDraft(12, PriorityLevel.B));
        var staying = Criterion(list, 11);
        var later = CreatedAt.AddHours(1);

        list.ChangeSelection(
            null,
            ["web", "softwareOpen"],
            ["base", "audio"],
            ["softwareTest"],
            [new ConfigurationCriterionDraft(11, PriorityLevel.B, Note: "bleibt"), new ConfigurationCriterionDraft(13, PriorityLevel.Info)],
            later);

        list.ShouldSatisfyAllConditions(
            l => l.UseCaseKey.ShouldBeNull(),
            l => l.AppTypes.Select(v => v.AppTypeKey).ShouldBe(["web", "softwareOpen"]),
            l => l.ProductParts.Select(v => v.ProductPartKey).ShouldBe(["base", "audio"]),
            l => l.TestTypes.Select(v => v.TestTypeKey).ShouldBe(["softwareTest"]),
            l => l.Criteria.Select(c => c.CriterionRevisionId).ShouldBe([11, 13]),
            l => l.ModifiedAt.ShouldBe(later),
            l => l.CreatedAt.ShouldBe(CreatedAt));
        // The same row object: EF updates it instead of deleting and inserting the same key.
        Criterion(list, 11).ShouldBeSameAs(staying);
        staying.ShouldSatisfyAllConditions(c => c.ResolvedLevel.ShouldBe(PriorityLevel.B), c => c.NoteDe.ShouldBe("bleibt"));
    }

    [Fact]
    public void ChangeSelection_ImpossibleDecision_ThrowsAndKeepsTheRecommendation()
    {
        var list = List(new ConfigurationCriterionDraft(11, PriorityLevel.B, OverrideWeight: 4));

        Should.Throw<ArgumentException>(() => list.ChangeSelection(
            "hr", ["web"], ["base"], [], [new ConfigurationCriterionDraft(11, PriorityLevel.A, OverrideWeight: 4)], CreatedAt));
        Should.Throw<ArgumentException>(() => list.ChangeSelection("hr", [], ["base"], [], [], CreatedAt));

        Criterion(list, 11).ShouldSatisfyAllConditions(c => c.ResolvedLevel.ShouldBe(PriorityLevel.B), c => c.OverrideWeight.ShouldBe(4));
    }

    [Theory]
    [InlineData(true, null, null, null, false)]
    [InlineData(false, null, null, null, true)]
    [InlineData(true, PriorityLevel.Info, null, null, true)]
    [InlineData(true, null, 3, null, true)]
    [InlineData(true, null, null, "Notiz", true)]
    public void HasDecision_RowWithoutOrWithAChange_SaysWhetherTheUserChangedIt(
        bool isIncluded, PriorityLevel? level, int? weight, string? note, bool expected)
    {
        var list = List(new ConfigurationCriterionDraft(12, PriorityLevel.B, isIncluded, level, weight, note));

        Criterion(list, 12).HasDecision.ShouldBe(expected);
    }

    [Fact]
    public void CopyForProject_OrganisationList_CopiesSelectionAndDecisionsAndLeavesTheSource()
    {
        var source = List(
            new ConfigurationCriterionDraft(11, PriorityLevel.A, IsIncluded: false, Note: "nicht bei uns"),
            new ConfigurationCriterionDraft(12, PriorityLevel.B, OverrideWeight: 5));
        var project = Guid.NewGuid();
        var later = CreatedAt.AddDays(2);

        var copy = source.CopyForProject(project, "user-2", later);

        copy.ShouldSatisfyAllConditions(
            c => c.Id.ShouldNotBe(source.Id),
            c => c.ProcurementProjectId.ShouldBe(project),
            c => c.IsProjectCopy.ShouldBeTrue(),
            c => c.OrganizationId.ShouldBe(Organization),
            c => c.Name.ShouldBe("Vergabe 2027"),
            c => c.ReleaseVersion.ShouldBe("T.1"),
            c => c.UseCaseKey.ShouldBe("hr"),
            c => c.AppTypes.Select(v => v.AppTypeKey).ShouldBe(["web"]),
            c => c.ProductParts.Select(v => v.ProductPartKey).ShouldBe(["base"]),
            c => c.TestTypes.ShouldBeEmpty(),
            c => c.CreatedByUserId.ShouldBe("user-2"),
            c => c.CreatedAt.ShouldBe(later),
            c => c.ModifiedAt.ShouldBe(later));
        Criterion(copy, 11).ShouldSatisfyAllConditions(
            c => c.IsIncluded.ShouldBeFalse(), c => c.ResolvedLevel.ShouldBe(PriorityLevel.A), c => c.NoteDe.ShouldBe("nicht bei uns"));
        Criterion(copy, 12).ShouldSatisfyAllConditions(
            c => c.ResolvedLevel.ShouldBe(PriorityLevel.B), c => c.OverrideWeight.ShouldBe(5));

        // Rows of their own: a decision on the copy leaves the source as it was (D1).
        copy.Decide(11, isIncluded: true, overrideLevel: null, overrideWeight: null, note: null, later);
        Criterion(source, 11).IsIncluded.ShouldBeFalse();
        source.ShouldSatisfyAllConditions(
            s => s.ProcurementProjectId.ShouldBeNull(), s => s.IsProjectCopy.ShouldBeFalse(), s => s.ModifiedAt.ShouldBe(CreatedAt));
    }

    [Fact]
    public void CopyForProject_ProjectCopyAsSource_Throws()
    {
        var copy = List(new ConfigurationCriterionDraft(11, PriorityLevel.A)).CopyForProject(Guid.NewGuid(), "user-1", CreatedAt);

        Should.Throw<InvalidOperationException>(() => copy.CopyForProject(Guid.NewGuid(), "user-1", CreatedAt));
        Should.Throw<ArgumentException>(() => List().CopyForProject(Guid.Empty, "user-1", CreatedAt));
    }

    [Fact(DisplayName = "INV:WSP-03 " + nameof(ChangeSelection_ProjectCopy_ThrowsAndKeepsTheSelection))]
    [Trait("Invariant", "WSP-03")]
    public void ChangeSelection_ProjectCopy_ThrowsAndKeepsTheSelection()
    {
        var copy = List(new ConfigurationCriterionDraft(11, PriorityLevel.A)).CopyForProject(Guid.NewGuid(), "user-1", CreatedAt);

        Should.Throw<InvalidOperationException>(() => copy.ChangeSelection(
            null, ["web", "softwareOpen"], ["base"], [], [new ConfigurationCriterionDraft(11, PriorityLevel.A)], CreatedAt.AddHours(1)));

        copy.ShouldSatisfyAllConditions(
            c => c.UseCaseKey.ShouldBe("hr"),
            c => c.AppTypes.Select(v => v.AppTypeKey).ShouldBe(["web"]),
            c => c.ModifiedAt.ShouldBe(CreatedAt));
    }

    private static CriteriaConfiguration List(params ConfigurationCriterionDraft[] criteria) =>
        CriteriaConfiguration.Create(Organization, "Vergabe 2027", "user-1", CreatedAt, "T.1", "hr", ["web"], ["base"], [], criteria);

    private static ConfigurationCriterion Criterion(CriteriaConfiguration list, int revisionId) =>
        list.Criteria.Single(c => c.CriterionRevisionId == revisionId);
}
