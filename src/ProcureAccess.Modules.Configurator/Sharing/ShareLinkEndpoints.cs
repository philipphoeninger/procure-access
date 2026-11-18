// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.Configurator.Configurations;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Sharing;

namespace ProcureAccess.Modules.Configurator.Sharing;

/// <summary>
/// The answer of <c>GET /configurations/{id}/share-links</c> for S15 (owner decision S5-24 1a, A (a)):
/// the dates a new link may end on today, worked out by the server in its time zone, and the active links.
/// </summary>
/// <param name="Expiry">Tomorrow, the 🔧 default and the 🔧 maximum (TEN-12).</param>
/// <param name="Links">The links neither revoked nor expired, the oldest first; never a token.</param>
internal sealed record ShareLinksResponse(ShareLinkExpiryWindow Expiry, IReadOnlyList<ActiveShareLink> Links);

/// <summary>
/// The share links of a saved list (SPEC R-SAV-04, SD15; ARCHITECTURE §5.3, §7.2, C59, C81; S5-23):
/// create one, list the active ones, revoke one. The links live in Platform (<see cref="IShareLinks"/>);
/// these endpoints are Configurator's because the role is checked on the list's organisation, and
/// Platform depends on no module. The anonymous read is Platform's <c>GET /shared/{token}</c>.
/// </summary>
/// <remarks>
/// Every endpoint needs a sign-in (401) and Editor or Owner of the list's organisation, taken from the
/// stored list or link through the organisation filter: 404 for a non-member or another
/// organisation's list or link (TEN-01), 403 for a Reader (TEN-05; owner decision S5-23 4a).
/// Creating and revoking change something: a confirmed address (SEC-10; owner decision S5-23 3a)
/// and antiforgery (SEC-02). A project's copy cannot be shared: creating a link answers 409
/// <c>project-copy</c> (C59, TEN-08; S6-12). No rate limit (owner decision S5-23 7).
/// </remarks>
internal static class ShareLinkEndpoints
{
    public const string LinksPath = "/share-links";

    private const string ExpiresOnField = "expiresOn";

    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder routes)
    {
        var ofList = routes.MapGroup($"{ConfigurationEndpoints.GroupPath}/{{id}}{LinksPath}")
                           .WithTags("Configurations")
                           .RequireAuthorization();

        ofList.MapPost("", CreateAsync)
              .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
              .WithName("PostConfigurationShareLink")
              .WithSummary("Creates a read-only link to a saved list (Editor or Owner)")
              .WithDescription(
                  "201 { id, token, expiresAt }: the token only here, the link is /geteilt/<token>; expiresAt is the end of " +
                  "the chosen day. Body { expiresOn: \"yyyy-MM-dd\" }, required, from tomorrow to the maximum (one year). " +
                  "400 validation at 'expiresOn'; 400 antiforgery; 401; 403 email-unconfirmed or below Editor; 404 for an " +
                  "unknown list or one of another organisation; 409 project-copy for a project's list, which is never shared.")
              .Accepts<CreateShareLinkBody>("application/json")
              .Produces<CreatedShareLink>(StatusCodes.Status201Created)
              .ProducesValidationProblem()
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound)
              .ProducesProblem(StatusCodes.Status409Conflict);

        ofList.MapGet("", ListAsync)
              .WithName("GetConfigurationShareLinks")
              .WithSummary("The expiry dates a new link may take and the active links to a saved list (Editor or Owner), for S15")
              .WithDescription(
                  "200 { expiry: { earliest, default, latest }, links: [ { id, createdAt, expiresAt, expiresOn } ] }: the dates " +
                  "(yyyy-MM-dd) counted in the server's time zone; the links neither revoked nor expired, the oldest first, " +
                  "never with a token. 401; 403 below Editor; 404 for an unknown list or one of another organisation.")
              .Produces<ShareLinksResponse>()
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);

        routes.MapDelete($"{LinksPath}/{{id}}", RevokeAsync)
              .WithTags("Configurations")
              .RequireAuthorization()
              .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
              .WithName("DeleteShareLink")
              .WithSummary("Revokes a share link (Editor or Owner of its list's organisation)")
              .WithDescription(
                  "204, also for a link that is already revoked or expired; its token answers 404 from now on. 400 antiforgery; " +
                  "401; 403 email-unconfirmed or below Editor; 404 for an unknown link or one of another organisation.")
              .Produces(StatusCodes.Status204NoContent)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);

        return routes;
    }

    private static async Task<IResult> CreateAsync(
        Guid id,
        CreateShareLinkBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ConfigurationService configurations,
        IShareLinks links,
        CancellationToken cancellationToken)
    {
        // The organisation is the list's, read through the filter (TEN-01): 404 for another's.
        if (await configurations.OwnerOfAsync(id, cancellationToken) is not { } owner)
        {
            return NotFound();
        }

        var organizationId = owner.OrganizationId;
        if (await ConfigurationEndpoints.RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor)
            is { } refused)
        {
            return refused;
        }

        // After the role check, so only a member who may share learns that the list is a project's.
        if (owner.IsProjectCopy)
        {
            return ConfigurationEndpoints.ProjectCopyConflict("A project's criteria list cannot be shared by link.");
        }

        var creation = await links.CreateAsync(
            organizationId, ShareableResourceType.Configuration, id, body.ExpiresOn, cancellationToken);
        return creation.Link is { } link
            ? TypedResults.Created($"/api/v1{ConfigurationEndpoints.GroupPath}/{id}{LinksPath}", link)
            : TypedResults.ValidationProblem(
                new Dictionary<string, string[]>(StringComparer.Ordinal) { [ExpiresOnField] = [creation.ExpiryError!] },
                type: ProblemTypes.Validation);
    }

    private static async Task<IResult> ListAsync(
        Guid id,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ConfigurationService configurations,
        IShareLinks links,
        CancellationToken cancellationToken) =>
        await ConfigurationEndpoints.RefusedForListAsync(authorization, user, configurations, id, cancellationToken) is { } refused
            ? refused
            : TypedResults.Ok(new ShareLinksResponse(
                links.ExpiryWindow(),
                await links.ListActiveAsync(ShareableResourceType.Configuration, id, cancellationToken)));

    private static async Task<IResult> RevokeAsync(
        Guid id,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        IShareLinks links,
        CancellationToken cancellationToken)
    {
        // The organisation is the link's, read through the filter (TEN-01): 404 for another's.
        if (await links.FindAsync(id, cancellationToken) is not { } link)
        {
            return NotFound();
        }

        if (await ConfigurationEndpoints.RefusedAsync(authorization, user, link.OrganizationId, OrganizationRoleRequirement.Editor)
            is { } refused)
        {
            return refused;
        }

        return await links.RevokeAsync(id, cancellationToken) ? TypedResults.NoContent() : NotFound();
    }

    private static ProblemHttpResult NotFound() =>
        TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, type: ProblemTypes.NotFound);
}
