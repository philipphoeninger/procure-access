// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using NSubstitute;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Export.Documents;
using ProcureAccess.Modules.Export.Requests;
using ProcureAccess.Modules.Export.Sources;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Export;

/// <summary>
/// The request check and re-resolution of the anonymous export (EXP-07; owner decisions O-S4-57,
/// O-S4-58, O-S4-60 (a)), on a catalog double that answers like <c>ICatalogSelectionResolver</c>.
/// The real resolution of the catalog runs in the integration tests.
/// </summary>
public class AdHocSelectionSourceTests
{
    private const string Release = "T.1";
    private const string Sentinel = "SENTINEL-EXPORT-NOTE-3c9d";

    // Three criteria as the catalog resolves them, in list order: A, B with weight 4, Info, and a
    // „Geprüft nach …“ criterion.
    private static readonly ResolvedCatalogCriterion RequirementA = Criterion(11, "99.1", PriorityLevel.A, null);
    private static readonly ResolvedCatalogCriterion RequirementB = Criterion(12, "99.2", PriorityLevel.B, 4);
    private static readonly ResolvedCatalogCriterion RequirementInfo = Criterion(13, "99.3", PriorityLevel.Info, null);
    private static readonly ResolvedCatalogCriterion TestedBy = new(
        14, "PROC-softwareTest", CriterionKind.TestProcedure, null, "Software-Test", null, null, null, null,
        new CatalogPriority(PriorityLevel.Info, null, "Begründung PROC"), "https://example.org/software");

    private static readonly ResolvedCatalogCriterion[] All = [RequirementA, RequirementB, RequirementInfo, TestedBy];

    private readonly ICatalogSelectionResolver _catalog = Substitute.For<ICatalogSelectionResolver>();

    public AdHocSelectionSourceTests() => Answer(Resolved(All));

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Load_ValidRequest_TakesPrioritiesFromTheCatalogAndKeepsChoicesNextToThem))]
    [Trait("Invariant", "EXP-07")]
    public async Task Load_ValidRequest_TakesPrioritiesFromTheCatalogAndKeepsChoicesNextToThem()
    {
        var result = await LoadAsync(Request(
            Entry(11, overrideLevel: "B", overrideWeight: 7),
            Entry(12),
            Entry(13, overrideLevel: "Info", note: "Eine Notiz"),
            Entry(14)));

        result.Problem.ShouldBeNull();
        var rows = result.Content!.Rows;
        rows.Select(r => r.Key).ShouldBe(["99.1", "99.2", "99.3", "PROC-softwareTest"]);
        rows.Select(r => r.RecommendedLevel).ShouldBe([PriorityLevel.A, PriorityLevel.B, PriorityLevel.Info, PriorityLevel.Info]);
        rows[0].ShouldSatisfyAllConditions(
            r => r.ChosenLevel.ShouldBe(PriorityLevel.B),
            r => r.Weight.ShouldBe(7),
            r => r.Rationale.ShouldBe("Begründung 99.1"),
            r => r.ShortDescription.ShouldBe("Kurzbeschreibung 99.1"));
        rows[1].ShouldSatisfyAllConditions(r => r.ChosenLevel.ShouldBeNull(), r => r.Weight.ShouldBe(4));
        rows[2].ShouldSatisfyAllConditions(r => r.ChosenLevel.ShouldBeNull(), r => r.Note.ShouldBe("Eine Notiz"));
        rows[3].ShouldSatisfyAllConditions(r => r.Number.ShouldBeNull(), r => r.TestStepUrl.ShouldBe("https://example.org/software"));
        result.Content.ReleaseVersion.ShouldBe(Release);
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Load_SendsTheSelectionAsGivenToTheCatalogOfTheStatedRelease))]
    [Trait("Invariant", "EXP-07")]
    public async Task Load_SendsTheSelectionAsGivenToTheCatalogOfTheStatedRelease()
    {
        await LoadAsync(Request(EveryCriterion()));

        await _catalog.Received(1).ResolveAsync(
            Release,
            Arg.Is<CatalogSelectionQuery>(q =>
                q.UseCase == "hr" && q.AppTypeIds.SequenceEqual(new[] { "web" }) &&
                q.ProductPartIds.SequenceEqual(new[] { "base" }) && q.TestTypeIds.SequenceEqual(new[] { "softwareTest" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Load_ExcludedCriteria_AreLeftOutAndTheCatalogOrderIsKept()
    {
        var result = await LoadAsync(Request(Entry(14), Entry(13, included: false), Entry(12), Entry(11)));

        result.Content!.Rows.Select(r => r.Key).ShouldBe(["99.1", "99.2", "PROC-softwareTest"]);
    }

    [Fact(DisplayName = "INV:CAT-16 " + nameof(Load_TestedByCriterion_IsARowUntilTheUserDeselectsIt))]
    [Trait("Invariant", "CAT-16")]
    public async Task Load_TestedByCriterion_IsARowUntilTheUserDeselectsIt()
    {
        var included = await LoadAsync(Request(EveryCriterion()));
        var deselected = await LoadAsync(Request(Entry(11), Entry(12), Entry(13), Entry(14, included: false)));

        included.Content!.Rows.ShouldContain(r => r.Key == "PROC-softwareTest" && r.Kind == CriterionKind.TestProcedure);
        deselected.Content!.Rows.Select(r => r.Key).ShouldBe(["99.1", "99.2", "99.3"]);
    }

    [Fact]
    public async Task Load_OverrideToBFromA_HasNoWeightUntilTheUserSetsOne()
    {
        var result = await LoadAsync(Request(Entry(11, overrideLevel: "B"), Entry(12), Entry(13), Entry(14)));

        result.Content!.Rows[0].ShouldSatisfyAllConditions(r => r.ChosenLevel.ShouldBe(PriorityLevel.B), r => r.Weight.ShouldBeNull());
    }

    [Fact]
    public async Task Load_OverrideAwayFromB_DropsTheRecommendedWeight()
    {
        var result = await LoadAsync(Request(Entry(11), Entry(12, overrideLevel: "A"), Entry(13), Entry(14)));

        result.Content!.Rows[1].ShouldSatisfyAllConditions(r => r.ChosenLevel.ShouldBe(PriorityLevel.A), r => r.Weight.ShouldBeNull());
    }

    [Fact]
    public async Task Load_ChosenLevelEqualToTheRecommendation_CountsAsNoChoice()
    {
        var result = await LoadAsync(Request(Entry(11, overrideLevel: "A"), Entry(12, overrideLevel: "B", overrideWeight: 9), Entry(13), Entry(14)));

        result.Content!.Rows[0].ChosenLevel.ShouldBeNull();
        result.Content.Rows[1].ShouldSatisfyAllConditions(r => r.ChosenLevel.ShouldBeNull(), r => r.Weight.ShouldBe(9));
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Load_MissingRepeatedOrForeignRevision_IsAFieldError))]
    [Trait("Invariant", "EXP-07")]
    public async Task Load_MissingRepeatedOrForeignRevision_IsAFieldError()
    {
        // 13 and 14 are missing, 11 is repeated, 99 belongs to no criterion of the selection.
        var result = await LoadAsync(Request(Entry(11), Entry(12), Entry(11), Entry(99)));

        ShouldBeValidation(result, "criteria", "criteria[2].criterionRevisionId", "criteria[3].criterionRevisionId");
        result.Problem!.Errors!["criteria"][0].ShouldContain("'99.3' (revision 13)");
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Load_MoreEntriesThanTheReleaseHasCriteria_IsRefused))]
    [Trait("Invariant", "EXP-07")]
    public async Task Load_MoreEntriesThanTheReleaseHasCriteria_IsRefused()
    {
        Answer(Resolved(All, criterionCount: 4));

        var result = await LoadAsync(Request([.. EveryCriterion(), Entry(15)]));

        ShouldBeValidation(result, "criteria");
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Load_EntryShape_IsCheckedPerField))]
    [Trait("Invariant", "EXP-07")]
    public async Task Load_EntryShape_IsCheckedPerField()
    {
        var result = await LoadAsync(Request(
            new ExportCriterionRequest(null, true, null, null, null),
            new ExportCriterionRequest(12, null, null, null, null),
            Entry(13, overrideLevel: "a"),
            null,
            Entry(14)));

        ShouldBeValidation(
            result,
            "criteria[0].criterionRevisionId", "criteria[1].isIncluded", "criteria[2].overrideLevel", "criteria[3]", "criteria");
    }

    [Theory(DisplayName = "INV:EXP-07 " + nameof(Load_WeightOutsideTheRange_IsRefused))]
    [Trait("Invariant", "EXP-07")]
    [InlineData(0)]
    [InlineData(11)]
    public async Task Load_WeightOutsideTheRange_IsRefused(int weight)
    {
        var result = await LoadAsync(Request(Entry(11), Entry(12, overrideWeight: weight), Entry(13), Entry(14)));

        ShouldBeValidation(result, "criteria[1].overrideWeight");
        result.Problem!.Errors!["criteria[1].overrideWeight"][0].ShouldContain("between 1 and 10");
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Load_WeightWhileTheLevelIsNotB_IsRefused))]
    [Trait("Invariant", "EXP-07")]
    public async Task Load_WeightWhileTheLevelIsNotB_IsRefused()
    {
        var result = await LoadAsync(Request(Entry(11, overrideWeight: 5), Entry(12, overrideLevel: "Info", overrideWeight: 5), Entry(13), Entry(14)));

        ShouldBeValidation(result, "criteria[0].overrideWeight", "criteria[1].overrideWeight");
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Load_NoteAboveTheLimit_IsRefusedWithoutRepeatingIt))]
    [Trait("Invariant", "EXP-07")]
    public async Task Load_NoteAboveTheLimit_IsRefusedWithoutRepeatingIt()
    {
        var tooLong = Sentinel + new string('x', AdHocSelectionSource.NoteMaxLength + 1 - Sentinel.Length);
        var exact = new string('x', AdHocSelectionSource.NoteMaxLength);

        var result = await LoadAsync(Request(Entry(11, note: tooLong), Entry(12, note: exact), Entry(13), Entry(14)));

        ShouldBeValidation(result, "criteria[0].note");
        result.Problem!.Errors!.Values.SelectMany(m => m).ShouldAllBe(m => !m.Contains(Sentinel));
        result.Problem.Detail.ShouldNotContain(Sentinel);
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Load_MissingSelectionReleaseAndCriteria_AreFieldErrors))]
    [Trait("Invariant", "EXP-07")]
    public async Task Load_MissingSelectionReleaseAndCriteria_AreFieldErrors()
    {
        var noSelection = await LoadAsync(new ExportRequest("xlsx", null, null));
        var noRelease = await LoadAsync(new ExportRequest("xlsx", Selection() with { ReleaseVersion = "" }, EveryCriterion()));

        ShouldBeValidation(noSelection, "selection", "criteria");
        ShouldBeValidation(noRelease, "selection.releaseVersion");
        await _catalog.DidNotReceiveWithAnyArgs().ResolveAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Load_UnknownOrIncompleteRelease_Is404))]
    [Trait("Invariant", "EXP-07")]
    public async Task Load_UnknownOrIncompleteRelease_Is404()
    {
        Answer(new CatalogSelectionResult(CatalogSelectionOutcome.ReleaseNotFound, null, null, NoErrors()));

        var result = await LoadAsync(Request(EveryCriterion()));

        result.Problem!.ShouldSatisfyAllConditions(
            p => p.Status.ShouldBe(404),
            p => p.Type.ShouldBe(ProblemTypes.NotFound),
            p => p.Errors.ShouldBeNull());
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Load_ReleaseNoLongerActive_IsReleaseMismatch))]
    [Trait("Invariant", "EXP-07")]
    public async Task Load_ReleaseNoLongerActive_IsReleaseMismatch()
    {
        // Owner decision O-S4-60 (a): the export refuses a release a newer one has replaced.
        Answer(Resolved(All, active: false));

        var result = await LoadAsync(Request(EveryCriterion()));

        result.Problem!.ShouldSatisfyAllConditions(
            p => p.Status.ShouldBe(400),
            p => p.Type.ShouldBe(ProblemTypes.ReleaseMismatch),
            p => p.Errors!.Keys.ShouldBe(["selection.releaseVersion"]));
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Load_InvalidSelection_IsKeyedBelowSelection))]
    [Trait("Invariant", "EXP-07")]
    public async Task Load_InvalidSelection_IsKeyedBelowSelection()
    {
        Answer(Refused(CatalogSelectionOutcome.InvalidSelection, ("useCase", "Unknown use case 'x'."), ("appTypeIds[1]", "Unknown app type 'y'.")));

        var result = await LoadAsync(Request(EveryCriterion()));

        ShouldBeValidation(result, "selection.useCaseTemplateId", "selection.appTypeIds[1]");
    }

    [Fact(DisplayName = "INV:CAT-12 " + nameof(Load_ExcludedSelection_IsSelectionExcluded))]
    [Trait("Invariant", "CAT-12")]
    public async Task Load_ExcludedSelection_IsSelectionExcluded()
    {
        Answer(Refused(CatalogSelectionOutcome.ExcludedSelection, ("appTypeIds[1]", "'hardware' is excluded by use case 'hr'.")));

        var result = await LoadAsync(Request(EveryCriterion()));

        result.Problem!.ShouldSatisfyAllConditions(
            p => p.Status.ShouldBe(400),
            p => p.Type.ShouldBe(ProblemTypes.SelectionExcluded),
            p => p.Errors!.Keys.ShouldBe(["selection.appTypeIds[1]"]));
    }

    [Fact]
    public async Task Load_ExcludedSelectionWithOtherFieldErrors_AnswersTheFieldErrorsFirst()
    {
        // Two problem types never mix: the request errors come first, the exclusion after their fix.
        Answer(Refused(CatalogSelectionOutcome.ExcludedSelection, ("appTypeIds[1]", "excluded")));

        var result = await LoadAsync(Request(Entry(11, overrideLevel: "C"), Entry(12), Entry(13), Entry(14)));

        ShouldBeValidation(result, "criteria[0].overrideLevel");
    }

    [Fact]
    public void WeightRange_FromConfiguration_RefusesAnImpossibleRange()
    {
        Should.Throw<InvalidOperationException>(() => PriorityWeightRange.From(Configuration(("Min", "5"), ("Max", "4"))));
        Should.Throw<InvalidOperationException>(() => PriorityWeightRange.From(Configuration(("Min", "0"), ("Max", "4"))));
        Should.Throw<InvalidOperationException>(() => PriorityWeightRange.From(Configuration(("Max", "4"))));
        PriorityWeightRange.From(Configuration(("Min", "1"), ("Max", "10"))).ShouldBe(new PriorityWeightRange(1, 10));
    }

    private Task<ExportSourceResult> LoadAsync(ExportRequest request) =>
        new AdHocSelectionSource(_catalog, new PriorityWeightRange(1, 10)).LoadAsync(request, TestContext.Current.CancellationToken);

    private void Answer(CatalogSelectionResult result) =>
        _catalog.ResolveAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(result);

    private static void ShouldBeValidation(ExportSourceResult result, params string[] errorKeys)
    {
        result.Content.ShouldBeNull();
        result.Problem.ShouldNotBeNull();
        result.Problem.Status.ShouldBe(400);
        result.Problem.Type.ShouldBe(ProblemTypes.Validation);
        result.Problem.Errors!.Keys.ShouldBe(errorKeys, ignoreOrder: true);
    }

    private static ResolvedCatalogCriterion Criterion(int revisionId, string key, PriorityLevel level, int? weight) =>
        new(revisionId, key, CriterionKind.Requirement, key, $"Titel {key}", null, $"Kurzbeschreibung {key}",
            $"Originaltitel {key}", "Basisfunktionalität",
            new CatalogPriority(level, weight, $"Begründung {key}"), $"https://example.org/{key}");

    private static CatalogSelectionResult Resolved(ResolvedCatalogCriterion[] criteria, bool active = true, int criterionCount = 15) =>
        new(
            CatalogSelectionOutcome.Resolved,
            new CatalogReleaseSummary(Release, active, criterionCount),
            new ResolvedCatalogSelection(
                new CatalogValueName("hr", "HR-Anwendung"),
                [new CatalogValueName("web", "Webanwendung")],
                [new CatalogValueName("base", "Basisfunktionalität")],
                [new CatalogValueName("softwareTest", "Software-Test")],
                criteria),
            NoErrors());

    private static CatalogSelectionResult Refused(CatalogSelectionOutcome outcome, params (string Key, string Message)[] errors) =>
        new(outcome, new CatalogReleaseSummary(Release, true, 15), null, errors.ToDictionary(e => e.Key, e => new[] { e.Message }));

    private static Dictionary<string, string[]> NoErrors() => new(StringComparer.Ordinal);

    private static ExportSelectionRequest Selection() => new(Release, "hr", ["web"], ["base"], ["softwareTest"]);

    private static ExportRequest Request(params ExportCriterionRequest?[] entries) => new("xlsx", Selection(), entries);

    private static ExportCriterionRequest[] EveryCriterion() => [.. All.Select(c => Entry(c.RevisionId))];

    private static ExportCriterionRequest Entry(
        int revisionId, bool included = true, string? overrideLevel = null, int? overrideWeight = null, string? note = null) =>
        new(revisionId, included, overrideLevel, overrideWeight, note);

    private static IConfiguration Configuration(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>($"{PriorityWeightRange.SectionName}:{v.Key}", v.Value)))
            .Build();
}
