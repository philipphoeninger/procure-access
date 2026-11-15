// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Authorization;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;

namespace ProcureAccess.Modules.IdentityAccess.Tenancy;

/// <summary>
/// Decides an <see cref="OrganizationRoleRequirement"/> for an <see cref="IOrganizationResource"/>
/// from the memberships the middleware loaded (ARCHITECTURE §9.1; TEN-05): a non-member fails with
/// <see cref="OrganizationAccess.NotAMember"/>, a member below the minimum with
/// <see cref="OrganizationAccess.InsufficientRole"/>. The organisation is the resource's; nothing
/// the client sends is read.
/// </summary>
internal sealed class OrganizationRoleHandler(OrganizationContext organizations)
    : AuthorizationHandler<OrganizationRoleRequirement, IOrganizationResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, OrganizationRoleRequirement requirement, IOrganizationResource resource)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            // Left to RequireAuthenticatedUser (401); no reason, so StatusOf answers 404 as for a non-member.
            return Task.CompletedTask;
        }

        var role = organizations.RoleIn(resource.OrganizationId);
        if (role is null)
        {
            context.Fail(new AuthorizationFailureReason(this, OrganizationAccess.NotAMember));
        }
        else if (role < requirement.Minimum)
        {
            context.Fail(new AuthorizationFailureReason(this, OrganizationAccess.InsufficientRole));
        }
        else
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
