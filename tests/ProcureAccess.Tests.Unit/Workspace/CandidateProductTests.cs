// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Reflection;
using ProcureAccess.Modules.Workspace.Domain;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Workspace;

/// <summary>
/// The rules of a candidate product and its test evidences (SPEC R-CAN-02, R-CAN-05, §7.2;
/// ARCHITECTURE §5.4, C46, C65; S6-15): the status „Nicht bekannt“ of a new candidate (SIG-03
/// workspace part), evidences only under „Ja“ and at most one per catalog procedure, leaving „Ja“
/// only without evidences or with their removal, a new product version updating its row (WSP-01),
/// and the private fields kept off the two types a publication copies.
/// </summary>
public class CandidateProductTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 18, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = CreatedAt.AddHours(1);
    private static readonly DateOnly TestDate = new(2026, 10, 1);

    [Fact(DisplayName = "INV:SIG-03 " + nameof(Create_Project_StatusUnknownWithoutEvidences))]
    [Trait("Invariant", "SIG-03")]
    public void Create_Project_StatusUnknownWithoutEvidences()
    {
        var project = Project();

        var candidate = CandidateProduct.Create(project, new CandidateFacts("Produkt", "Anbieter", "https://example.test", "Stichworte"), CreatedAt);

        candidate.ShouldSatisfyAllConditions(
            c => c.Id.ShouldNotBe(Guid.Empty),
            c => c.OrganizationId.ShouldBe(project.OrganizationId),
            c => c.ProcurementProjectId.ShouldBe(project.Id),
            c => c.SoftwareProductId.ShouldBeNull(),
            c => c.Name.ShouldBe("Produkt"),
            c => c.VendorName.ShouldBe("Anbieter"),
            c => c.TestStatus.ShouldBe(TestStatus.Unknown),
            c => c.TestEvidences.ShouldBeEmpty(),
            c => c.CreatedAt.ShouldBe(CreatedAt),
            c => c.ModifiedAt.ShouldBe(CreatedAt));
    }

    [Fact]
    public void Create_OnlyAName_KeepsTheOptionalFactsEmpty()
    {
        var candidate = CandidateProduct.Create(Project(), new CandidateFacts("Produkt", null, null, null), CreatedAt);

        candidate.Facts.ShouldBe(new CandidateFacts("Produkt", null, null, null));
    }

    [Fact]
    public void Create_ArchivedProjectOrInvalidFacts_Throws()
    {
        var archived = Project();
        archived.ChangeStatus(ProjectStatus.Archived, CreatedAt);

        Should.Throw<InvalidOperationException>(() => CandidateProduct.Create(archived, new CandidateFacts("Produkt", null, null, null), CreatedAt));
        Should.Throw<ArgumentException>(() => CandidateProduct.Create(Project(), new CandidateFacts(" ", null, null, null), CreatedAt));
        Should.Throw<ArgumentException>(() => CandidateProduct.Create(
            Project(), new CandidateFacts(new string('a', CandidateProduct.NameMaxLength + 1), null, null, null), CreatedAt));
        Should.Throw<ArgumentException>(() => CandidateProduct.Create(
            Project(), new CandidateFacts("Produkt", null, null, new string('a', CandidateProduct.FunctionalSummaryMaxLength + 1)), CreatedAt));
    }

    [Fact]
    public void ChangeFacts_SameFacts_ChangesNothing()
    {
        var candidate = Candidate();

        candidate.ChangeFacts(candidate.Facts, Later).ShouldBeFalse();
        candidate.ModifiedAt.ShouldBe(CreatedAt);
        candidate.ChangeFacts(candidate.Facts with { VendorName = "Neu" }, Later).ShouldBeTrue();
        candidate.ModifiedAt.ShouldBe(Later);
    }

    [Theory(DisplayName = "INV:WSP-01 " + nameof(AddEvidence_StatusNotTested_IsRefused))]
    [Trait("Invariant", "WSP-01")]
    [InlineData(nameof(TestStatus.Unknown))]
    [InlineData(nameof(TestStatus.NotTested))]
    public void AddEvidence_StatusNotTested_IsRefused(string status)
    {
        var candidate = Candidate();
        candidate.ChangeTestStatus(Enum.Parse<TestStatus>(status), removeEvidences: false, CreatedAt);

        var change = candidate.AddEvidence(Procedure("softwareTest"), Later);

        change.Outcome.ShouldBe(EvidenceChangeOutcome.NotTested);
        candidate.TestEvidences.ShouldBeEmpty();
    }

    [Fact(DisplayName = "INV:WSP-01 " + nameof(AddEvidence_SeveralKinds_HoldsOneRowEach))]
    [Trait("Invariant", "WSP-01")]
    public void AddEvidence_SeveralKinds_HoldsOneRowEach()
    {
        var candidate = TestedCandidate();

        candidate.AddEvidence(Procedure("softwareTest"), Later).Outcome.ShouldBe(EvidenceChangeOutcome.Done);
        candidate.AddEvidence(Procedure("bikWeb"), Later).Outcome.ShouldBe(EvidenceChangeOutcome.Done);
        candidate.AddEvidence(Other("Hausinterner Test"), Later).Outcome.ShouldBe(EvidenceChangeOutcome.Done);
        candidate.AddEvidence(Other("Hausinterner Test"), Later).Outcome.ShouldBe(EvidenceChangeOutcome.Done);
        candidate.AddEvidence(Own(), Later).Outcome.ShouldBe(EvidenceChangeOutcome.Done);

        candidate.TestEvidences.Select(e => e.TestKind).ShouldBe(
            [TestKind.Procedure, TestKind.Procedure, TestKind.Other, TestKind.Other, TestKind.Own]);
        candidate.TestEvidences.ShouldAllBe(e => e.CandidateProductId == candidate.Id && e.OrganizationId == candidate.OrganizationId);
        candidate.ModifiedAt.ShouldBe(Later);
    }

    [Fact(DisplayName = "INV:WSP-01 " + nameof(AddEvidence_SecondOfTheSameProcedure_IsRefused))]
    [Trait("Invariant", "WSP-01")]
    public void AddEvidence_SecondOfTheSameProcedure_IsRefused()
    {
        var candidate = TestedCandidate();
        candidate.AddEvidence(Procedure("softwareTest"), CreatedAt);

        var change = candidate.AddEvidence(Procedure("softwareTest") with { TestedByName = "Andere Prüfstelle" }, Later);

        change.Outcome.ShouldBe(EvidenceChangeOutcome.DuplicateTestType);
        candidate.TestEvidences.Count.ShouldBe(1);
    }

    [Fact(DisplayName = "INV:WSP-01 " + nameof(ChangeEvidence_NewProductVersion_UpdatesTheSameRow))]
    [Trait("Invariant", "WSP-01")]
    public void ChangeEvidence_NewProductVersion_UpdatesTheSameRow()
    {
        var candidate = TestedCandidate();
        var evidence = candidate.AddEvidence(Procedure("softwareTest") with { TestedVersion = "1.0" }, CreatedAt).Evidence!;
        var newVersion = Procedure("softwareTest") with { TestedVersion = "2.0", TestDate = TestDate.AddMonths(1) };

        var change = candidate.ChangeEvidence(evidence.Id, newVersion, Later);

        change.Outcome.ShouldBe(EvidenceChangeOutcome.Done);
        candidate.TestEvidences.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            e => e.Id.ShouldBe(evidence.Id),
            e => e.TestedVersion.ShouldBe("2.0"),
            e => e.TestDate.ShouldBe(TestDate.AddMonths(1)));
        candidate.ModifiedAt.ShouldBe(Later);
    }

    [Fact(DisplayName = "INV:WSP-01 " + nameof(ChangeEvidence_ToAProcedureAnotherEvidenceHas_IsRefused))]
    [Trait("Invariant", "WSP-01")]
    public void ChangeEvidence_ToAProcedureAnotherEvidenceHas_IsRefused()
    {
        var candidate = TestedCandidate();
        candidate.AddEvidence(Procedure("softwareTest"), CreatedAt);
        var other = candidate.AddEvidence(Own(), CreatedAt).Evidence!;

        candidate.ChangeEvidence(other.Id, Procedure("softwareTest"), Later).Outcome.ShouldBe(EvidenceChangeOutcome.DuplicateTestType);
        candidate.ChangeEvidence(other.Id, Procedure("bikWeb"), Later).Outcome.ShouldBe(EvidenceChangeOutcome.Done);
    }

    [Fact]
    public void ChangeEvidence_SameFields_KeepsTheLastChange()
    {
        var candidate = TestedCandidate();
        var evidence = candidate.AddEvidence(Procedure("softwareTest"), CreatedAt).Evidence!;

        candidate.ChangeEvidence(evidence.Id, Procedure("softwareTest"), Later);

        candidate.ModifiedAt.ShouldBe(CreatedAt);
    }

    [Theory(DisplayName = "INV:WSP-01 " + nameof(ChangeTestStatus_AwayFromTestedWithEvidences_IsRefusedWithoutRemoval))]
    [Trait("Invariant", "WSP-01")]
    [InlineData(nameof(TestStatus.NotTested))]
    [InlineData(nameof(TestStatus.Unknown))]
    public void ChangeTestStatus_AwayFromTestedWithEvidences_IsRefusedWithoutRemoval(string status)
    {
        var candidate = TestedCandidate();
        candidate.AddEvidence(Procedure("softwareTest"), CreatedAt);

        var outcome = candidate.ChangeTestStatus(Enum.Parse<TestStatus>(status), removeEvidences: false, Later);

        outcome.ShouldBe(TestStatusChangeOutcome.EvidencesExist);
        candidate.TestStatus.ShouldBe(TestStatus.Tested);
        candidate.TestEvidences.Count.ShouldBe(1);
    }

    [Fact(DisplayName = "INV:WSP-01 " + nameof(ChangeTestStatus_AwayFromTestedWithRemoval_RemovesTheEvidences))]
    [Trait("Invariant", "WSP-01")]
    public void ChangeTestStatus_AwayFromTestedWithRemoval_RemovesTheEvidences()
    {
        var candidate = TestedCandidate();
        candidate.AddEvidence(Procedure("softwareTest"), CreatedAt);
        candidate.AddEvidence(Own(), CreatedAt);

        var outcome = candidate.ChangeTestStatus(TestStatus.NotTested, removeEvidences: true, Later);

        outcome.ShouldBe(TestStatusChangeOutcome.Done);
        candidate.TestStatus.ShouldBe(TestStatus.NotTested);
        candidate.TestEvidences.ShouldBeEmpty();
        candidate.ModifiedAt.ShouldBe(Later);
    }

    [Fact]
    public void ChangeTestStatus_SameStatus_ChangesNothing()
    {
        var candidate = TestedCandidate();
        candidate.AddEvidence(Procedure("softwareTest"), CreatedAt);

        var outcome = candidate.ChangeTestStatus(TestStatus.Tested, removeEvidences: true, Later);

        outcome.ShouldBe(TestStatusChangeOutcome.Done);
        candidate.TestEvidences.Count.ShouldBe(1);
        candidate.ModifiedAt.ShouldBe(CreatedAt);
    }

    [Fact]
    public void RemoveEvidence_OneOfTwo_KeepsTheOther()
    {
        var candidate = TestedCandidate();
        var first = candidate.AddEvidence(Procedure("softwareTest"), CreatedAt).Evidence!;
        var second = candidate.AddEvidence(Own(), CreatedAt).Evidence!;

        candidate.RemoveEvidence(first.Id, Later);

        candidate.TestEvidences.ShouldBe([second]);
        candidate.ModifiedAt.ShouldBe(Later);
    }

    [Fact]
    public void EvidenceFacts_KindAndNamesDoNotFit_Throw()
    {
        var candidate = TestedCandidate();

        Should.Throw<ArgumentException>(() => candidate.AddEvidence(Procedure("softwareTest") with { Kind = TestKind.Own }, Later));
        Should.Throw<ArgumentException>(() => candidate.AddEvidence(Other("Name") with { OtherTestTypeName = null }, Later));
        Should.Throw<ArgumentException>(() => candidate.AddEvidence(Own() with { OtherTestTypeName = "Name" }, Later));
        Should.Throw<ArgumentException>(() => candidate.AddEvidence(Own() with { TestedByName = " " }, Later));
        Should.Throw<ArgumentException>(() => candidate.AddEvidence(
            Other(new string('a', TestEvidence.OtherTestTypeNameMaxLength + 1)), Later));
    }

    /// <summary>
    /// The private fields (PRV-01's names) are no member of the two types the publication of step 6
    /// copies; they live in <see cref="CandidatePrivateSection"/> and <see cref="TestEvidencePrivateNote"/>,
    /// which neither type leads to (owner decision S6-15 4 (a)).
    /// </summary>
    [Theory]
    [InlineData(typeof(CandidateProduct))]
    [InlineData(typeof(TestEvidence))]
    public void CopiedTypes_Members_NoPrivateFieldAndNoWayToThePrivateRows(Type type)
    {
        string[] privateNames = ["ContactNotes", "InternalNotes", "TesterQualificationNote", "QualificationNote"];
        var members = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        members.Select(p => p.Name).ShouldNotContain(name => privateNames.Contains(name));
        members.Select(p => p.PropertyType).ShouldNotContain(t => t == typeof(CandidatePrivateSection) || t == typeof(TestEvidencePrivateNote));
        type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Select(f => f.FieldType)
            .ShouldNotContain(t => t == typeof(CandidatePrivateSection) || t == typeof(TestEvidencePrivateNote));
    }

    private static ProcurementProject Project() => ProcurementProject.Create(Guid.NewGuid(), "Projekt", "user-1", CreatedAt);

    private static CandidateProduct Candidate() =>
        CandidateProduct.Create(Project(), new CandidateFacts("Produkt", null, null, null), CreatedAt);

    private static CandidateProduct TestedCandidate()
    {
        var candidate = Candidate();
        candidate.ChangeTestStatus(TestStatus.Tested, removeEvidences: false, CreatedAt);
        return candidate;
    }

    private static TestEvidenceFacts Procedure(string key) =>
        new(TestKind.Procedure, key, null, "Prüfstelle", TestDate, null, null);

    private static TestEvidenceFacts Other(string name) =>
        new(TestKind.Other, null, name, "Prüfstelle", TestDate, null, null);

    private static TestEvidenceFacts Own() =>
        new(TestKind.Own, null, null, "Eigene Fachstelle", TestDate, null, null);
}
