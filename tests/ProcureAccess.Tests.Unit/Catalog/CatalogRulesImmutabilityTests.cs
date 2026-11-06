// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Catalog;

/// <summary>
/// CAT-05 for the rows S3-13 adds: a published release pins its exclusion rules and procedure
/// mapping, and the revisions it contains keep their applicability, priority rules and procedure
/// texts; every change fails with <see cref="PublishedCatalogChangeException"/>. The core rules are
/// in <see cref="CatalogReleaseImmutabilityTests"/>.
/// </summary>
public class CatalogRulesImmutabilityTests
{
    private static readonly DateTimeOffset PublishedAt = new(2026, 11, 6, 10, 0, 0, TimeSpan.Zero);

    private static readonly UseCaseTemplate Hr = UseCaseTemplate.Create("hr", "HR-Anwendung");
    private static readonly AppType Web = AppType.Create("web", "Website oder Webanwendung", AppTypeFunctionality.NotApplicable, 1);
    private static readonly AppType Document = AppType.Create("document", "Dokument", AppTypeFunctionality.NotApplicable, 6);
    private static readonly ProductPart Base = ProductPart.Create("base", "Basisfunktionalität", "Erklärung", 1, isAlwaysSelected: true);
    private static readonly TestType SoftwareTest = TestType.Create("softwareTest", "BIT inklusiv Software-Test", "https://softwaretest.bit-inklusiv.de/", 1);

    private static readonly ProcedureTextContent Text = new(
        "11.2.1.1", "Originaltitel", Rating.Fulfilled | Rating.Blockade, "https://softwaretest.bit-inklusiv.de/#11.2.1.1");

    [Fact(DisplayName = "INV:CAT-05 " + nameof(AddExclusion_PublishedRelease_ThrowsPublishedCatalogChange))]
    [Trait("Invariant", "CAT-05")]
    public void AddExclusion_PublishedRelease_ThrowsPublishedCatalogChange()
    {
        var release = PublishedRelease(Revision("K-1"));

        Should.Throw<PublishedCatalogChangeException>(
            () => release.AddExclusion(FilterExclusion.Create(Hr, Document, "Grund")));

        release.Exclusions.ShouldBeEmpty();
    }

    [Fact(DisplayName = "INV:CAT-05 " + nameof(MapProcedure_PublishedRelease_ThrowsPublishedCatalogChange))]
    [Trait("Invariant", "CAT-05")]
    public void MapProcedure_PublishedRelease_ThrowsPublishedCatalogChange()
    {
        var release = PublishedRelease(Revision("K-1"));

        Should.Throw<PublishedCatalogChangeException>(() => release.MapProcedure(Web, SoftwareTest));

        release.Procedures.ShouldBeEmpty();
    }

    [Fact(DisplayName = "INV:CAT-05 " + nameof(AddRows_RevisionOfPublishedRelease_ThrowPublishedCatalogChange))]
    [Trait("Invariant", "CAT-05")]
    public void AddRows_RevisionOfPublishedRelease_ThrowPublishedCatalogChange()
    {
        var revision = Revision("K-1");
        PublishedRelease(revision);

        Should.Throw<PublishedCatalogChangeException>(() => revision.AddApplicability(Web, Base));
        Should.Throw<PublishedCatalogChangeException>(() => revision.AddPriority(null, null, PriorityLevel.A, "Begründung"));
        Should.Throw<PublishedCatalogChangeException>(() => revision.AddProcedureText(SoftwareTest, Text));

        revision.Applicability.ShouldBeEmpty();
        revision.Priorities.ShouldBeEmpty();
        revision.ProcedureTexts.ShouldBeEmpty();
    }

    [Fact]
    public void DraftReleaseAndRevision_AddRules_KeepThemUntilPublished()
    {
        var revision = Revision("K-1");
        revision.AddApplicability(Web);
        revision.AddApplicability(Web, Base);
        revision.AddPriority(null, null, PriorityLevel.B, "Standard");
        revision.AddPriority(Web, Hr, PriorityLevel.A, "Für HR im Web", weight: 3);
        revision.AddProcedureText(SoftwareTest, Text);
        var exclusion = FilterExclusion.Create(Hr, Document, "Grund");
        var release = CatalogRelease.Create("T.1");
        release.Include(revision);
        release.AddExclusion(exclusion);
        release.MapProcedure(Web, SoftwareTest);

        release.Publish(PublishedAt);

        revision.Applicability.Select(a => a.ProductPart?.Key).ShouldBe([null, "base"]);
        var rule = revision.Priorities.Single(p => p.AppType is not null);
        rule.UseCase.ShouldBeSameAs(Hr);
        rule.Level.ShouldBe(PriorityLevel.A);
        rule.Weight.ShouldBe(3);
        rule.Source.ShouldBe(PrioritySource.ExpertJudgement);
        revision.ProcedureTexts.ShouldHaveSingleItem().AllowedRatings.ShouldBe(Rating.Fulfilled | Rating.Blockade);
        release.Exclusions.ShouldHaveSingleItem().Exclusion.ShouldBeSameAs(exclusion);
        release.Procedures.ShouldHaveSingleItem().TestType.ShouldBeSameAs(SoftwareTest);
    }

    [Fact]
    public void AddExclusion_SameRuleTwice_Throws()
    {
        var release = CatalogRelease.Create("T.1");
        var exclusion = FilterExclusion.Create(Hr, Document, "Grund");
        release.AddExclusion(exclusion);

        Should.Throw<ArgumentException>(() => release.AddExclusion(exclusion));
    }

    [Fact]
    public void MapProcedure_SamePairTwice_ThrowsButSecondProcedureIsAllowed()
    {
        // One app type may map to several procedures (CAT-21), but each pair only once.
        var release = CatalogRelease.Create("T.1");
        release.MapProcedure(Web, SoftwareTest);
        release.MapProcedure(Web, TestType.Create("bikWeb", "BIK BITV-Test (Web)", "https://bitvtest.de/", 4));

        Should.Throw<ArgumentException>(() => release.MapProcedure(Web, SoftwareTest));

        release.Procedures.Count.ShouldBe(2);
    }

    [Fact]
    public void AddRows_SameKeyTwice_Throw()
    {
        var revision = Revision("K-1");
        revision.AddApplicability(Web);
        revision.AddPriority(Web, null, PriorityLevel.A, "Begründung");
        revision.AddProcedureText(SoftwareTest, Text);

        Should.Throw<ArgumentException>(() => revision.AddApplicability(Web));
        Should.Throw<ArgumentException>(() => revision.AddPriority(Web, null, PriorityLevel.B, "Andere Begründung"));
        Should.Throw<ArgumentException>(() => revision.AddProcedureText(SoftwareTest, Text));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(64)]
    [InlineData(1 | 64)]
    public void AddProcedureText_AllowedRatingsEmptyOrUnknownBit_Throws(int allowedRatings)
    {
        var revision = Revision("K-1");

        Should.Throw<ArgumentOutOfRangeException>(
            () => revision.AddProcedureText(SoftwareTest, Text with { AllowedRatings = (Rating)allowedRatings }));
    }

    [Fact]
    public void AddPriority_WithoutRationale_Throws()
    {
        // A priority that cannot explain itself is a bug (ARCHITECTURE §5.1, CAT-02).
        Should.Throw<ArgumentException>(() => Revision("K-1").AddPriority(null, null, PriorityLevel.A, " "));
    }

    [Fact]
    public void ProcedureCriterion_ApplicabilityOrProcedureText_ThrowsButPriorityIsAllowed()
    {
        // A procedure criterion enters a selection only through its test type, has no BIT inklusiv
        // text, and has a catalog-default priority B (ARCHITECTURE §5.1, C30).
        var revision = CriterionRevision.Create(
            Criterion.Create("PROC-softwareTest", CriterionKind.TestProcedure, testType: SoftwareTest), 1, "Geprüft nach");

        Should.Throw<InvalidOperationException>(() => revision.AddApplicability(Web));
        Should.Throw<InvalidOperationException>(() => revision.AddProcedureText(SoftwareTest, Text));
        revision.AddPriority(null, null, PriorityLevel.B, "Standard");

        revision.Priorities.ShouldHaveSingleItem().Level.ShouldBe(PriorityLevel.B);
    }

    [Fact]
    public void CreateCriterion_TestTypeOnlyForProcedureCriteria()
    {
        Should.Throw<ArgumentException>(() => Criterion.Create("PROC-x", CriterionKind.TestProcedure));
        Should.Throw<ArgumentException>(() => Criterion.Create("K-1", CriterionKind.Requirement, testType: SoftwareTest));

        Criterion.Create("PROC-softwareTest", CriterionKind.TestProcedure, testType: SoftwareTest).TestType.ShouldBeSameAs(SoftwareTest);
    }

    private static CriterionRevision Revision(string key) =>
        CriterionRevision.Create(Criterion.Create(key, CriterionKind.Requirement), 1, $"Titel {key}");

    private static CatalogRelease PublishedRelease(params CriterionRevision[] revisions)
    {
        var release = CatalogRelease.Create("T.1");
        foreach (var revision in revisions)
        {
            release.Include(revision);
        }

        release.Publish(PublishedAt);
        return release;
    }
}
