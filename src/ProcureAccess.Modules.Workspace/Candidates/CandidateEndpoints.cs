// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
/// <para><b>Concurrency.</b> No <c>ETag</c>/<c>If-Match</c> yet: API-03 comes with S6-20.</para>
/// </remarks>
internal static class CandidateEndpoints
{
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
                      "200 with the candidate. Body as for adding: what is named changes, an empty text clears an optional field. " +
                      "400 validation; 400 antiforgery; 401; 403 email-unconfirmed or below Editor; 404; 409 project-archived.")
                  .Accepts<CandidateBody>("application/json")
                  .Produces<CandidateResponse>()
                  .ProducesValidationProblem()
                  .ProducesProblem(StatusCodes.Status403Forbidden)
                  .ProducesProblem(StatusCodes.Status404NotFound)
                  .ProducesProblem(StatusCodes.Status409Conflict);

        candidates.MapPut("/{id}/test-status", ChangeTestStatusAsync)
                  .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                  .WithName("PutCandidateTestStatus")
                  .WithSummary("Sets the test status of a candidate product (Editor or Owner)")
                  .WithDescription(
                      "200 with the candidate. Body { testStatus: tested | notTested | unknown, removeEvidences? }. 400 validation; " +
                      "400 antiforgery; 401; 403 email-unconfirmed or below Editor; 404; 409 project-archived; 409 " +
                      "test-status-conflict at testStatus with evidenceCount when it leaves tested while evidences exist and " +
                      "removeEvidences is not true.")
                  .Accepts<TestStatusBody>("application/json")
                  .Produces<CandidateResponse>()
                  .ProducesValidationProblem()
                  .ProducesProblem(StatusCodes.Status403Forbidden)
                  .ProducesProblem(StatusCodes.Status404NotFound)
                  .ProducesProblem(StatusCodes.Status409Conflict);

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
                     "200 with the evidence; the row and its id stay. Body as for adding. 400 validation; 400 antiforgery; 401; " +
                     "403 email-unconfirmed or below Editor; 404; 409 project-archived.")
                 .Accepts<TestEvidenceBody>("application/json")
                 .Produces<TestEvidenceResponse>()
                 .ProducesValidationProblem()
                 .ProducesProblem(StatusCodes.Status403Forbidden)
                 .ProducesProblem(StatusCodes.Status404NotFound)
                 .ProducesProblem(StatusCodes.Status409Conflict);

        evidences.MapDelete("/{id}", RemoveEvidenceAsync)
                 .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                 .WithName("DeleteTestEvidence")
                 .WithSummary("Removes a test evidence (Editor or Owner)")
                 .WithDescription(
                     "204. 400 antiforgery; 401; 403 email-unconfirmed or below Editor; 404; 409 project-archived.")
                 .Produces(StatusCodes.Status204NoContent)
                 .ProducesProblem(StatusCodes.Status403Forbidden)
                 .ProducesProblem(StatusCodes.Status404NotFound)
                 .ProducesProblem(StatusCodes.Status409Conflict);

        return routes;
    }

    private static async Task<IResult> CreateAsync(
        Guid id,
        CandidateBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ProjectService projects,
        CandidateService candidates,
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

        var result = await candidates.CreateAsync(id, input, cancellationToken);
        return result.Outcome == CandidateOutcome.Done
            ? TypedResults.Created(CandidatePath(result.Value!.Id), result.Value)
            : Problem(result);
    }

    // Every member may read (Reader is the lowest role), so the organisation filter is the whole check.
    private static async Task<IResult> GetAsync(Guid id, CandidateService candidates, CancellationToken cancellationToken) =>
        await candidates.FindAsync(id, cancellationToken) is { } candidate ? TypedResults.Ok(candidate) : WorkspaceAccess.NotFound();

    private static async Task<IResult> ChangeAsync(
        Guid id,
        CandidateBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        CandidateService candidates,
        CancellationToken cancellationToken)
    {
        var organizationId = await candidates.OrganizationOfCandidateAsync(id, cancellationToken);
        if (await WorkspaceAccess.RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor) is { } refused)
        {
            return refused;
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var input = CheckCandidate(body, errors, creating: false);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var result = await candidates.ChangeAsync(id, input, cancellationToken);
        return result.Outcome == CandidateOutcome.Done ? TypedResults.Ok(result.Value) : Problem(result);
    }

    private static async Task<IResult> ChangeTestStatusAsync(
        Guid id,
        TestStatusBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        CandidateService candidates,
        CancellationToken cancellationToken)
    {
        var organizationId = await candidates.OrganizationOfCandidateAsync(id, cancellationToken);
        if (await WorkspaceAccess.RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor) is { } refused)
        {
            return refused;
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

        var result = await candidates.ChangeTestStatusAsync(id, status.Value, body.RemoveEvidences == true, cancellationToken);
        return result.Outcome == CandidateOutcome.Done ? TypedResults.Ok(result.Value) : Problem(result);
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

        var result = await candidates.AddEvidenceAsync(id, input, cancellationToken);
        return result.Outcome == CandidateOutcome.Done
            ? TypedResults.Created(EvidencePath(result.Value!.Id), result.Value)
            : Problem(result);
    }

    private static async Task<IResult> ChangeEvidenceAsync(
        Guid id,
        TestEvidenceBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        CandidateService candidates,
        ICatalogTestTypes testTypes,
        WorkspaceSettings settings,
        TimeProvider clock,
        CancellationToken cancellationToken)
    {
        var organizationId = await candidates.OrganizationOfEvidenceAsync(id, cancellationToken);
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

        var result = await candidates.ChangeEvidenceAsync(id, input, cancellationToken);
        return result.Outcome == CandidateOutcome.Done ? TypedResults.Ok(result.Value) : Problem(result);
    }

    private static async Task<IResult> RemoveEvidenceAsync(
        Guid id,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        CandidateService candidates,
        CancellationToken cancellationToken)
    {
        var organizationId = await candidates.OrganizationOfEvidenceAsync(id, cancellationToken);
        if (await WorkspaceAccess.RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor) is { } refused)
        {
            return refused;
        }

        var outcome = await candidates.RemoveEvidenceAsync(id, cancellationToken);
        return outcome == CandidateOutcome.Done
            ? TypedResults.NoContent()
            : Problem(CandidateResult<TestEvidenceResponse>.Of(outcome));
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

    /// <summary>The answer of a request that did not end <see cref="CandidateOutcome.Done"/>.</summary>
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
