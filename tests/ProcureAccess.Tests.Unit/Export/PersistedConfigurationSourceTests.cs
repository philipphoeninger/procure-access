// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using NSubstitute;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Configurator.Drafts;
using ProcureAccess.Modules.Export.Documents;
using ProcureAccess.Modules.Export.Requests;
using ProcureAccess.Modules.Export.Sources;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Export;

/// <summary>
/// The export source of saved lists (S5-19; ARCHITECTURE §4.4, C62) on doubles of the catalog and
/// of <see cref="IConfigurationReader"/>: EXP-06 — the same selection and decisions give the same
/// content as the ad-hoc source — and what the source answers for a list it cannot read. The real
/// catalog and database run in the integration tests.
/// </summary>
public class PersistedConfigurationSourceTests
{
    private const string Release = "T.1";

    private static readonly Guid ListId = Guid.NewGuid();

    // As the catalog resolves the selection, in list order: A, B with weight 4, Info, B with weight 2
    // and a „Geprüft nach …“ criterion.
    private static readonly ResolvedCatalogCriterion[] Criteria =
    [
        Criterion(11, "99.1", PriorityLevel.A, null),
        Criterion(12, "99.2", PriorityLevel.B, 4),
        Criterion(13, "99.3", PriorityLevel.Info, null),
        Criterion(14, "99.4", PriorityLevel.B, 2),
        new(15, "PROC-softwareTest", CriterionKind.TestProcedure, null, "Software-Test", null, null, null, null,
            new CatalogPriority(PriorityLevel.Info, null, "Begründung PROC"), "https://example.org/software"),
    ];

    // One decision of every kind: a level and weight chosen, the recommendation kept, a note, a level
    // equal to the recommendation, a level away from B, an exclusion — in another order than the list.
    private static readonly SavedCriterion[] Decisions =
    [
        new(15, false, PriorityLevel.Info, null, null, null),
        new(14, true, PriorityLevel.B, PriorityLevel.A, null, null),
        new(13, true, PriorityLevel.Info, null, null, "Eine Notiz"),
        new(12, true, PriorityLevel.B, null, 9, null),
        new(11, true, PriorityLevel.A, PriorityLevel.B, 7, "Bei uns gewichtet"),
    ];

    private readonly ICatalogSelectionResolver _catalog = Substitute.For<ICatalogSelectionResolver>();
    private readonly IConfigurationReader _reader = Substitute.For<IConfigurationReader>();

    public PersistedConfigurationSourceTests()
    {
        Answer(Resolved(Criteria, active: true));
        _reader.FindAsync(ListId, TestContext.Current.CancellationToken).Returns(Saved(Decisions));
    }

    [Fact(DisplayName = "INV:EXP-06 " + nameof(Load_SameSelectionAndDecisionsAsTheAdHocSource_GivesIdenticalContent))]
    [Trait("Invariant", "EXP-06")]
    public async Task Load_SameSelectionAndDecisionsAsTheAdHocSource_GivesIdenticalContent()
    {
        var request = new ExportRequest(
            "csv",
            new ExportSelectionRequest(Release, "hr", ["web"], ["base"], ["softwareTest"]),
            [.. Decisions.Select(d => new ExportCriterionRequest(
                d.CriterionRevisionId, d.IsIncluded, LevelName(d.OverrideLevel), d.OverrideWeight, d.Note))]);

        var adHoc = await new AdHocSelectionSource(new ConfigurationDraftCheck(_catalog, new PriorityWeightRange(1, 10)))
            .LoadAsync(request, TestContext.Current.CancellationToken);
        var persisted = await LoadAsync();

        adHoc.Problem.ShouldBeNull();
        persisted.Problem.ShouldBeNull();
        ShouldBeTheSame(persisted.Content!, adHoc.Content!);
        persisted.Content!.Rows.Select(r => (r.Key, r.ChosenLevel, r.Weight, r.Note)).ShouldBe(
        [
            ("99.1", PriorityLevel.B, 7, "Bei uns gewichtet"),
            ("99.2", null, 9, null),
            ("99.3", null, null, "Eine Notiz"),
            ("99.4", PriorityLevel.A, null, null),
        ]);
    }

    [Fact]
    public async Task Load_SavedList_ResolvesItsOwnReleaseAndSelectionAlsoWhenTheReleaseIsNoLongerActive()
    {
        Answer(Resolved(Criteria, active: false));

        var result = await LoadAsync();

        result.Problem.ShouldBeNull();
        result.Content!.ReleaseVersion.ShouldBe(Release);
        await _catalog.Received(1).ResolveAsync(
            Release,
            Arg.Is<CatalogSelectionQuery>(q =>
                q.UseCase == "hr" && q.AppTypeIds.SequenceEqual(new[] { "web" }) &&
                q.ProductPartIds.SequenceEqual(new[] { "base" }) && q.TestTypeIds.SequenceEqual(new[] { "softwareTest" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Load_ListThatDoesNotExistForTheRequest_Is404AndAsksTheCatalogNothing()
    {
        var result = await new PersistedConfigurationSource(_reader, _catalog).LoadAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        result.Content.ShouldBeNull();
        result.Problem.ShouldNotBeNull().ShouldSatisfyAllConditions(
            p => p.Status.ShouldBe(404), p => p.Type.ShouldBe(ProblemTypes.NotFound), p => p.Errors.ShouldBeNull());
        await _catalog.DidNotReceiveWithAnyArgs().ResolveAsync(default!, default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Load_ListThatDoesNotMatchItsSelection_ThrowsInsteadOfExportingAPart()
    {
        _reader.FindAsync(ListId, TestContext.Current.CancellationToken).Returns(Saved([.. Decisions.Skip(1)]));
        (await Should.ThrowAsync<InvalidOperationException>(LoadAsync)).Message.ShouldContain("PROC-softwareTest");

        _reader.FindAsync(ListId, TestContext.Current.CancellationToken)
               .Returns(Saved([.. Decisions, new SavedCriterion(99, true, PriorityLevel.A, null, null, null)]));
        await Should.ThrowAsync<InvalidOperationException>(LoadAsync);

        Answer(new CatalogSelectionResult(
            CatalogSelectionOutcome.ReleaseNotFound, null, null, new Dictionary<string, string[]>(StringComparer.Ordinal)));
        _reader.FindAsync(ListId, TestContext.Current.CancellationToken).Returns(Saved(Decisions));
        (await Should.ThrowAsync<InvalidOperationException>(LoadAsync)).Message.ShouldContain("no longer resolves");
    }

    private Task<ExportSourceResult> LoadAsync() =>
        new PersistedConfigurationSource(_reader, _catalog).LoadAsync(ListId, TestContext.Current.CancellationToken);

    private void Answer(CatalogSelectionResult result) =>
        _catalog.ResolveAsync(default!, default!, TestContext.Current.CancellationToken).ReturnsForAnyArgs(result);

    private static void ShouldBeTheSame(ExportContent actual, ExportContent expected)
    {
        actual.ReleaseVersion.ShouldBe(expected.ReleaseVersion);
        actual.UseCase.ShouldBe(expected.UseCase);
        actual.AppTypes.ShouldBe(expected.AppTypes);
        actual.ProductParts.ShouldBe(expected.ProductParts);
        actual.TestTypes.ShouldBe(expected.TestTypes);
        actual.Rows.ShouldBe(expected.Rows);
    }

    private static SavedConfiguration Saved(SavedCriterion[] criteria) =>
        new(ListId, "Vergabe 2027", Release, "hr", ["web"], ["base"], ["softwareTest"], criteria);

    private static string? LevelName(PriorityLevel? level) => level switch
    {
        null => null,
        PriorityLevel.Info => "Info",
        _ => level.ToString(),
    };

    private static ResolvedCatalogCriterion Criterion(int revisionId, string key, PriorityLevel level, int? weight) =>
        new(revisionId, key, CriterionKind.Requirement, key, $"Titel {key}", null, $"Kurzbeschreibung {key}",
            $"Originaltitel {key}", "Basisfunktionalität",
            new CatalogPriority(level, weight, $"Begründung {key}"), $"https://example.org/{key}");

    private static CatalogSelectionResult Resolved(ResolvedCatalogCriterion[] criteria, bool active) =>
        new(
            CatalogSelectionOutcome.Resolved,
            new CatalogReleaseSummary(Release, active, 15),
            new ResolvedCatalogSelection(
                new CatalogValueName("hr", "HR-Anwendung"),
                [new CatalogValueName("web", "Webanwendung")],
                [new CatalogValueName("base", "Basisfunktionalität")],
                [new CatalogValueName("softwareTest", "Software-Test")],
                criteria),
            new Dictionary<string, string[]>(StringComparer.Ordinal));
}
