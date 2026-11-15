// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Authorization;

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;

/// <summary>
/// Resource-based authorisation of ARCHITECTURE §9.1 (SPEC R-ORG-05, TEN-05): the signed-in user
/// must be a member of the resource's organisation with at least <see cref="Minimum"/>. Check it with
/// <c>IAuthorizationService.AuthorizeAsync(user, resource, OrganizationRoleRequirement.Editor)</c> on an
/// <see cref="IOrganizationResource"/>; the IdentityAccess module registers the handler, which reads
/// the memberships loaded for the request (no query). Answer a failure with
/// <see cref="OrganizationAccess.StatusOf"/>: a non-member must not learn that the resource exists.
/// </summary>
public sealed class OrganizationRoleRequirement : IAuthorizationRequirement
{
    public static readonly OrganizationRoleRequirement Reader = new(OrganizationRole.Reader);

    public static readonly OrganizationRoleRequirement Editor = new(OrganizationRole.Editor);

    public static readonly OrganizationRoleRequirement Owner = new(OrganizationRole.Owner);

    private OrganizationRoleRequirement(OrganizationRole minimum)
    {
        Minimum = minimum;
    }

    /// <summary>The least role that satisfies the requirement.</summary>
    public OrganizationRole Minimum { get; }

    /// <summary>The requirement for <paramref name="minimum"/>.</summary>
    public static OrganizationRoleRequirement For(OrganizationRole minimum) => minimum switch
    {
        OrganizationRole.Reader => Reader,
        OrganizationRole.Editor => Editor,
        OrganizationRole.Owner => Owner,
        _ => throw new ArgumentOutOfRangeException(nameof(minimum), minimum, "Unknown organisation role."),
    };

    public override string ToString() => $"Organisation role at least {Minimum}";
}

/// <summary>
/// Why an <see cref="OrganizationRoleRequirement"/> failed, as the handler's
/// <see cref="AuthorizationFailureReason.Message"/>.
/// </summary>
public static class OrganizationAccess
{
    /// <summary>The user is signed in but not a member of the organisation: the resource is invisible (404).</summary>
    public const string NotAMember = "organisation:not-a-member";

    /// <summary>The user is a member, but the role is below the minimum (403).</summary>
    public const string InsufficientRole = "organisation:insufficient-role";

    /// <summary>
    /// The status an endpoint answers for a failed check: 404 when the user is no member (also
    /// anonymous — add <c>RequireAuthorization()</c> to the endpoint, so a missing sign-in is 401
    /// before the check), 403 for a member whose role is too low. Returns <c>null</c> on success.
    /// </summary>
    public static int? StatusOf(AuthorizationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Succeeded)
        {
            return null;
        }

        var reasons = result.Failure?.FailureReasons.Select(reason => reason.Message).ToList() ?? [];
        return reasons.Contains(InsufficientRole) && !reasons.Contains(NotAMember) ? 403 : 404;
    }
}
