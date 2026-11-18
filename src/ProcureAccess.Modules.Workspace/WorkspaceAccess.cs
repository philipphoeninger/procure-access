// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace ProcureAccess.Modules.Workspace;

/// <summary>
/// The role check and the common answers of the workspace endpoints (TEN-01, TEN-05): the
/// organisation is the one of the route or of the stored resource read through the filter, never
/// a header.
/// </summary>
internal static class WorkspaceAccess
{
    /// <summary>
    /// The role check on a stored resource whose organisation <paramref name="organizationId"/> was
    /// read through the filter: 404 when it does not exist for the request (TEN-01), else as
    /// <see cref="RefusedAsync(IAuthorizationService, ClaimsPrincipal, Guid, OrganizationRoleRequirement)"/>.
    /// </summary>
    public static async Task<IResult?> RefusedAsync(
        IAuthorizationService authorization, ClaimsPrincipal user, Guid? organizationId, OrganizationRoleRequirement requirement) =>
        organizationId is { } id ? await RefusedAsync(authorization, user, id, requirement) : NotFound();

    /// <summary>The 404 or 403 of a failed role check (TEN-05), or <c>null</c> when it passed.</summary>
    public static async Task<ProblemHttpResult?> RefusedAsync(
        IAuthorizationService authorization, ClaimsPrincipal user, Guid organizationId, OrganizationRoleRequirement requirement)
    {
        var result = await authorization.AuthorizeAsync(user, new OrganizationReference(organizationId), requirement);
        return OrganizationAccess.StatusOf(result) is { } status ? TypedResults.Problem(statusCode: status) : null;
    }

    public static ProblemHttpResult NotFound() =>
        TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, type: ProblemTypes.NotFound);

    /// <summary>409 <c>project-archived</c>: nothing of an archived project changes but its status (R-PRJ-05).</summary>
    public static ProblemHttpResult ProjectArchived() =>
        TypedResults.Problem(
            detail: "The project is archived; only its status can change.",
            statusCode: StatusCodes.Status409Conflict,
            type: ProblemTypes.ProjectArchived);

    /// <summary>The signed-in user's id; the groups require a sign-in, so it is always there.</summary>
    public static string UserIdOf(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("A signed-in principal without a user id reached a workspace endpoint.");
}
