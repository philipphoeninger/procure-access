// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace ProcureAccess.Modules.Workspace.Projects;

/// <summary>
/// <c>PUT /api/v1/projects/{id}/criteria-configuration</c> (SPEC R-PRJ-02, R-PRJ-04; ARCHITECTURE
/// §7.2, C47, C53; WSP-02; S6-12): the project gets its own copy of an organisation list, replacing
/// the copy it had. The source list stays unchanged. The copy is read and adjusted through the
/// saved-list endpoints with its id (C51; S11 in project-copy mode).
/// </summary>
/// <remarks>
/// Editor or Owner of the project's organisation, taken from the stored project (404 non-member or
/// another organisation's project, 403 Reader; TEN-01, TEN-05), a confirmed address (SEC-10) and
/// antiforgery (SEC-02). The role check comes before the body's check, as on the other project
/// endpoints. An archived project answers 409 <c>project-archived</c> (R-PRJ-05); once an assessment
/// other than „Nicht eingeschätzt“ exists, 409 <c>locked-criteria-list</c> (C47;
/// <see cref="ProjectCriteriaListService.IsLockedAsync"/>, on the assessment rows since S6-18). A source that is unknown, of another
/// organisation or a project copy answers 400 <c>validation</c> at <c>sourceConfigurationId</c>
/// (WSP-02): one answer for all three, so the request learns nothing about lists it cannot use.
/// <para><b>Concurrency (API-03; S6-20, owner decision D2 (a)).</b> The assignment is a change of
/// the project: it needs the project's <c>ETag</c> in <c>If-Match</c> (428 without, 409
/// <c>conflict</c> when stale), and the <c>ETag</c> of its answer is the project's new one, not a
/// tag of the copy.</para>
/// </remarks>
internal static class ProjectCriteriaListEndpoint
{
    public const string Path = "/{id}/criteria-configuration";

    private const string SourceField = "sourceConfigurationId";

    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder routes)
    {
        routes.MapGroup(ProjectEndpoints.GroupPath)
              .WithTags("Projects")
              .RequireAuthorization()
              .MapPut(Path, AssignAsync)
              .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
              .WithName("PutProjectCriteriaConfiguration")
              .WithSummary("Gives the project its own copy of a saved list of the organisation (Editor or Owner)")
              .WithDescription(
                  "200 { id, name, releaseVersion }: the project's copy, which replaces its previous one; the source list is " +
                  "unchanged. Needs If-Match with the ETag of the project; the ETag of the answer is the project's new one. Body { sourceConfigurationId }: a list of the project's organisation that is not a project's " +
                  "copy. 400 validation at 'sourceConfigurationId' (missing, unknown, of another organisation or a project's " +
                  "copy); 400 antiforgery; 401; 403 email-unconfirmed or below Editor; 404 for an unknown project or one of " +
                  "another organisation; 409 conflict with editorDisplayName for a stale If-Match; 409 project-archived for an " +
                  "archived project; 409 locked-criteria-list once a product of the project has an assessment; 428 " +
                  "precondition-required without If-Match.")
              .Accepts<AssignCriteriaListBody>("application/json")
              .Produces<ProjectCriteriaListResponse>()
              .ProducesValidationProblem()
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound)
              .ProducesProblem(StatusCodes.Status409Conflict)
              .ProducesProblem(StatusCodes.Status428PreconditionRequired);
        return routes;
    }

    private static async Task<IResult> AssignAsync(
        Guid id,
        AssignCriteriaListBody body,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ProjectService projects,
        ProjectCriteriaListService criteriaLists,
        IUserDirectory users,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        var organizationId = await projects.OrganizationOfAsync(id, cancellationToken);
        if (await WorkspaceAccess.RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor) is { } refused)
        {
            return refused;
        }

        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return WorkspaceAccess.PreconditionRequired();
        }

        if (body.SourceConfigurationId is not { } sourceId)
        {
            return SourceProblem("The list to copy is required.");
        }

        var assignment = await criteriaLists.AssignAsync(
            id, sourceId, new EditPrecondition(WorkspaceAccess.UserIdOf(user), ifMatch), cancellationToken);
        if (assignment.Outcome == CriteriaListAssignmentOutcome.Done)
        {
            response.Headers.ETag = assignment.ProjectEtag;
        }

        return assignment.Outcome switch
        {
            CriteriaListAssignmentOutcome.Conflict =>
                await WorkspaceAccess.ConflictAsync(users, assignment.EditorUserId, cancellationToken),
            CriteriaListAssignmentOutcome.Done => TypedResults.Ok(assignment.List),
            CriteriaListAssignmentOutcome.NotFound => WorkspaceAccess.NotFound(),
            CriteriaListAssignmentOutcome.Archived => TypedResults.Problem(
                detail: "The project is archived; its criteria list cannot change.",
                statusCode: StatusCodes.Status409Conflict,
                type: ProblemTypes.ProjectArchived),
            // „409 with field error“ (ARCHITECTURE §5.4, C47).
            CriteriaListAssignmentOutcome.Locked => TypedResults.Problem(
                detail: "Products of the project have been assessed; its criteria list can be adjusted but not replaced.",
                statusCode: StatusCodes.Status409Conflict,
                type: ProblemTypes.LockedCriteriaList,
                extensions: new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["errors"] = new Dictionary<string, string[]>(StringComparer.Ordinal)
                    {
                        [SourceField] = ["The criteria list can no longer be replaced."],
                    },
                }),
            CriteriaListAssignmentOutcome.SourceNotFound or CriteriaListAssignmentOutcome.SourceIsProjectCopy =>
                SourceProblem("Choose a saved list of the project's organisation; the lists of projects cannot be chosen."),
            _ => throw new InvalidOperationException($"Unknown outcome {assignment.Outcome} of a list assignment."),
        };
    }

    private static IResult SourceProblem(string message) =>
        TypedResults.ValidationProblem(
            new Dictionary<string, string[]>(StringComparer.Ordinal) { [SourceField] = [message] },
            type: ProblemTypes.Validation);
}
