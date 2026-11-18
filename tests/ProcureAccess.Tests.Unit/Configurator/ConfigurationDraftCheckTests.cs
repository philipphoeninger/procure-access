// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using NSubstitute;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Contracts.Drafts;
using ProcureAccess.Modules.Configurator.Drafts;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Configurator;

/// <summary>
/// The draft check shared by the anonymous export and saving a list (C153; S5-20a), on a catalog
/// double. The rules it took over from the export stay covered by <c>AdHocSelectionSourceTests</c>;
/// here what differs by purpose (CAT-18), the checked draft a save stores, and the 🔧 weight range.
/// </summary>
public class ConfigurationDraftCheckTests
{
    private const string Release = "T.1";

    private static readonly ResolvedCatalogCriterion RequirementA = Criterion(11, "99.1", PriorityLevel.A, null);
    private static readonly ResolvedCatalogCriterion RequirementB = Criterion(12, "99.2", PriorityLevel.B, 4);

    private readonly ICatalogSelectionResolver _catalog = Substitute.For<ICatalogSelectionResolver>();

    [Fact(DisplayName = "INV:CAT-18 " + nameof(Check_InactiveUseCaseForANewList_IsAFieldError))]
    [Trait("Invariant", "CAT-18")]
    public async Task Check_InactiveUseCaseForANewList_IsAFieldError()
    {
        Answer(useCaseIsActive: false);

        var result = await CheckAsync(DraftPurpose.Save, Entry(11), Entry(12));

        result.Outcome.ShouldBe(DraftCheckOutcome.Invalid);
        result.Errors.Keys.ShouldBe(["selection.useCaseTemplateId"]);
        result.Draft.ShouldBeNull();
    }

    [Fact(DisplayName = "INV:CAT-18 " + nameof(Check_InactiveUseCaseForTheExport_IsAccepted))]
    [Trait("Invariant", "CAT-18")]
    public async Task Check_InactiveUseCaseForTheExport_IsAccepted()
    {
        // Owner, S3-20: the export of a draft accepts an inactive use case; only a new list refuses it.
        Answer(useCaseIsActive: false);

        var result = await CheckAsync(DraftPurpose.Export, Entry(11), Entry(12));

        result.Outcome.ShouldBe(DraftCheckOutcome.Checked);
    }

    [Fact]
    public async Task Check_InactiveUseCaseWithOtherFieldErrors_NamesThemAll()
    {
        Answer(useCaseIsActive: false);

        var result = await CheckAsync(DraftPurpose.Save, Entry(11, overrideLevel: "C"), Entry(12));

        result.Outcome.ShouldBe(DraftCheckOutcome.Invalid);
        result.Errors.Keys.ShouldBe(["criteria[0].overrideLevel", "selection.useCaseTemplateId"], ignoreOrder: true);
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Check_ValidDraft_PairsEveryCriterionWithItsDecisionInListOrder))]
    [Trait("Invariant", "EXP-07")]
    public async Task Check_ValidDraft_PairsEveryCriterionWithItsDecisionInListOrder()
    {
        Answer(useCaseIsActive: true);

        var result = await CheckAsync(DraftPurpose.Save, Entry(12, included: false, overrideWeight: 9, note: "Notiz"), Entry(11, overrideLevel: "B"));

        result.Outcome.ShouldBe(DraftCheckOutcome.Checked);
        result.Draft!.ReleaseVersion.ShouldBe(Release);
        result.Draft.Criteria.Select(c => c.Criterion.RevisionId).ShouldBe([11, 12]);
        result.Draft.Criteria[0].ShouldSatisfyAllConditions(
            c => c.IsIncluded.ShouldBeTrue(),
            c => c.OverrideLevel.ShouldBe(PriorityLevel.B),
            c => c.Criterion.RecommendedPriority.Level.ShouldBe(PriorityLevel.A));
        result.Draft.Criteria[1].ShouldSatisfyAllConditions(
            c => c.IsIncluded.ShouldBeFalse(),
            c => c.OverrideWeight.ShouldBe(9),
            c => c.Note.ShouldBe("Notiz"));
    }

    [Fact]
    public void WeightRange_FromConfiguration_RefusesAnImpossibleRange()
    {
        Should.Throw<InvalidOperationException>(() => PriorityWeightRange.From(Configuration(("Min", "5"), ("Max", "4"))));
        Should.Throw<InvalidOperationException>(() => PriorityWeightRange.From(Configuration(("Min", "0"), ("Max", "4"))));
        Should.Throw<InvalidOperationException>(() => PriorityWeightRange.From(Configuration(("Max", "4"))));
        PriorityWeightRange.From(Configuration(("Min", "1"), ("Max", "10"))).ShouldBe(new PriorityWeightRange(1, 10));
    }

    private Task<DraftCheckResult> CheckAsync(DraftPurpose purpose, params DraftCriterion[] entries) =>
        new ConfigurationDraftCheck(_catalog, new PriorityWeightRange(1, 10)).CheckAsync(
            new CriteriaDraft(new DraftSelection(Release, "hr", ["web"], ["base"], []), entries),
            purpose,
            TestContext.Current.CancellationToken);

    private void Answer(bool useCaseIsActive) =>
        _catalog.ResolveAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(
            new CatalogSelectionResult(
                CatalogSelectionOutcome.Resolved,
                new CatalogReleaseSummary(Release, true, 15),
                new ResolvedCatalogSelection(
                    new CatalogValueName("hr", "HR-Anwendung"),
                    [new CatalogValueName("web", "Webanwendung")],
                    [new CatalogValueName("base", "Basisfunktionalität")],
                    [],
                    [RequirementA, RequirementB],
                    useCaseIsActive),
                new Dictionary<string, string[]>(StringComparer.Ordinal)));

    private static ResolvedCatalogCriterion Criterion(int revisionId, string key, PriorityLevel level, int? weight) =>
        new(revisionId, key, CriterionKind.Requirement, key, $"Titel {key}", null, null, null, null,
            new CatalogPriority(level, weight, $"Begründung {key}"), null, Rating.All, null);

    private static DraftCriterion Entry(
        int revisionId, bool included = true, string? overrideLevel = null, int? overrideWeight = null, string? note = null) =>
        new(revisionId, included, overrideLevel, overrideWeight, note);

    private static IConfiguration Configuration(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>($"{PriorityWeightRange.SectionName}:{v.Key}", v.Value)))
            .Build();
}
