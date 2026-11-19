// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Projects;

namespace ProcureAccess.Modules.Workspace.Candidates;

/// <summary>
/// The candidate products of a project with their private section, test status and test evidences
/// (SPEC R-CAN-02, R-CAN-05; ARCHITECTURE §5.4, §7.2; WSP-01; S6-15): add a candidate, read and
/// change it, set its test status, add, change and remove its evidences. Removing a candidate comes
/// with S6-22 (R-CAN-09), „Merken“ from the directory with M6 (R-CAN-01).
/// </summary>
/// <remarks>
/// <para><b>Sign-in and organisation</b> as for the projects (<see cref="ProjectEndpoints"/>): the
/// organisation is the one of the stored project, candidate or evidence, read through the filter —
/// 404 for one of another organisation (TEN-01) —; every member reads, a change needs Editor
/// (TEN-05). The role check comes before the validation. Changes need a confirmed address (SEC-10)
/// and antiforgery (SEC-02); SPEC §7.3 sets no rate limit.</para>
/// <para><b>Archived projects</b> are read-only (R-PRJ-05): every change answers 409
/// <c>project-archived</c>.</para>
/// <para><b>Test status and evidences</b> (WSP-01, owner decision S6-15 3 (a)): an evidence while
/// the status is not „Ja“, or leaving „Ja“ while evidences exist without <c>removeEvidences</c>,
/// answers 409 <c>test-status-conflict</c> with a field error at <c>testStatus</c> and the extension
/// <c>evidenceCount</c>. A second evidence of the same catalog procedure answers 400 at
/// <c>testTypeId</c> (<c>candidate.evidence.list.duplicate</c>).</para>
/// <para><b>Concurrency (API-03, API-04; S6-20, owner decision D2 (a)).</b> An answer with a
/// candidate carries its <c>ETag</c>, an answer with an evidence the evidence's; every evidence
/// also names its tag as <c>etag</c>, because it is read inside its candidate. Changing a
/// candidate or its test status needs the candidate's tag in <c>If-Match</c>, changing or removing
/// an evidence the evidence's: 428 <c>precondition-required</c> without the header (after the role
/// check, before the validation), 409 <c>conflict</c> with <c>editorDisplayName</c> for a stale
/// one. Adding a candidate or an evidence needs none. An added or removed evidence gives the
/// candidate a new tag (<see cref="CandidateService"/>).</para>
/// </remarks>
internal static class CandidateEndpoints
{
    private const string IfMatchHeader = "If-Match";
    private const string NameField = "name";
    private const string VendorField = "vendor";
    private const string WebsiteField = "website";
    private const string SummaryField = "functionalSummary";
    private const string ContactNotesField = "contactNotes";
    private const string InternalNotesField = "internalNotes";
    private const string TestStatusField = "testStatus";
    private const string TestKindField = "testKind";
    private const string TestTypeField = "testTypeId";
    private const string OtherNameField = "otherTestTypeName";
    private const string TestedByField = "testedBy";
    private const string TestDateField = "testDate";
    private const string VersionField = "testedVersion";
    private const string EvidenceUrlField = "evidenceUrl";
    private const string QualificationField = "qualificationNote";

    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder routes)
    {
        routes.MapGroup(ProjectEndpoints.GroupPath).WithTags("Candidates").RequireAuthorization()
              .MapPost("/{id}/candidates", CreateAsync)
              .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
              .WithName("PostCandidate")
              .WithSummary("Adds a candidate product to a procurement project (Editor or Owner)")
              .WithDescription(
                  "201 with the candidate and its Location; its test status is unknown. Body { name, vendor?, website?, " +
                  "functionalSummary?, contactNotes?, internalNotes? }: the name is required. 400 validation; 400 antiforgery; " +
                  "401; 403 email-unconfirmed or below Editor; 404 for an unknown project or one of another organisation; 409 " +
                  "project-archived.")
              .Accepts<CandidateBody>("application/json")
              .Produces<CandidateResponse>(StatusCodes.Status201Created)
              .ProducesValidationProblem()
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound)
              .ProducesProblem(StatusCodes.Status409Conflict);

        var candidates = routes.MapGroup("/candidates").WithTags("Candidates").RequireAuthorization();

        candidates.MapGet("/{id}", GetAsync)
                  .WithName("GetCandidate")
                  .WithSummary("A candidate product with its private section and test evidences")
                  .WithDescription(
                      "200 for every member of the organisation. 401; 404 for an unknown candidate or one of another organisation.")
                  .Produces<CandidateResponse>()
                  .ProducesProblem(StatusCodes.Status404NotFound);

        candidates.MapPatch("/{id}", ChangeAsync)
                  .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                  .WithName("PatchCandidate")
                  .WithSummary("Changes the facts or the private section of a candidate product (Editor or Owner)")
                  .WithDescription(
                      "200 with the candidate and its new ETag. Needs If-Match with the ETag of the candidate. Body as for " +
                      "adding: what is named changes, an empty text clears an optional field. " +
                      "400 validation; 400 antiforgery; 401; 403 email-unconfirmed or below Editor; 404; 409 conflict with " +
                      "editorDisplayName for a stale If-Match; 409 project-archived; 428 precondition-required without If-Match.")
                  .Accepts<CandidateBody>("application/json")
                  .Produces<CandidateResponse>()
                  .ProducesValidationProblem()
                  .ProducesProblem(StatusCodes.Status403Forbidden)
                  .ProducesProblem(StatusCodes.Status404NotFound)
                  .ProducesProblem(StatusCodes.Status409Conflict)
                  .ProducesProblem(StatusCodes.Status428PreconditionRequired);

        candidates.MapPut("/{id}/test-status", ChangeTestStatusAsync)
                  .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                  .WithName("PutCandidateTestStatus")
                  .WithSummary("Sets the test status of a candidate product (Editor or Owner)")
                  .WithDescription(
                      "200 with the candidate and its new ETag. Needs If-Match with the ETag of the candidate. Body { testStatus: " +
                      "tested | notTested | unknown, removeEvidences? }. 400 validation; " +
                      "400 antiforgery; 401; 403 email-unconfirmed or below Editor; 404; 409 conflict with editorDisplayName " +
                      "for a stale If-Match; 428 precondition-required without If-Match; 409 project-archived; 409 " +
                      "test-status-conflict at testStatus with evidenceCount when it leaves tested while evidences exist and " +
                      "removeEvidences is not true.")
                  .Accepts<TestStatusBody>("application/json")
                  .Produces<CandidateResponse>()
                  .ProducesValidationProblem()
                  .ProducesProblem(StatusCodes.Status403Forbidden)
                  .ProducesProblem(StatusCodes.Status404NotFound)
                  .ProducesProblem(StatusCodes.Status409Conflict)
                  .ProducesProblem(StatusCodes.Status428PreconditionRequired);

        candidates.MapPost("/{id}/test-evidences", AddEvidenceAsync)
                  .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                  .WithName("PostTestEvidence")
                  .WithSummary("Adds a test evidence to a candidate product whose test status is tested (Editor or Owner)")
                  .WithDescription(
                      "201 with the evidence. Body { testKind: procedure | other | own, testTypeId (procedure), otherTestTypeName " +
                      "(other), testedBy, testDate, testedVersion?, evidenceUrl?, qualificationNote? }. 400 validation, also for a " +
                      "procedure that already has an evidence (testTypeId); 400 antiforgery; 401; 403 email-unconfirmed or below " +
                      "Editor; 404; 409 project-archived; 409 test-status-conflict at testStatus when the status is not tested.")
                  .Accepts<TestEvidenceBody>("application/json")
                  .Produces<TestEvidenceResponse>(StatusCodes.Status201Created)
                  .ProducesValidationProblem()
                  .ProducesProblem(StatusCodes.Status403Forbidden)
                  .ProducesProblem(StatusCodes.Status404NotFound)
                  .ProducesProblem(StatusCodes.Status409Conflict);

        var evidences = routes.MapGroup("/test-evidences").WithTags("Candidates").RequireAuthorization();

        evidences.MapPut("/{id}", ChangeEvidenceAsync)
                 .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                 .WithName("PutTestEvidence")
                 .WithSummary("Replaces the fields of a test evidence, e.g. for a new product version (Editor or Owner)")
                 .WithDescription(
                     "200 with the evidence and its new ETag; the row and its id stay. Needs If-Match with the ETag of the " +
                     "evidence. Body as for adding. 400 validation; 400 antiforgery; 401; " +
                     "403 email-unconfirmed or below Editor; 404; 409 conflict with editorDisplayName for a stale If-Match; " +
                     "409 project-archived; 428 precondition-required without If-Match.")
                 .Accepts<TestEvidenceBody>("application/json")
                 .Produces<TestEvidenceResponse>()
                 .ProducesValidationProblem()
                 .ProducesProblem(StatusCodes.Status403Forbidden)
                 .ProducesProblem(StatusCodes.Status404NotFound)
                 .ProducesProblem(StatusCodes.Status409Conflict)
                 .ProducesProblem(StatusCodes.Status428PreconditionRequired);

        evidences.MapDelete("/{id}", RemoveEvidenceAsync)
                 .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                 .WithName("DeleteTestEvidence")
                 .WithSummary("Removes a test evidence (Editor or Owner)")
                 .WithDescription(
                     "204. Needs If-Match with the ETag of the evidence. 400 antiforgery; 401; 403 email-unconfirmed or below " +
                     "Editor; 404; 409 conflict with editorDisplayName for a stale If-Match; 409 project-archived; 428 " +
                     "precondition-required without If-Match.")
                 .Produces(StatusCodes.Status204NoContent)
                 .ProducesProblem(StatusCodes.Status403Forbidden)
                 .ProducesProblem(StatusCodes.Status404NotFound)
                 .ProducesProblem(StatusCodes.Status409Conflict)
                 .ProducesProblem(StatusCodes.Status428PreconditionRequired);

        return routes;
    }

    private static async Task<IResult> CreateAsync(
        Guid id,
        CandidateBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ProjectService projects,
        CandidateService candidates,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        var organizationId = await projects.OrganizationOfAsync(id, cancellationToken);
        if (await WorkspaceAccess.RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor) is { } refused)
        {
            return refused;
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var input = CheckCandidate(body, errors, creating: true);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var result = await candidates.CreateAsync(id, input, WorkspaceAccess.UserIdOf(user), cancellationToken);
        if (result.Outcome != CandidateOutcome.Done)
        {
            return Problem(result);
        }

        response.Headers.ETag = result.Value!.Etag;
        return TypedResults.Created(CandidatePath(result.Value.Id), result.Value);
    }

    // Every member may read (Reader is the lowest role), so the organisation filter is the whole check.
    private static async Task<IResult> GetAsync(
        Guid id, CandidateService candidates, HttpResponse response, CancellationToken cancellationToken)
    {
        if (await candidates.FindAsync(id, cancellationToken) is not { } candidate)
        {
            return WorkspaceAccess.NotFound();
        }

        response.Headers.ETag = candidate.Etag;
        return TypedResults.Ok(candidate);
    }

    private static async Task<IResult> ChangeAsync(
        Guid id,
        CandidateBody body,
        [FromHeader(Name = IfMatchHeader)] string? ifMatch,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        CandidateService candidates,
        IUserDirectory users,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        var organizationId = await candidates.OrganizationOfCandidateAsync(id, cancellationToken);
        if (await WorkspaceAccess.RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor) is { } refused)
        {
            return refused;
        }

        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return WorkspaceAccess.PreconditionRequired();
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var input = CheckCandidate(body, errors, creating: false);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var result = await candidates.ChangeAsync(id, input, EditOf(user, ifMatch), cancellationToken);
        return await AnswerAsync(result, c => c.Etag, users, response, cancellationToken);
    }

    private static async Task<IResult> ChangeTestStatusAsync(
        Guid id,
        TestStatusBody body,
        [FromHeader(Name = IfMatchHeader)] string? ifMatch,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        CandidateService candidates,
        IUserDirectory users,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        var organizationId = await candidates.OrganizationOfCandidateAsync(id, cancellationToken);
        if (await WorkspaceAccess.RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor) is { } refused)
        {
            return refused;
        }

        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return WorkspaceAccess.PreconditionRequired();
        }

        var status = body.TestStatus is null ? null : CandidateNames.StatusOf(body.TestStatus);
        if (status is null)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    [TestStatusField] = [$"The test status must be one of: {string.Join(", ", CandidateNames.StatusNames)}."],
                },
                type: ProblemTypes.Validation);
        }

        var result = await candidates.ChangeTestStatusAsync(
            id, status.Value, body.RemoveEvidences == true, EditOf(user, ifMatch), cancellationToken);
        return await AnswerAsync(result, c => c.Etag, users, response, cancellationToken);
    }

    private static async Task<IResult> AddEvidenceAsync(
        Guid id,
        TestEvidenceBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        CandidateService candidates,
        ICatalogTestTypes testTypes,
        WorkspaceSettings settings,
        TimeProvider clock,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        var organizationId = await candidates.OrganizationOfCandidateAsync(id, cancellationToken);
        if (await WorkspaceAccess.RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor) is { } refused)
        {
            return refused;
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var input = CheckEvidence(body, await testTypes.GetKeysAsync(cancellationToken), settings.Today(clock.GetUtcNow()), errors);
        if (input is null)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var result = await candidates.AddEvidenceAsync(id, input, WorkspaceAccess.UserIdOf(user), cancellationToken);
        if (result.Outcome != CandidateOutcome.Done)
        {
            return Problem(result);
        }

        response.Headers.ETag = result.Value!.Etag;
        return TypedResults.Created(EvidencePath(result.Value.Id), result.Value);
    }

    private static async Task<IResult> ChangeEvidenceAsync(
        Guid id,
        TestEvidenceBody body,
        [FromHeader(Name = IfMatchHeader)] string? ifMatch,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        CandidateService candidates,
        ICatalogTestTypes testTypes,
        WorkspaceSettings settings,
        TimeProvider clock,
        IUserDirectory users,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        var organizationId = await candidates.OrganizationOfEvidenceAsync(id, cancellationToken);
        if (await WorkspaceAccess.RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor) is { } refused)
        {
            return refused;
        }

        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return WorkspaceAccess.PreconditionRequired();
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var input = CheckEvidence(body, await testTypes.GetKeysAsync(cancellationToken), settings.Today(clock.GetUtcNow()), errors);
        if (input is null)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var result = await candidates.ChangeEvidenceAsync(id, input, EditOf(user, ifMatch), cancellationToken);
        return await AnswerAsync(result, e => e.Etag, users, response, cancellationToken);
    }

    private static async Task<IResult> RemoveEvidenceAsync(
        Guid id,
        [FromHeader(Name = IfMatchHeader)] string? ifMatch,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        CandidateService candidates,
        IUserDirectory users,
        CancellationToken cancellationToken)
    {
        var organizationId = await candidates.OrganizationOfEvidenceAsync(id, cancellationToken);
        if (await WorkspaceAccess.RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor) is { } refused)
        {
            return refused;
        }

        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return WorkspaceAccess.PreconditionRequired();
        }

        var result = await candidates.RemoveEvidenceAsync(id, EditOf(user, ifMatch), cancellationToken);
        return result.Outcome switch
        {
            CandidateOutcome.Done => TypedResults.NoContent(),
            CandidateOutcome.Conflict => await WorkspaceAccess.ConflictAsync(users, result.EditorUserId, cancellationToken),
            _ => Problem(result),
        };
    }

    private static EditPrecondition EditOf(ClaimsPrincipal user, string ifMatch) => new(WorkspaceAccess.UserIdOf(user), ifMatch);

    /// <summary>
    /// The answer of a change that needed <c>If-Match</c>: 200 with the value and its new
    /// <c>ETag</c>, 409 <c>conflict</c> with the editor's name, or <see cref="Problem{T}"/>.
    /// </summary>
    private static async Task<IResult> AnswerAsync<T>(
        CandidateResult<T> result, Func<T, string> etagOf, IUserDirectory users, HttpResponse response, CancellationToken cancellationToken)
        where T : class
    {
        switch (result.Outcome)
        {
            case CandidateOutcome.Done:
                response.Headers.ETag = etagOf(result.Value!);
                return TypedResults.Ok(result.Value);
            case CandidateOutcome.Conflict:
                return await WorkspaceAccess.ConflictAsync(users, result.EditorUserId, cancellationToken);
            default:
                return Problem(result);
        }
    }

    /// <summary>
    /// Checks the fields of a candidate (SPEC §7.2; owner decision S6-15 2 (a)): trimmed, within their
    /// limits, the website an <c>https</c> address. Creating needs a name; a change leaves out what
    /// is <c>null</c> and refuses an empty name.
    /// </summary>
    private static CandidateEdit CheckCandidate(CandidateBody body, Dictionary<string, string[]> errors, bool creating)
    {
        var name = InputCheck.Text(body.Name, NameField, CandidateProduct.NameMaxLength, errors);
        if (name is "" || (creating && name is null))
        {
            errors[NameField] = ["The name is required."];
        }

        return new CandidateEdit(
            name,
            InputCheck.Text(body.Vendor, VendorField, CandidateProduct.NameMaxLength, errors),
            InputCheck.Url(body.Website, WebsiteField, CandidateProduct.WebsiteUrlMaxLength, errors),
            InputCheck.Text(body.FunctionalSummary, SummaryField, CandidateProduct.FunctionalSummaryMaxLength, errors),
            InputCheck.Text(body.ContactNotes, ContactNotesField, CandidatePrivateSection.NotesMaxLength, errors),
            InputCheck.Text(body.InternalNotes, InternalNotesField, CandidatePrivateSection.NotesMaxLength, errors));
    }

    /// <summary>
    /// Checks the fields of an evidence (SPEC §7.2; R-CAN-05; owner decision S6-15 2 (a)): the kind,
    /// the procedure known to the catalog with <c>procedure</c> and only then, the name with
    /// <c>other</c> and only then, the tester and a date not after <paramref name="today"/>, the
    /// optional fields within their limits. <c>null</c> with <paramref name="errors"/> filled when
    /// one fails.
    /// </summary>
    private static EvidenceInput? CheckEvidence(
        TestEvidenceBody body, IReadOnlyList<string> testTypeKeys, DateOnly today, Dictionary<string, string[]> errors)
    {
        var kind = body.TestKind is null ? null : CandidateNames.KindOf(body.TestKind);
        if (kind is null)
        {
            errors[TestKindField] = [$"The test kind must be one of: {string.Join(", ", CandidateNames.KindNames)}."];
        }

        var testType = InputCheck.Text(body.TestTypeId, TestTypeField, int.MaxValue, errors);
        if (kind == TestKind.Procedure && !testTypeKeys.Contains(testType ?? string.Empty, StringComparer.Ordinal))
        {
            errors[TestTypeField] = [$"The test type must be one of: {string.Join(", ", testTypeKeys)}."];
        }
        else if (kind is not null and not TestKind.Procedure && !string.IsNullOrEmpty(testType))
        {
            errors[TestTypeField] = ["Only an evidence of the kind procedure names a test type."];
        }

        var otherName = InputCheck.Text(body.OtherTestTypeName, OtherNameField, TestEvidence.OtherTestTypeNameMaxLength, errors);
        if (kind == TestKind.Other && string.IsNullOrEmpty(otherName))
        {
            errors[OtherNameField] = ["The name of the procedure is required."];
        }
        else if (kind is not null and not TestKind.Other && !string.IsNullOrEmpty(otherName))
        {
            errors[OtherNameField] = ["Only an evidence of the kind other names its procedure."];
        }

        var testedBy = InputCheck.Text(body.TestedBy, TestedByField, TestEvidence.TestedByNameMaxLength, errors);
        if (string.IsNullOrEmpty(testedBy))
        {
            errors[TestedByField] = ["Who tested is required."];
        }

        if (body.TestDate is null)
        {
            errors[TestDateField] = ["The test date is required."];
        }
        else if (body.TestDate > today)
        {
            errors[TestDateField] = ["The test date lies in the future."];
        }

        var version = InputCheck.Text(body.TestedVersion, VersionField, TestEvidence.TestedVersionMaxLength, errors);
        var url = InputCheck.Url(body.EvidenceUrl, EvidenceUrlField, TestEvidence.EvidenceUrlMaxLength, errors);
        var note = InputCheck.Text(body.QualificationNote, QualificationField, TestEvidencePrivateNote.QualificationNoteMaxLength, errors);
        if (errors.Count > 0)
        {
            return null;
        }

        var facts = new TestEvidenceFacts(
            kind!.Value,
            kind == TestKind.Procedure ? testType : null,
            kind == TestKind.Other ? otherName : null,
            testedBy!,
            body.TestDate!.Value,
            NullIfEmpty(version),
            NullIfEmpty(url));
        return new EvidenceInput(facts, NullIfEmpty(note));
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrEmpty(value) ? null : value;

    /// <summary>
    /// The answer of a request that did not end <see cref="CandidateOutcome.Done"/>; a
    /// <see cref="CandidateOutcome.Conflict"/> is answered by its handler, which knows the editor.
    /// </summary>
    private static IResult Problem<T>(CandidateResult<T> result) where T : class => result.Outcome switch
    {
        CandidateOutcome.NotFound => WorkspaceAccess.NotFound(),
        CandidateOutcome.ProjectArchived => WorkspaceAccess.ProjectArchived(),
        CandidateOutcome.TestStatusConflict => TypedResults.Problem(new HttpValidationProblemDetails(
            new Dictionary<string, string[]>
            {
                [TestStatusField] = [result.EvidenceCount > 0
                    ? "The test evidences must be removed before the test status leaves tested."
                    : "Test evidences exist only while the test status is tested."],
            })
        {
            Status = StatusCodes.Status409Conflict,
            Type = ProblemTypes.TestStatusConflict,
            Detail = "The test status and the test evidences do not fit.",
            Extensions = { ["evidenceCount"] = result.EvidenceCount },
        }),
        CandidateOutcome.DuplicateTestType => TypedResults.ValidationProblem(
            new Dictionary<string, string[]> { [TestTypeField] = ["This procedure already has a test evidence."] },
            type: ProblemTypes.Validation),
        _ => throw new InvalidOperationException($"Unknown outcome {result.Outcome} of a candidate request."),
    };

    private static string CandidatePath(Guid id) => $"/api/v1/candidates/{id}";

    private static string EvidencePath(Guid id) => $"/api/v1/test-evidences/{id}";
}
