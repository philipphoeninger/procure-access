// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
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
/// other than „Nicht eingeschätzt“ exists, 409 <c>locked-criteria-list</c> (C47; the check is
/// <see cref="ICriteriaListLock"/>, real with S6-18). A source that is unknown, of another
/// organisation or a project copy answers 400 <c>validation</c> at <c>sourceConfigurationId</c>
/// (WSP-02): one answer for all three, so the request learns nothing about lists it cannot use.
/// No <c>ETag</c>/<c>If-Match</c> until S6-20 (API-03); the last assignment wins.
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
                  "unchanged. Body { sourceConfigurationId }: a list of the project's organisation that is not a project's " +
                  "copy. 400 validation at 'sourceConfigurationId' (missing, unknown, of another organisation or a project's " +
                  "copy); 400 antiforgery; 401; 403 email-unconfirmed or below Editor; 404 for an unknown project or one of " +
                  "another organisation; 409 project-archived for an archived project; 409 locked-criteria-list once a product " +
                  "of the project has an assessment.")
              .Accepts<AssignCriteriaListBody>("application/json")
              .Produces<ProjectCriteriaListResponse>()
              .ProducesValidationProblem()
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound)
              .ProducesProblem(StatusCodes.Status409Conflict);
        return routes;
    }

    private static async Task<IResult> AssignAsync(
        Guid id,
        AssignCriteriaListBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ProjectService projects,
        ProjectCriteriaListService criteriaLists,
        CancellationToken cancellationToken)
    {
        if (await ProjectEndpoints.RefusedForProjectAsync(authorization, user, projects, id, cancellationToken) is { } refused)
        {
            return refused;
        }

        if (body.SourceConfigurationId is not { } sourceId)
        {
            return SourceProblem("The list to copy is required.");
        }

        var assignment = await criteriaLists.AssignAsync(id, sourceId, ProjectEndpoints.UserIdOf(user), cancellationToken);
        return assignment.Outcome switch
        {
            CriteriaListAssignmentOutcome.Done => TypedResults.Ok(assignment.List),
            CriteriaListAssignmentOutcome.NotFound => ProjectEndpoints.NotFound(),
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
