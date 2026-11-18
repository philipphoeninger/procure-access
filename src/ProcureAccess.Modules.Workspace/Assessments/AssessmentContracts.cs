// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace ProcureAccess.Modules.Workspace.Assessments;

// Bodies and answers of the assessment endpoints and the count branches of the project and
// candidate views (SPEC R-CAN-06 – R-CAN-08; S6-18). Internal like the endpoints. No answer has a
// score, a percentage or a total verdict (SIG-01), and none has `notCovered` (SIG-13).

/// <summary>
/// Body of <c>PUT /api/v1/candidates/{id}/assessments/{criterionRevisionId}</c>: the whole row.
/// </summary>
/// <param name="Value">
/// Required: <c>fulfilled</c>, <c>slightRestriction</c>, <c>restriction</c>, <c>barrier</c>,
/// <c>blockade</c>, <c>notApplicable</c> — only those the criterion allows (CAT-15) — or
/// <c>unknown</c>, which is always allowed.
/// </param>
/// <param name="Note">At most 1 000 characters after trimming (SPEC §7.2); missing, <c>null</c> or empty for none.</param>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record AssessmentBody(string? Value, string? Note);

/// <summary>An assessment as stored; <c>unknown</c> without a note when no row exists.</summary>
internal sealed record AssessmentResponse(int CriterionRevisionId, string Value, string? Note);

/// <summary>
/// The assessment tab of a candidate (S34b): the included requirement criteria of the project's
/// list in list order with their values, and the derived „Geprüft nach …“ rows (CAT-17).
/// </summary>
/// <param name="ReleaseVersion">The catalog release of the project's list; <c>null</c> while the project has none (then nothing else is filled).</param>
/// <param name="RatingsFromSeveralProcedures">
/// <c>true</c> when the app types of the list are tested by more than one procedure, so a criterion
/// offers the levels of all of them (<c>candidate.assessment.unionNote</c>, C31).
/// </param>
internal sealed record CandidateAssessmentsResponse(
    Guid CandidateId,
    string? ReleaseVersion,
    bool RatingsFromSeveralProcedures,
    AssessmentProgressResponse Progress,
    IReadOnlyList<AssessmentCriterionResponse> Criteria,
    IReadOnlyList<ProcedureAnswerResponse> ProcedureAnswers);

/// <summary>A requirement criterion of the project's list with the candidate's assessment.</summary>
/// <param name="Number">The EN 301 549 number.</param>
/// <param name="PlainLanguageTitle"><c>null</c> until the project team writes it.</param>
/// <param name="AllowedRatings">The levels K6 offers, in the order of the scale; <c>unknown</c> is always allowed and not listed.</param>
/// <param name="Value">The assessment; <c>unknown</c> without a row.</param>
internal sealed record AssessmentCriterionResponse(
    int CriterionRevisionId,
    string? Number,
    string Title,
    string? PlainLanguageTitle,
    string? ProductPart,
    IReadOnlyList<string> AllowedRatings,
    string Value,
    string? Note);

/// <summary>
/// The derived answer of a procedure criterion of the project's list (CAT-17, C30): never stored,
/// never assessed, never counted.
/// </summary>
/// <param name="TestTypeId">The key of the test procedure, as an evidence names it.</param>
/// <param name="Title">The criterion's title, „Geprüft nach …“.</param>
/// <param name="Answer"><c>yes</c> with an evidence of the procedure, <c>no</c> when the product was not tested, else <c>notStated</c>.</param>
internal sealed record ProcedureAnswerResponse(int CriterionRevisionId, string TestTypeId, string Title, string Answer);

/// <summary>
/// How far the assessment is (<c>candidate.assessment.progress</c>): requirement criteria only
/// (R-CAN-08). A measure of the work, not of the product.
/// </summary>
/// <param name="Done">The criteria with a value other than <c>unknown</c>.</param>
/// <param name="Total">The included requirement criteria of the project's list.</param>
internal sealed record AssessmentProgressResponse(int Done, int Total);

/// <summary>
/// K1 in the project scope (ARCHITECTURE C34; SIG-13): how many included requirement criteria of
/// the project's list have which value — always these seven, in this order. The list and the
/// assessments are pinned to the same release, so there is no <c>notCovered</c>.
/// </summary>
internal sealed record FulfillmentCountsResponse(
    int Fulfilled,
    int SlightRestriction,
    int Restriction,
    int Barrier,
    int Blockade,
    int NotApplicable,
    int Unknown);

/// <summary>
/// What the project and candidate views show of a candidate's assessment: progress, the seven
/// counts and the derived procedure answers.
/// </summary>
internal sealed record CandidateAssessmentSummary(
    AssessmentProgressResponse Progress,
    FulfillmentCountsResponse Fulfillment,
    IReadOnlyList<ProcedureAnswerResponse> ProcedureAnswers);
