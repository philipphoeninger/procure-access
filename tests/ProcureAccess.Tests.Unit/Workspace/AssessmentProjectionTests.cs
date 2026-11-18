// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using System.Text.RegularExpressions;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Workspace.Assessments;
using ProcureAccess.Modules.Workspace.Candidates;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Projects;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Workspace;

/// <summary>
/// The rules of an assessment without a database (S6-18): the allowed-ratings union (CAT-15), the
/// derived answer of a procedure criterion (CAT-17), the seven counts and the progress (SIG-13,
/// R-CAN-08), and the lazy row (C53).
/// </summary>
public sealed class AssessmentProjectionTests
{
    private const Rating SoftwareTest = Rating.All & ~Rating.NotApplicable;
    private const Rating AppTest = Rating.Fulfilled | Rating.Barrier | Rating.Blockade | Rating.NotApplicable;

    private static readonly DateTimeOffset Now = new(2026, 11, 18, 9, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// The union is the catalog's (<c>ResolvedCatalogCriterion.AllowedRatings</c>): a level one of
    /// the app types' procedures allows is allowed, „Nicht eingeschätzt“ always, every level without
    /// a procedure text (<see cref="Rating.All"/>), none but „Nicht eingeschätzt“ for a procedure criterion.
    /// </summary>
    [Theory(DisplayName = "INV:CAT-15 " + nameof(IsAllowed_ValueAndUnion_FollowsTheTable))]
    [Trait("Invariant", "CAT-15")]
    // Website alone (Software-Test).
    [InlineData(SoftwareTest, nameof(Fulfillment.SlightRestriction), true)]
    [InlineData(SoftwareTest, nameof(Fulfillment.NotApplicable), false)]
    // Mobile Anwendung alone (App-Test).
    [InlineData(AppTest, nameof(Fulfillment.NotApplicable), true)]
    [InlineData(AppTest, nameof(Fulfillment.Restriction), false)]
    // Website + Mobile Anwendung: what either allows.
    [InlineData(SoftwareTest | AppTest, nameof(Fulfillment.NotApplicable), true)]
    [InlineData(SoftwareTest | AppTest, nameof(Fulfillment.Restriction), true)]
    // No procedure text: all six.
    [InlineData(Rating.All, nameof(Fulfillment.Blockade), true)]
    // „Nicht eingeschätzt“ is always allowed.
    [InlineData(SoftwareTest, nameof(Fulfillment.Unknown), true)]
    [InlineData(Rating.Fulfilled, nameof(Fulfillment.Unknown), true)]
    [InlineData(Rating.None, nameof(Fulfillment.Unknown), true)]
    [InlineData(Rating.None, nameof(Fulfillment.Fulfilled), false)]
    public void IsAllowed_ValueAndUnion_FollowsTheTable(Rating allowed, string value, bool expected) =>
        AssessmentProjection.IsAllowed(Enum.Parse<Fulfillment>(value), allowed).ShouldBe(expected);

    [Fact]
    public void Names_OfValuesAndRatings_AreTheWireNamesInTheOrderOfTheScale()
    {
        AssessmentProjection.ValueNames.ShouldBe(
            ["fulfilled", "slightRestriction", "restriction", "barrier", "blockade", "notApplicable", "unknown"]);
        foreach (var value in Enum.GetValues<Fulfillment>())
        {
            AssessmentProjection.ValueOf(AssessmentProjection.NameOf(value)).ShouldBe(value);
        }

        AssessmentProjection.ValueOf("Fulfilled").ShouldBeNull();
        AssessmentProjection.NamesOf(AppTest).ShouldBe(["fulfilled", "barrier", "blockade", "notApplicable"]);
        AssessmentProjection.NamesOf(Rating.None).ShouldBeEmpty();
    }

    [Theory(DisplayName = "INV:CAT-17 " + nameof(AnswerOf_ProcedureCriterion_IsDerivedFromEvidencesAndTestStatus))]
    [Trait("Invariant", "CAT-17")]
    [InlineData(nameof(TestStatus.Tested), "softwareTest", "yes")]
    [InlineData(nameof(TestStatus.Tested), "bikWeb", "notStated")]
    [InlineData(nameof(TestStatus.Tested), null, "notStated")]
    [InlineData(nameof(TestStatus.NotTested), null, "no")]
    [InlineData(nameof(TestStatus.Unknown), null, "notStated")]
    public void AnswerOf_ProcedureCriterion_IsDerivedFromEvidencesAndTestStatus(string status, string? evidenceOf, string expected) =>
        AssessmentProjection.AnswerOf(Enum.Parse<TestStatus>(status), evidenceOf is null ? [] : [evidenceOf], "softwareTest").ShouldBe(expected);

    [Fact(DisplayName = "INV:CAT-17 " + nameof(SummaryOf_ListWithProcedureCriterion_CountsRequirementCriteriaOnly))]
    [Trait("Invariant", "CAT-17")]
    public void SummaryOf_ListWithProcedureCriterion_CountsRequirementCriteriaOnly()
    {
        var procedure = Procedure(90, "softwareTest");
        var scope = Scope([Requirement(1), Requirement(2)], [procedure]);
        // A value stored for the procedure criterion (which the service refuses) would still not count.
        var values = new Dictionary<int, Fulfillment> { [1] = Fulfillment.Fulfilled, [90] = Fulfillment.Fulfilled };

        var summary = AssessmentProjection.SummaryOf(scope, values, TestStatus.Tested, ["softwareTest"]);

        summary.Progress.ShouldBe(new AssessmentProgressResponse(1, 2));
        summary.Fulfillment.ShouldBe(new FulfillmentCountsResponse(1, 0, 0, 0, 0, 0, 1));
        summary.ProcedureAnswers.ShouldHaveSingleItem().ShouldBe(new ProcedureAnswerResponse(90, "softwareTest", "Geprüft nach softwareTest", "yes"));
    }

    [Fact(DisplayName = "INV:SIG-13 " + nameof(SummaryOf_Values_GivesSevenCountsThatAddUpToTheIncludedRequirementCriteria))]
    [Trait("Invariant", "SIG-13")]
    public void SummaryOf_Values_GivesSevenCountsThatAddUpToTheIncludedRequirementCriteria()
    {
        var scope = Scope([.. Enumerable.Range(1, 9).Select(Requirement)], []);
        var values = new Dictionary<int, Fulfillment>
        {
            [1] = Fulfillment.Fulfilled,
            [2] = Fulfillment.Fulfilled,
            [3] = Fulfillment.SlightRestriction,
            [4] = Fulfillment.Restriction,
            [5] = Fulfillment.Barrier,
            [6] = Fulfillment.Blockade,
            [7] = Fulfillment.NotApplicable,
            [8] = Fulfillment.Unknown,
            // An excluded criterion, or one of a replaced list: kept, not counted (C51).
            [77] = Fulfillment.Blockade,
        };

        var summary = AssessmentProjection.SummaryOf(scope, values, TestStatus.Unknown, []);

        summary.Fulfillment.ShouldBe(new FulfillmentCountsResponse(2, 1, 1, 1, 1, 1, 2));
        summary.Progress.ShouldBe(new AssessmentProgressResponse(7, 9));
    }

    /// <summary>
    /// The contract of the workspace projections (C34): the count branch has exactly seven
    /// properties in the fixed order and no <c>notCovered</c>; no view of a project or candidate
    /// has a member that reads like a verdict (SIG-01).
    /// </summary>
    [Fact(DisplayName = "INV:SIG-13 " + nameof(Contracts_OfTheWorkspaceViews_HaveExactlySevenCountsAndNoVerdict))]
    [Trait("Invariant", "SIG-13")]
    public void Contracts_OfTheWorkspaceViews_HaveExactlySevenCountsAndNoVerdict()
    {
        var counts = JsonSerializer.SerializeToElement(new FulfillmentCountsResponse(1, 2, 3, 4, 5, 6, 7), JsonSerializerOptions.Web);

        counts.EnumerateObject().Select(p => p.Name).ShouldBe(
            ["fulfilled", "slightRestriction", "restriction", "barrier", "blockade", "notApplicable", "unknown"]);
        typeof(CandidateSummaryResponse).GetProperty(nameof(CandidateSummaryResponse.Fulfillment))!.PropertyType.ShouldBe(typeof(FulfillmentCountsResponse));
        typeof(CandidateResponse).GetProperty(nameof(CandidateResponse.Fulfillment))!.PropertyType.ShouldBe(typeof(FulfillmentCountsResponse));
        Type[] views =
        [
            typeof(ProjectResponse), typeof(ProjectSummaryResponse), typeof(CandidateSummaryResponse), typeof(CandidateResponse),
            typeof(CandidateAssessmentsResponse), typeof(AssessmentCriterionResponse), typeof(AssessmentProgressResponse),
            typeof(FulfillmentCountsResponse), typeof(ProcedureAnswerResponse), typeof(AssessmentResponse),
        ];
        var names = views.SelectMany(v => v.GetProperties().Select(p => $"{v.Name}.{p.Name}")).ToList();
        names.ShouldNotContain(n => Regex.IsMatch(n.Split('.')[1], "notCovered|score|percent|overall|average|total(?!$)", RegexOptions.IgnoreCase));
        // „Ratings“ are the levels K6 offers for a criterion, as in the catalog's answer; nothing rates the product.
        names.Where(n => n.Contains("Rating", StringComparison.OrdinalIgnoreCase)).ShouldBe(
        [
            $"{nameof(CandidateAssessmentsResponse)}.{nameof(CandidateAssessmentsResponse.RatingsFromSeveralProcedures)}",
            $"{nameof(AssessmentCriterionResponse)}.{nameof(AssessmentCriterionResponse.AllowedRatings)}",
        ]);
    }

    [Fact]
    public void Change_Assessment_SaysWhetherItChangedAndWhenItSaysNothing()
    {
        var project = ProcurementProject.Create(Guid.NewGuid(), "Projekt", "user", Now);
        var candidate = CandidateProduct.Create(project, new CandidateFacts("Produkt", null, null, null), Now);
        var assessment = CandidateAssessment.Of(candidate, 7);

        assessment.ShouldSatisfyAllConditions(
            a => a.SaysNothing.ShouldBeTrue(),
            a => a.OrganizationId.ShouldBe(project.OrganizationId),
            a => a.CandidateProductId.ShouldBe(candidate.Id),
            a => a.CriterionRevisionId.ShouldBe(7));
        assessment.Change(Fulfillment.Unknown, null, Now).ShouldBeFalse();
        assessment.Change(Fulfillment.Unknown, "Nachfragen", Now).ShouldBeTrue();
        assessment.SaysNothing.ShouldBeFalse();
        assessment.Change(Fulfillment.Barrier, "Nachfragen", Now.AddHours(1)).ShouldBeTrue();
        assessment.Change(Fulfillment.Barrier, "Nachfragen", Now.AddHours(2)).ShouldBeFalse();
        assessment.ModifiedAt.ShouldBe(Now.AddHours(1));
        assessment.Change(Fulfillment.Unknown, null, Now.AddHours(3)).ShouldBeTrue();
        assessment.SaysNothing.ShouldBeTrue();
        Should.Throw<ArgumentException>(() => assessment.Change(Fulfillment.Fulfilled, string.Empty, Now));
        Should.Throw<ArgumentException>(
            () => assessment.Change(Fulfillment.Fulfilled, new string('n', CandidateAssessment.EvidenceNoteMaxLength + 1), Now));
        Should.Throw<ArgumentOutOfRangeException>(() => assessment.Change((Fulfillment)99, null, Now));
    }

    private static ProjectCriteriaScope Scope(ResolvedCatalogCriterion[] requirements, ResolvedCatalogCriterion[] procedures) =>
        new("T.1", requirements, procedures, RatingsFromSeveralProcedures: false);

    private static ResolvedCatalogCriterion Requirement(int revisionId) => new(
        revisionId, $"99.{revisionId}", CriterionKind.Requirement, $"99.{revisionId}", $"Titel {revisionId}", null, null, null, null,
        new CatalogPriority(PriorityLevel.B, 1, "Begründung"), null, Rating.All, null);

    private static ResolvedCatalogCriterion Procedure(int revisionId, string testType) => new(
        revisionId, $"PROC-{testType}", CriterionKind.TestProcedure, null, $"Geprüft nach {testType}", null, null, null, null,
        new CatalogPriority(PriorityLevel.B, 1, "Begründung"), null, Rating.None, testType);
}
