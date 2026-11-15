// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Tenancy;

/// <summary>
/// Fills the request's <see cref="OrganizationContext"/> with the signed-in user's memberships
/// (ARCHITECTURE §5.4; S5-15). Runs after authentication and before authorization, so the
/// organisation query filters and <see cref="OrganizationRoleHandler"/> see the same set. One
/// indexed query per signed-in request, none for an anonymous one. Read from the database on every
/// request, not from a claim: a removed member loses access with the next request (R-ORG-03).
/// </summary>
internal sealed class OrganizationContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext, OrganizationContext organizations, IdentityAccessDbContext db)
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (httpContext.User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(userId))
        {
            // The one allow-listed unfiltered read of the memberships (TEN-02): the user's own
            // rows are what defines the filter, so they cannot be read through it.
            var memberships = await db.OrganizationMemberships
                                      .IgnoreQueryFilters()
                                      .AsNoTracking()
                                      .Where(m => m.UserId == userId)
                                      .Select(m => new KeyValuePair<Guid, OrganizationRole>(m.OrganizationId, m.Role))
                                      .ToListAsync(httpContext.RequestAborted);
            organizations.Load(memberships);
        }
        else
        {
            organizations.Load([]);
        }

        await next(httpContext);
    }
}
