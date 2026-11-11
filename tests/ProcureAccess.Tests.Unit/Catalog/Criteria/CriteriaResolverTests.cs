// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Criteria;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Reading;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Shared.Builders.Catalog.CatalogData;
using Keys = ProcureAccess.Tests.Shared.Fixtures.MiniCatalog.Keys;

namespace ProcureAccess.Tests.Unit.Catalog.Criteria;

/// <summary>
/// The answer of the criteria endpoint for a checked selection (SPEC §8.2): which criteria, one
/// procedure criterion per selected test type (CAT-16), the procedure texts and allowed ratings of
/// the procedures mapped to the selected app types (CAT-16, CAT-21, C31), the selected app types
/// without criteria (CAT-20), <c>productPart</c> and <c>allowedRatings</c> of procedure criteria
/// (owner, S3-20 decisions 2, 3). Over the MiniCatalog; the HTTP part is in <c>CriteriaEndpointTests</c>.
/// </summary>
public class CriteriaResolverTests
{
    private static readonly MiniCatalogValues V = MiniCatalogValues.InMemory();
    private static readonly MiniCatalog Catalog = MiniCatalog.Build(V);
    private static readonly CatalogFilterValues Values = new(
        [V.Hr, V.DatabaseApplication],
        [V.Web, V.SoftwareOpen, V.MobileOpen, V.Hardware],
        [V.Base, V.Audio, V.Video],
        V.TestTypes);

    private static readonly string[] AllSix = ["fulfilled", "slightRestriction", "restriction", "barrier", "blockade", "notApplicable"];

    [Fact]
    public void Resolve_Web_ListsTheCriteriaThatApplyInListOrder()
    {
        var response = Resolve(Catalog.T1, Selection(null, ["web"], ["base"]));

        response.Release.ShouldBe("T.1");
        KeysOf(response).ShouldBe(
            [Keys.PriorityAllLevels, Keys.PriorityAppTypes, Keys.DefaultOnly, Keys.Changed, Keys.Removed, Keys.RenamedOld, Keys.MergedA, Keys.MergedB]);
        response.AppTypesWithoutCriteria.ShouldBeEmpty();
    }

    [Fact]
    public void Resolve_WithAudioAndVideo_AddsTheCriteriaLimitedToThoseParts()
    {
        var response = Resolve(Catalog.T1, Selection(null, ["web"], ["base", "audio", "video"]));

        KeysOf(response).ShouldContain(Keys.AudioOnly);
        KeysOf(response).ShouldContain(Keys.VideoOnly);
        KeysOf(response).IndexOf(Keys.AudioOnly).ShouldBe(KeysOf(response).IndexOf(Keys.DefaultOnly) + 1);
    }

    [Fact(DisplayName = "INV:CAT-16 " + nameof(Resolve_SelectedTestTypes_AddExactlyTheirProcedureCriteria))]
    [Trait("Invariant", "CAT-16")]
    public void Resolve_SelectedTestTypes_AddExactlyTheirProcedureCriteria()
    {
        var response = Resolve(Catalog.T1, Selection(null, ["web"], ["base"], ["pdfTest", "softwareTest"]));

        var procedures = response.Criteria.Where(c => c.Kind == "testProcedure").ToList();
        procedures.Select(c => c.Key).ShouldBe([Keys.ProcedureSoftwareTest, Keys.ProcedurePdfTest]);
        foreach (var procedure in procedures)
        {
            procedure.Number.ShouldBeNull();
            procedure.ProductPart.ShouldBeNull();
            procedure.ProcedureTexts.ShouldBeEmpty();
            procedure.ResolvedPriority.Level.ShouldBe("B");
            procedure.ResolvedPriority.MatchedRule.ShouldBe(new MatchedRuleItem("default", null, null));
        }
    }

    [Fact]
    public void Resolve_ProcedureCriterion_HasNoAllowedRatings()
    {
        // Never rated, only answered Ja/Nein/Nicht angegeben from test evidences (C30; owner, S3-20 decision 3).
        var response = Resolve(Catalog.T1, Selection(null, ["web"], ["base"], ["softwareTest"]));

        response.Criteria.Single(c => c.Key == Keys.ProcedureSoftwareTest).AllowedRatings.ShouldBeEmpty();
    }

    [Fact(DisplayName = "INV:CAT-16 " + nameof(Resolve_Criterion_ShowsTheTextsOfTheProceduresMappedToTheSelectedAppTypes))]
    [Trait("Invariant", "CAT-16")]
    public void Resolve_Criterion_ShowsTheTextsOfTheProceduresMappedToTheSelectedAppTypes()
    {
        // 99.1 has a Software-Test and an App-Test text. T.1 maps Web → Software-Test, Mobile offen → App-Test and Software-Test.
        var web = Criterion(Resolve(Catalog.T1, Selection(null, ["web"], ["base"])), Keys.PriorityAllLevels);
        var mobile = Criterion(Resolve(Catalog.T1, Selection(null, ["mobileOpen"], ["base"])), Keys.PriorityAllLevels);

        web.ProcedureTexts.Select(t => t.TestType).ShouldBe(["softwareTest"]);
        web.ProcedureTexts[0].SourceUrl.ShouldBe("https://example.org/softwareTest#99.1-softwareTest");
        web.AllowedRatings.ShouldBe(["fulfilled", "slightRestriction", "restriction", "barrier", "blockade"]);
        mobile.ProcedureTexts.Select(t => t.TestType).ShouldBe(["softwareTest", "appTest"]);
        mobile.ProcedureTexts[1].AllowedRatings.ShouldBe(["fulfilled", "barrier", "blockade", "notApplicable"]);
        mobile.AllowedRatings.ShouldBe(AllSix);
    }

    [Fact(DisplayName = "INV:CAT-21 " + nameof(Resolve_SameCriterionInTwoReleases_FollowsTheMappingOfEachRelease))]
    [Trait("Invariant", "CAT-21")]
    public void Resolve_SameCriterionInTwoReleases_FollowsTheMappingOfEachRelease()
    {
        // 99.1 is the same revision in both releases; T.2 no longer maps Mobile offen → Software-Test.
        var selection = Selection(null, ["mobileOpen"], ["base"]);

        var t1 = Criterion(Resolve(Catalog.T1, selection), Keys.PriorityAllLevels);
        var t2 = Criterion(Resolve(Catalog.T2, selection), Keys.PriorityAllLevels);

        t1.ProcedureTexts.Select(t => t.TestType).ShouldBe(["softwareTest", "appTest"]);
        t1.AllowedRatings.ShouldBe(AllSix);
        t2.ProcedureTexts.Select(t => t.TestType).ShouldBe(["appTest"]);
        t2.AllowedRatings.ShouldBe(["fulfilled", "barrier", "blockade", "notApplicable"]);
    }

    [Fact]
    public void Resolve_CriterionWithoutAnyApplyingText_AllowsAllSixLevels()
    {
        // 99.5 has no procedure text (C31: without any text all six levels are allowed).
        var response = Resolve(Catalog.T1, Selection(null, ["web"], ["base", "video"]));

        Criterion(response, Keys.VideoOnly).AllowedRatings.ShouldBe(AllSix);
        Criterion(response, Keys.VideoOnly).ProcedureTexts.ShouldBeEmpty();
    }

    [Fact(DisplayName = "INV:CAT-20 " + nameof(Resolve_AppTypeWithoutRequirementCriteria_IsListedNotAnError))]
    [Trait("Invariant", "CAT-20")]
    public void Resolve_AppTypeWithoutRequirementCriteria_IsListedNotAnError()
    {
        var hardware = Resolve(Catalog.T1, Selection(null, ["hardware"], ["base"]));
        var hardwareWithTest = Resolve(Catalog.T1, Selection(null, ["hardware"], ["base"], ["pdfTest"]));
        var withWeb = Resolve(Catalog.T1, Selection(null, ["hardware", "web"], ["base"]));

        hardware.Criteria.ShouldBeEmpty();
        hardware.AppTypesWithoutCriteria.ShouldBe([new AppTypeItem("hardware", "Hardware")]);
        KeysOf(hardwareWithTest).ShouldBe([Keys.ProcedurePdfTest]);
        hardwareWithTest.AppTypesWithoutCriteria.ShouldBe([new AppTypeItem("hardware", "Hardware")]);
        withWeb.Criteria.ShouldNotBeEmpty();
        withWeb.AppTypesWithoutCriteria.ShouldBe([new AppTypeItem("hardware", "Hardware")]);
    }

    [Fact(DisplayName = "INV:CAT-20 " + nameof(Resolve_AppTypeWhoseCriteriaNeedAnUnselectedPart_IsListed))]
    [Trait("Invariant", "CAT-20")]
    public void Resolve_AppTypeWhoseCriteriaNeedAnUnselectedPart_IsListed()
    {
        // In this release Web has only a Video criterion; without Video, Web has none.
        var release = ReleaseOf(ARevision().WithKey("T-VIDEO").AppliesTo(V.Web, V.Video).WithDefaultPriority(PriorityLevel.B).Build());

        Resolve(release, Selection(null, ["web"], ["base"])).AppTypesWithoutCriteria.Select(a => a.Id).ShouldBe(["web"]);
        Resolve(release, Selection(null, ["web"], ["base", "video"])).AppTypesWithoutCriteria.ShouldBeEmpty();
    }

    [Fact]
    public void Resolve_ProductPart_IsTheSelectedPartWithTheLowestSortOrderOrNull()
    {
        // Owner, S3-20 decision 2: one part per criterion (SPEC §8.2).
        var release = ReleaseOf(ARevision().WithKey("T-PARTS")
            .AppliesTo(V.Web, V.Video).AppliesTo(V.Web, V.Audio)
            .WithDefaultPriority(PriorityLevel.B)
            .Build());

        Criterion(Resolve(release, Selection(null, ["web"], ["base", "video", "audio"])), "T-PARTS").ProductPart.ShouldBe("audio");
        Criterion(Resolve(release, Selection(null, ["web"], ["base", "video"])), "T-PARTS").ProductPart.ShouldBe("video");
        var mini = Resolve(Catalog.T1, Selection(null, ["web"], ["base", "audio"]));
        Criterion(mini, Keys.DefaultOnly).ProductPart.ShouldBe("base");
        Criterion(mini, Keys.AudioOnly).ProductPart.ShouldBe("audio");
        Criterion(mini, Keys.PriorityAllLevels).ProductPart.ShouldBeNull();
    }

    [Fact]
    public void Resolve_AppliesTo_IsTheSelectedAppTypesTheCriterionAppliesToInSortOrder()
    {
        // Owner decision O-S4-29 (a): „Gilt für“ in the detail panel. Mobile applies only with Audio.
        var release = ReleaseOf(ARevision().WithKey("T-APPLIES")
            .AppliesTo(V.MobileOpen, V.Audio).AppliesTo(V.SoftwareOpen).AppliesTo(V.Web, V.Video)
            .WithDefaultPriority(PriorityLevel.B)
            .Build());

        var withVideo = Resolve(release, Selection(null, ["mobileOpen", "softwareOpen", "web"], ["base", "video"]));
        var withAudio = Resolve(release, Selection(null, ["mobileOpen", "web"], ["base", "audio"]));

        Criterion(withVideo, "T-APPLIES").AppliesTo.ShouldBe(
            [new AppTypeItem("web", "Website oder Webanwendung"), new AppTypeItem("softwareOpen", "Software (offen)")]);
        Criterion(withAudio, "T-APPLIES").AppliesTo.ShouldBe([new AppTypeItem("mobileOpen", "Mobile Anwendung (offen)")]);
    }

    [Fact]
    public void Resolve_ProcedureCriterion_AppliesToNothing()
    {
        var response = Resolve(Catalog.T1, Selection(null, ["web"], ["base"], ["softwareTest"]));

        Criterion(response, Keys.ProcedureSoftwareTest).AppliesTo.ShouldBeEmpty();
        Criterion(response, Keys.DefaultOnly).AppliesTo.ShouldBe([new AppTypeItem("web", "Website oder Webanwendung")]);
    }

    [Fact(DisplayName = "INV:CAT-02 " + nameof(Resolve_Criterion_CarriesItsContentAndExplainedPriority))]
    [Trait("Invariant", "CAT-02")]
    public void Resolve_Criterion_CarriesItsContentAndExplainedPriority()
    {
        var response = Resolve(Catalog.T1, Selection("hr", ["web"], ["base"]));

        var allLevels = Criterion(response, Keys.PriorityAllLevels);
        allLevels.Kind.ShouldBe("requirement");
        allLevels.Number.ShouldBe(Keys.PriorityAllLevels);
        allLevels.Title.ShouldBe("Testkriterium mit Regeln auf allen vier Ebenen");
        allLevels.ResolvedPriority.ShouldBe(new ResolvedPriorityItem(
            "A", null, "Testbegründung A für web und hr.", "expertJudgement", new MatchedRuleItem("appTypeAndUseCase", "web", "hr")));
        Criterion(response, Keys.PriorityAppTypes).ResolvedPriority.Weight.ShouldBe(2);
        Criterion(response, Keys.DefaultOnly).StandardReferences
            .ShouldBe([new StandardReferenceItem("en301549", "T.9.1.1", "https://example.org/en301549#T.9.1.1")]);
    }

    [Fact]
    public void Resolve_IncompleteRelease_IsRefused()
    {
        // No catalog default: catalog activate would refuse it, the endpoint answers 404 (owner, S3-20 decision 7).
        var release = ReleaseOf(ARevision().WithKey("T-NODEFAULT").AppliesTo(V.Web).WithPriority(V.Web, null, PriorityLevel.A).Build());

        release.IsComplete.ShouldBeFalse();
        release.Gaps.ShouldHaveSingleItem().ShouldContain("'T-NODEFAULT'");
        Should.Throw<ArgumentException>(() => Resolve(release, Selection(null, ["web"], ["base"])));
    }

    [Fact]
    public void Snapshot_OfTheMiniCatalog_IsComplete()
    {
        Snapshot(Catalog.T1).IsComplete.ShouldBeTrue();
        Snapshot(Catalog.T2).IsComplete.ShouldBeTrue();
    }

    private static CriteriaResponse Resolve(CatalogRelease release, CriteriaSelection selection) =>
        Resolve(Snapshot(release), selection);

    private static CriteriaResponse Resolve(CatalogReleaseSnapshot release, CriteriaSelection selection) =>
        CriteriaResolver.Resolve(Values, release, selection);

    private static CatalogReleaseSnapshot Snapshot(CatalogRelease release) =>
        CatalogReleaseSnapshot.From(release, V.TestTypes.Select(t => t.Key));

    /// <summary>A published release of <paramref name="revision"/> alone; completeness is checked without test types.</summary>
    private static CatalogReleaseSnapshot ReleaseOf(CriterionRevision revision) =>
        CatalogReleaseSnapshot.From(ACatalogRelease().WithVersion("T.X").Including(revision).Build(), []);

    private static CriteriaSelection Selection(string? useCase, string[] appTypes, string[] productParts, string[]? testTypes = null) =>
        new(useCase, appTypes, productParts, testTypes ?? []);

    private static List<string> KeysOf(CriteriaResponse response) => [.. response.Criteria.Select(c => c.Key)];

    private static CriterionItem Criterion(CriteriaResponse response, string key) => response.Criteria.Single(c => c.Key == key);
}
