// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Organizations;

/// <summary>
/// The organisation part of the E2E seeder (TESTING §6.3; owner decision S5-16 7a): one organisation
/// with an Owner, an Editor and a Reader, created once. The host calls it through
/// <see cref="IdentityAccessModule.SeedTestUsersAsync"/> only in <c>Development</c> and
/// <c>E2E</c> and only when the E2E users are switched on; this class checks nothing about the
/// environment.
/// </summary>
internal sealed class TestOrganizationSeeder(IdentityAccessDbContext db, TimeProvider clock)
{
    /// <summary>
    /// Creates the organisation <paramref name="id"/> with the members, which the E2E tests address by
    /// that known id. An organisation of the same Owner is left as it is (a second start). One of
    /// another Owner is replaced with its memberships: the Development rebuild deletes every user on
    /// each start and the users are created again with new ids, so its memberships would name users
    /// that no longer exist. Returns true when it created the organisation.
    /// </summary>
    public async Task<bool> SeedAsync(
        Guid id, string name, IReadOnlyList<(string UserId, OrganizationRole Role)> members, CancellationToken cancellationToken)
    {
        var owner = members.First(m => m.Role == OrganizationRole.Owner).UserId;

        // Outside a request the filter shows no organisation: the one read of the seeder goes past
        // it (TEN-02 allow-list).
        var existing = await db.Organizations.IgnoreQueryFilters().SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (existing is not null)
        {
            if (existing.CreatedByUserId == owner)
            {
                return false;
            }

            // The memberships go with it (cascade).
            db.Organizations.Remove(existing);
            await db.SaveChangesAsync(cancellationToken);
        }

        var now = clock.GetUtcNow();
        var organization = Organization.Create(name, OrganizationType.PublicBody, owner, now, id);
        db.Organizations.Add(organization);
        db.OrganizationMemberships.Add(OrganizationMembership.Founder(organization, now));
        foreach (var (userId, role) in members.Where(m => m.UserId != owner))
        {
            db.OrganizationMemberships.Add(OrganizationMembership.Create(organization.Id, userId, role, now, now));
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
