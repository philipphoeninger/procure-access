// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Workspace.Domain;

namespace ProcureAccess.Modules.Workspace.Projects;

/// <summary>
/// The organisation's procurement projects (SPEC R-PRJ-01, R-PRJ-05; ARCHITECTURE §7.2; S6-11):
/// list the projects of one organisation, create one, read one, change its name or status. There
/// is no endpoint that deletes a project (SPEC §3).
/// </summary>
/// <remarks>
/// <para><b>Sign-in and organisation.</b> Every endpoint needs a sign-in (401). Listing and creating
/// name their organisation in the route, never in a header; it is checked with
/// <see cref="OrganizationRoleRequirement"/> before anything else — Reader to list, Editor to
/// create —, so a non-member gets 404 and a Reader who creates 403 (TEN-05). Reading and changing
/// one project take the organisation from the stored project: the organisation filter answers 404
/// for a project of another organisation (TEN-01); every member may read, a change needs Editor
/// on it.</para>
/// <para><b>Changes.</b> Creating and changing need a confirmed address
/// (<see cref="IdentityAccessPolicies.ConfirmedEmail"/>, SEC-10) and antiforgery like every
/// state-changing endpoint (SEC-02). SPEC §7.3 sets no rate limit.</para>
/// <para><b>Archived projects</b> are read-only for everyone (R-PRJ-05): only the status may
/// change; a change that names a name answers 409 <c>project-archived</c>, also together with a
/// status (owner decisions S6-11 1 (a), 2 (a)).</para>
/// <para><b>Concurrency.</b> No <c>ETag</c>/<c>If-Match</c> yet: API-03 comes with S6-20; until
/// then the last change wins.</para>
/// <para><b>Ids.</b> No route constraint on the ids, so a malformed id answers 400
/// <c>validation</c> like every other bad input (API-01), as the saved lists do.</para>
/// </remarks>
internal static class ProjectEndpoints
{
    public const string GroupPath = "/projects";

    private const string NameField = "name";
    private const string StatusField = "status";

    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder routes)
    {
        var ofOrganization = routes.MapGroup("/organizations/{organizationId}/projects").WithTags("Projects").RequireAuthorization();

        ofOrganization.MapGet("", ListAsync)
                      .WithName("GetProjects")
                      .WithSummary("The procurement projects of the organisation (Reader and up), for S30")
                      .WithDescription(
                          "200 [ { id, name, status, productCount, modifiedAt } ]: every project, also the archived ones, the " +
                          "most recently changed first. status is open, done or archived. 401; 404 for a non-member.")
                      .Produces<IReadOnlyList<ProjectSummaryResponse>>()
                      .ProducesProblem(StatusCodes.Status404NotFound);

        ofOrganization.MapPost("", CreateAsync)
                      .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                      .WithName("PostProject")
                      .WithSummary("Creates a procurement project of the organisation (Editor or Owner)")
                      .WithDescription(
                          "201 with the project and its Location; its status is open. Body { name } (required, at most 120 " +
                          "characters). 400 validation; 400 antiforgery; 401; 403 email-unconfirmed or below Editor; 404 for a " +
                          "non-member.")
                      .Accepts<CreateProjectBody>("application/json")
                      .Produces<ProjectResponse>(StatusCodes.Status201Created)
                      .ProducesValidationProblem()
                      .ProducesProblem(StatusCodes.Status403Forbidden)
                      .ProducesProblem(StatusCodes.Status404NotFound);

        var projects = routes.MapGroup(GroupPath).WithTags("Projects").RequireAuthorization();

        projects.MapGet("/{id}", GetAsync)
                .WithName("GetProject")
                .WithSummary("A procurement project")
                .WithDescription(
                    "200 for every member of the project's organisation. 401; 404 for an unknown project or one of another " +
                    "organisation.")
                .Produces<ProjectResponse>()
                .ProducesProblem(StatusCodes.Status404NotFound);

        projects.MapPatch("/{id}", ChangeAsync)
                .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                .WithName("PatchProject")
                .WithSummary("Changes the name or the status of a procurement project (Editor or Owner)")
                .WithDescription(
                    "200 with the project. Body { name?, status? }: what is named changes; status is open, done or archived. " +
                    "400 validation; 400 antiforgery; 401; 403 email-unconfirmed or below Editor; 404 for an unknown project or " +
                    "one of another organisation; 409 project-archived when the body names a name and the project is archived " +
                    "(only its status may change).")
                .Accepts<ChangeProjectBody>("application/json")
                .Produces<ProjectResponse>()
                .ProducesValidationProblem()
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict);

        return routes;
    }

    private static async Task<IResult> ListAsync(
        Guid organizationId,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ProjectService projects,
        CancellationToken cancellationToken)
    {
        if (await RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Reader) is { } refused)
        {
            return refused;
        }

        return TypedResults.Ok(await projects.ListAsync(organizationId, cancellationToken));
    }

    private static async Task<IResult> CreateAsync(
        Guid organizationId,
        CreateProjectBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ProjectService projects,
        CancellationToken cancellationToken)
    {
        if (await RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor) is { } refused)
        {
            return refused;
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var name = body.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            errors[NameField] = ["The name is required."];
        }
        else
        {
            CheckNameLength(name, errors);
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var project = await projects.CreateAsync(organizationId, name!, UserIdOf(user), cancellationToken);
        return TypedResults.Created($"/api/v1{GroupPath}/{project.Id}", project);
    }

    // Every member may read (Reader is the lowest role), so the organisation filter is the whole check.
    private static async Task<Results<Ok<ProjectResponse>, ProblemHttpResult>> GetAsync(
        Guid id,
        ProjectService projects,
        CancellationToken cancellationToken) =>
        await projects.FindAsync(id, cancellationToken) is { } project ? TypedResults.Ok(project) : NotFound();

    private static async Task<IResult> ChangeAsync(
        Guid id,
        ChangeProjectBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ProjectService projects,
        CancellationToken cancellationToken)
    {
        if (await RefusedForProjectAsync(authorization, user, projects, id, cancellationToken) is { } refused)
        {
            return refused;
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var name = body.Name?.Trim();
        if (name is not null)
        {
            if (name.Length == 0)
            {
                errors[NameField] = ["The name must not be empty."];
            }
            else
            {
                CheckNameLength(name, errors);
            }
        }

        ProjectStatus? status = null;
        if (body.Status is not null)
        {
            status = ProjectService.StatusOf(body.Status);
            if (status is null)
            {
                errors[StatusField] = [$"The status must be one of: {string.Join(", ", ProjectService.StatusNames)}."];
            }
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var change = await projects.ChangeAsync(id, name, status, cancellationToken);
        return change.Outcome switch
        {
            ProjectChangeOutcome.Done => TypedResults.Ok(change.Project),
            ProjectChangeOutcome.NotFound => NotFound(),
            ProjectChangeOutcome.Archived => TypedResults.Problem(
                detail: "The project is archived; only its status can change.",
                statusCode: StatusCodes.Status409Conflict,
                type: ProblemTypes.ProjectArchived),
            _ => throw new InvalidOperationException($"Unknown outcome {change.Outcome} of a project change."),
        };
    }

    private static void CheckNameLength(string name, Dictionary<string, string[]> errors)
    {
        if (name.Length > ProcurementProject.NameMaxLength)
        {
            errors[NameField] = [$"The name has at most {ProcurementProject.NameMaxLength} characters."];
        }
    }

    /// <summary>
    /// The role check of a change to a stored project (TEN-05): the organisation is the project's,
    /// read through the filter — 404 for an unknown project or one of another organisation
    /// (TEN-01) —, then Editor on it — 403 for a Reader. <c>null</c> when it passed.
    /// </summary>
    private static async Task<IResult?> RefusedForProjectAsync(
        IAuthorizationService authorization,
        ClaimsPrincipal user,
        ProjectService projects,
        Guid id,
        CancellationToken cancellationToken) =>
        await projects.OrganizationOfAsync(id, cancellationToken) is { } organizationId
            ? await RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor)
            : NotFound();

    /// <summary>The 404 or 403 of a failed role check (TEN-05), or <c>null</c> when it passed.</summary>
    private static async Task<ProblemHttpResult?> RefusedAsync(
        IAuthorizationService authorization, ClaimsPrincipal user, Guid organizationId, OrganizationRoleRequirement requirement)
    {
        var result = await authorization.AuthorizeAsync(user, new OrganizationReference(organizationId), requirement);
        return OrganizationAccess.StatusOf(result) is { } status ? TypedResults.Problem(statusCode: status) : null;
    }

    private static ProblemHttpResult NotFound() =>
        TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, type: ProblemTypes.NotFound);

    /// <summary>The signed-in user's id; the groups require a sign-in, so it is always there.</summary>
    private static string UserIdOf(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("A signed-in principal without a user id reached a project endpoint.");
}
