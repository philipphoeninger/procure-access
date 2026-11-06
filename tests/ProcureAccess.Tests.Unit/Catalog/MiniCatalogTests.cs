// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using Keys = ProcureAccess.Tests.Shared.Fixtures.MiniCatalog.Keys;

namespace ProcureAccess.Tests.Unit.Catalog;

/// <summary>
/// The MiniCatalog (TESTING §6.2, S3-14) holds every case it promises, so the tests of step 2 can
/// rely on it: if a later change to the fixture removes a case, this test names it.
/// </summary>
public class MiniCatalogTests
{
    private readonly MiniCatalog _catalog = MiniCatalog.Build(MiniCatalogValues.InMemory());

    [Fact]
    public void Build_TwoPublishedInactiveReleases_WithTwelveAndElevenCriteria()
    {
        _catalog.T1.Version.ShouldBe(MiniCatalog.T1Version);
        _catalog.T2.Version.ShouldBe(MiniCatalog.T2Version);
        _catalog.Releases.ShouldAllBe(r => r.IsPublished && !r.IsActive);
        _catalog.T1.PublishedAt.ShouldBe(MiniCatalog.T1PublishedAt);
        _catalog.T2.PublishedAt.ShouldBe(MiniCatalog.T2PublishedAt);
        _catalog.T1.Criteria.Count.ShouldBe(12);
        _catalog.T2.Criteria.Count.ShouldBe(11);
        _catalog.Releases.SelectMany(r => r.Criteria).ShouldAllBe(c => c.Revision.IsSealed);
    }

    [Fact]
    public void Releases_DifferByOneChangedAddedRemovedRenamedAndMergedCriterion()
    {
        var t1Keys = KeysOf(_catalog.T1);
        var t2Keys = KeysOf(_catalog.T2);

        t1Keys.Except(t2Keys).ShouldBe([Keys.Removed, Keys.RenamedOld, Keys.MergedA, Keys.MergedB], ignoreOrder: true);
        t2Keys.Except(t1Keys).ShouldBe([Keys.Added, Keys.RenamedNew, Keys.Merged], ignoreOrder: true);

        var changedV1 = MiniCatalog.RevisionOf(_catalog.T1, Keys.Changed);
        var changedV2 = MiniCatalog.RevisionOf(_catalog.T2, Keys.Changed);
        changedV2.Criterion.ShouldBeSameAs(changedV1.Criterion);
        (changedV1.RevisionNumber, changedV2.RevisionNumber).ShouldBe((1, 2));
        changedV2.TitleDe.ShouldNotBe(changedV1.TitleDe);
        changedV2.ProcedureTexts.Single().AllowedRatings.ShouldNotBe(changedV1.ProcedureTexts.Single().AllowedRatings);
    }

    [Fact]
    public void Releases_ShareTheUnchangedRevisions_SoT2HasFourNewOnes()
    {
        var t1Revisions = _catalog.T1.Criteria.Select(c => c.Revision).ToList();
        var t2Revisions = _catalog.T2.Criteria.Select(c => c.Revision).ToList();

        var shared = t2Revisions.Where(r => t1Revisions.Contains(r)).Select(r => r.Criterion.Key);
        shared.ShouldBe(
            [Keys.PriorityAllLevels, Keys.PriorityAppTypes, Keys.DefaultOnly, Keys.AudioOnly, Keys.VideoOnly,
             Keys.ProcedureSoftwareTest, Keys.ProcedureAppTest],
            ignoreOrder: true);
        t2Revisions.Count(r => !t1Revisions.Contains(r)).ShouldBe(4);
    }

    [Fact]
    public void SupersededByKey_RenameIsOneToOne_MergeIsNot()
    {
        var predecessors = _catalog.T1.Criteria.Select(c => c.Revision.Criterion)
            .Where(c => c.SupersededByKey is not null)
            .ToLookup(c => c.SupersededByKey!, c => c.Key);

        predecessors[Keys.RenamedNew].ShouldBe([Keys.RenamedOld]);
        predecessors[Keys.Merged].ShouldBe([Keys.MergedA, Keys.MergedB], ignoreOrder: true);
        predecessors.Count.ShouldBe(2);
        _catalog.T2.Criteria.ShouldAllBe(c => c.Revision.Criterion.SupersededByKey == null);
    }

    [Fact]
    public void PriorityAllLevels_HasOneRuleOnEachOfTheFourLevels_AndOneForTheInactiveUseCase()
    {
        var rules = MiniCatalog.RevisionOf(_catalog.T1, Keys.PriorityAllLevels).Priorities;

        rules.Select(p => (p.AppType?.Key, p.UseCase?.Key, p.Level)).ShouldBe(
        [
            ("web", "hr", PriorityLevel.A),
            ("softwareOpen", null, PriorityLevel.Info),
            (null, "hr", PriorityLevel.B),
            (null, "databaseApplication", PriorityLevel.A),
            (null, null, PriorityLevel.B),
        ]);
        rules.Single(p => p.UseCase?.Key == "databaseApplication").UseCase!.IsActive.ShouldBeFalse();
        rules.ShouldAllBe(p => !string.IsNullOrWhiteSpace(p.RationaleDe) && p.Source == PrioritySource.ExpertJudgement);
    }

    [Fact]
    public void PriorityAppTypes_DecidesByLevelByWeightAndByAppTypeSortOrder()
    {
        var rules = MiniCatalog.RevisionOf(_catalog.T1, Keys.PriorityAppTypes).Priorities;

        // With HR: the same level and weight for two app types, so only the sort order decides.
        var tie = rules.Where(p => p.UseCase?.Key == "hr").ToList();
        tie.Select(p => (p.Level, p.Weight)).Distinct().ShouldHaveSingleItem().ShouldBe((PriorityLevel.B, (int?)2));
        tie.Select(p => p.AppType!.SortOrder).Distinct().Count().ShouldBe(2);

        // Without a use case: B with different weights, and one app type with A.
        rules.Where(p => p.UseCase is null && p.AppType is not null)
            .Select(p => (p.AppType!.Key, p.Level, p.Weight))
            .ShouldBe(
                [("web", PriorityLevel.B, (int?)1), ("softwareOpen", PriorityLevel.B, 3), ("mobileOpen", PriorityLevel.A, null)],
                ignoreOrder: true);
    }

    [Fact]
    public void Values_HaveOneInactiveUseCaseAndOnlyBasisfunktionalitaetAlwaysSelected()
    {
        var values = _catalog.Values;

        (values.Hr.IsActive, values.DatabaseApplication.IsActive).ShouldBe((true, false));
        values.All.OfType<ProductPart>().Where(p => p.IsAlwaysSelected).ShouldHaveSingleItem().ShouldBeSameAs(values.Base);
        values.All.OfType<TestType>().Count().ShouldBe(2);
        MiniCatalog.RevisionOf(_catalog.T1, Keys.DefaultOnly).Applicability.ShouldAllBe(a => a.ProductPart == values.Base);
    }

    [Fact]
    public void AudioAndVideoCriteria_ApplyOnlyWithTheirProductPart()
    {
        MiniCatalog.RevisionOf(_catalog.T1, Keys.AudioOnly).Applicability.Select(a => (a.AppType.Key, a.ProductPart!.Key))
            .ShouldBe([("web", "audio"), ("softwareOpen", "audio")], ignoreOrder: true);
        MiniCatalog.RevisionOf(_catalog.T1, Keys.VideoOnly).Applicability.Select(a => (a.AppType.Key, a.ProductPart!.Key))
            .ShouldBe([("web", "video"), ("mobileOpen", "video")], ignoreOrder: true);
    }

    [Fact]
    public void Hardware_HasNoRequirementCriterionAndNoProcedure()
    {
        var hardware = _catalog.Values.Hardware;

        foreach (var release in _catalog.Releases)
        {
            release.Criteria.SelectMany(c => c.Revision.Applicability).ShouldNotContain(a => a.AppType == hardware);
            release.Procedures.ShouldNotContain(p => p.AppType == hardware);
        }
    }

    [Fact]
    public void Exclusions_CoverEveryPairOfLayers_AndT2SwapsOneRule()
    {
        foreach (var release in _catalog.Releases)
        {
            release.Exclusions.Select(e => (e.Exclusion.Source!.Layer, e.Exclusion.Target!.Layer)).Distinct().ShouldBe(
                [
                    (FilterLayer.UseCase, FilterLayer.AppType),
                    (FilterLayer.UseCase, FilterLayer.ProductPart),
                    (FilterLayer.UseCase, FilterLayer.TestType),
                    (FilterLayer.AppType, FilterLayer.ProductPart),
                    (FilterLayer.AppType, FilterLayer.TestType),
                    (FilterLayer.ProductPart, FilterLayer.TestType),
                ],
                ignoreOrder: true);
        }

        var t1Rules = _catalog.T1.Exclusions.Select(e => e.Exclusion).ToList();
        var t2Rules = _catalog.T2.Exclusions.Select(e => e.Exclusion).ToList();
        t1Rules.Except(t2Rules).Select(Pair).ShouldBe([("hr", "video")]);
        t2Rules.Except(t1Rules).Select(Pair).ShouldBe([("hr", "audio")]);

        static (string, string) Pair(FilterExclusion e) => (e.Source!.Key, e.Target!.Key);
    }

    [Fact]
    public void Procedures_OneAppTypeOnTwoInT1_AndT2DropsOneMapping()
    {
        Pairs(_catalog.T1).ShouldBe(
            [("web", "softwareTest"), ("softwareOpen", "softwareTest"), ("mobileOpen", "appTest"), ("mobileOpen", "softwareTest")],
            ignoreOrder: true);
        Pairs(_catalog.T2).ShouldBe(
            [("web", "softwareTest"), ("softwareOpen", "softwareTest"), ("mobileOpen", "appTest")],
            ignoreOrder: true);

        // The shared revision of PriorityAllLevels has a text per test type, so Mobile (offen) gets
        // the union of both texts in T.1 and only the App-Test text in T.2 (CAT-15, CAT-21).
        var texts = MiniCatalog.RevisionOf(_catalog.T2, Keys.PriorityAllLevels).ProcedureTexts
            .ToDictionary(t => t.TestType.Key, t => t.AllowedRatings);
        AllowedFor(_catalog.T1, "mobileOpen").ShouldBe(texts["appTest"] | texts["softwareTest"]);
        AllowedFor(_catalog.T2, "mobileOpen").ShouldBe(texts["appTest"]);
        AllowedFor(_catalog.T2, "mobileOpen").ShouldNotBe(AllowedFor(_catalog.T1, "mobileOpen"));

        static IEnumerable<(string, string)> Pairs(CatalogRelease release) =>
            release.Procedures.Select(p => (p.AppType.Key, p.TestType.Key));

        Rating AllowedFor(CatalogRelease release, string appTypeKey)
        {
            var mapped = release.Procedures.Where(p => p.AppType.Key == appTypeKey).Select(p => p.TestType.Key).ToHashSet();
            return texts.Where(t => mapped.Contains(t.Key)).Aggregate(Rating.None, (union, t) => union | t.Value);
        }
    }

    [Fact]
    public void ProcedureCriteria_OnePerTestType_WithDefaultBAndNoOtherRows()
    {
        foreach (var release in _catalog.Releases)
        {
            var procedures = release.Criteria.Select(c => c.Revision)
                .Where(r => r.Criterion.Kind == CriterionKind.TestProcedure)
                .ToList();

            procedures.Select(r => (r.Criterion.Key, r.Criterion.TestType!.Key))
                .ShouldBe([(Keys.ProcedureSoftwareTest, "softwareTest"), (Keys.ProcedureAppTest, "appTest")], ignoreOrder: true);
            procedures.ShouldAllBe(r => r.Applicability.Count == 0 && r.ProcedureTexts.Count == 0);
            procedures.Select(r => r.Priorities.Single()).ShouldAllBe(p =>
                p.AppType == null && p.UseCase == null && p.Level == PriorityLevel.B);
        }
    }

    [Fact]
    public void ProcedureTexts_AllowDifferentRatingsPerTestType()
    {
        var texts = MiniCatalog.RevisionOf(_catalog.T1, Keys.PriorityAllLevels).ProcedureTexts
            .ToDictionary(t => t.TestType.Key, t => t.AllowedRatings);

        texts.Keys.ShouldBe(["softwareTest", "appTest"], ignoreOrder: true);
        texts["softwareTest"].HasFlag(Rating.NotApplicable).ShouldBeFalse();
        texts["appTest"].HasFlag(Rating.NotApplicable).ShouldBeTrue();
        texts["softwareTest"].ShouldNotBe(texts["appTest"]);
    }

    private static List<string> KeysOf(CatalogRelease release) =>
        [.. release.Criteria.Select(c => c.Revision.Criterion.Key)];
}
