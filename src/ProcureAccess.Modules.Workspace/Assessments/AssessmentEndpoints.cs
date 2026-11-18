// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Workspace.Candidates;
using ProcureAccess.Modules.Workspace.Domain;

namespace ProcureAccess.Modules.Workspace.Assessments;

/// <summary>
/// The assessments of a candidate product (SPEC R-CAN-06 – R-CAN-08; ARCHITECTURE §5.4, §7.2;
/// CAT-15, CAT-17; S6-18): the assessment tab S34b reads its rows, and autosave sets one row at a
/// time.
/// </summary>
/// <remarks>
/// <para><b>Sign-in and organisation</b> as for the candidates (<see cref="CandidateEndpoints"/>):
/// the organisation is the one of the stored candidate, read through the filter — 404 for one of
/// another organisation (TEN-01) —; every member reads, a change needs Editor (TEN-05), a confirmed
/// address (SEC-10) and antiforgery (SEC-02). The role check comes before the validation.</para>
/// <para><b>Concurrency.</b> The last write of a row wins; these rows get no <c>If-Match</c>, also
/// not with S6-20 (R-CAN-11).</para>
/// </remarks>
internal static class AssessmentEndpoints
{
    private const string CriterionField = "criterionRevisionId";
    private const string ValueField = "value";
    private const string NoteField = "note";

    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder routes)
    {
        var candidates = routes.MapGroup("/candidates").WithTags("Assessments").RequireAuthorization();

        candidates.MapGet("/{id}/assessments", GetAsync)
                  .WithName("GetCandidateAssessments")
                  .WithSummary("The assessment of a candidate product against the criteria list of its project")
                  .WithDescription(
                      "200 for every member of the organisation: the included requirement criteria of the project's list in " +
                      "list order with allowedRatings, value and note, the derived procedureAnswers (yes | no | notStated), the " +
                      "progress and ratingsFromSeveralProcedures; releaseVersion is null and the lists are empty while the " +
                      "project has no list. 401; 404 for an unknown candidate or one of another organisation.")
                  .Produces<CandidateAssessmentsResponse>()
                  .ProducesProblem(StatusCodes.Status404NotFound);

        candidates.MapPut("/{id}/assessments/{criterionRevisionId}", SetAsync)
                  .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                  .WithName("PutCandidateAssessment")
                  .WithSummary("Sets the assessment of one criterion for a candidate product (Editor or Owner)")
                  .WithDescription(
                      "200 with the row. Body { value, note? }: value is unknown or a level the criterion allows; the last write " +
                      "wins. 400 validation at value (unknown name, or not allowed for the criterion), at note (over 1 000 " +
                      "characters) or at criterionRevisionId (not an included requirement criterion of the project's list — also " +
                      "a procedure criterion); 400 antiforgery; 401; 403 email-unconfirmed or below Editor; 404; 409 " +
                      "project-archived.")
                  .Accepts<AssessmentBody>("application/json")
                  .Produces<AssessmentResponse>()
                  .ProducesValidationProblem()
                  .ProducesProblem(StatusCodes.Status403Forbidden)
                  .ProducesProblem(StatusCodes.Status404NotFound)
                  .ProducesProblem(StatusCodes.Status409Conflict);

        return routes;
    }

    // Every member may read (Reader is the lowest role), so the organisation filter is the whole check.
    private static async Task<IResult> GetAsync(Guid id, AssessmentReader assessments, CancellationToken cancellationToken) =>
        await assessments.FindAsync(id, cancellationToken) is { } found ? TypedResults.Ok(found) : WorkspaceAccess.NotFound();

    private static async Task<IResult> SetAsync(
        Guid id,
        int criterionRevisionId,
        AssessmentBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        CandidateService candidates,
        AssessmentService assessments,
        CancellationToken cancellationToken)
    {
        var organizationId = await candidates.OrganizationOfCandidateAsync(id, cancellationToken);
        if (await WorkspaceAccess.RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor) is { } refused)
        {
            return refused;
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var value = body.Value is null ? null : AssessmentProjection.ValueOf(body.Value);
        if (value is null)
        {
            errors[ValueField] = [$"The value must be one of: {string.Join(", ", AssessmentProjection.ValueNames)}."];
        }

        var note = InputCheck.Text(body.Note, NoteField, CandidateAssessment.EvidenceNoteMaxLength, errors);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var result = await assessments.SetAsync(
            id, criterionRevisionId, value!.Value, string.IsNullOrEmpty(note) ? null : note, cancellationToken);
        return result.Outcome switch
        {
            AssessmentOutcome.Done => TypedResults.Ok(result.Assessment),
            AssessmentOutcome.NotFound => WorkspaceAccess.NotFound(),
            AssessmentOutcome.ProjectArchived => WorkspaceAccess.ProjectArchived(),
            AssessmentOutcome.NotInList => Invalid(
                CriterionField, "Only an included requirement criterion of the project's criteria list can be assessed."),
            AssessmentOutcome.ValueNotAllowed => Invalid(
                ValueField, $"The criterion allows: {string.Join(", ", [.. result.AllowedRatings!, "unknown"])}."),
            _ => throw new InvalidOperationException($"Unknown outcome {result.Outcome} of an assessment."),
        };
    }

    private static IResult Invalid(string field, string message) =>
        TypedResults.ValidationProblem(
            new Dictionary<string, string[]>(StringComparer.Ordinal) { [field] = [message] },
            type: ProblemTypes.Validation);
}
