// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Organizations;

/// <summary>
/// <see cref="IOrganizationMembers"/> on <see cref="IdentityAccessDbContext"/> (S5-16a). Every read
/// goes through the organisation query filter of the request. A role change and a removal run in a
/// transaction that first locks the organisation row, so two Owners who demote or remove each other
/// at the same moment cannot both pass the last-Owner check (TEN-11).
/// </summary>
internal sealed class OrganizationMemberService(IdentityAccessDbContext db, TimeProvider clock) : IOrganizationMembers
{
    /// <summary>German collation for names („Ämter“ before „Bauamt“), as the header shows them.</summary>
    private static readonly StringComparer NameOrder = StringComparer.Create(CultureInfo.GetCultureInfo("de-DE"), ignoreCase: true);

    public async Task<Guid> CreateAsync(
        string creatorUserId, string name, OrganizationType type, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var organization = Organization.Create(name, type, creatorUserId, now);
        db.Organizations.Add(organization);
        db.OrganizationMemberships.Add(OrganizationMembership.Founder(organization, now));
        await db.SaveChangesAsync(cancellationToken);
        return organization.Id;
    }

    public async Task<OrganizationMembers?> GetAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var organization = await db.Organizations.AsNoTracking()
                                   .SingleOrDefaultAsync(o => o.Id == organizationId, cancellationToken);
        if (organization is null)
        {
            return null;
        }

        var members = await db.OrganizationMemberships.AsNoTracking()
                              .Where(m => m.OrganizationId == organizationId)
                              .Select(m => new OrganizationMember(m.UserId, m.Role))
                              .ToListAsync(cancellationToken);

        // The role is stored by name, so the order of the enum is applied here.
        return new OrganizationMembers(
            organization.Id,
            organization.Name,
            organization.Type,
            [.. members.OrderByDescending(m => m.Role).ThenBy(m => m.UserId, StringComparer.Ordinal)]);
    }

    public async Task<IReadOnlyList<OrganizationSummary>> ListOfAsync(string userId, CancellationToken cancellationToken)
    {
        // Both sets go through the organisation filter of the request, so a caller only ever lists
        // organisations it is a member of, whatever user id it passes (TEN-01).
        var organizations = await (from m in db.OrganizationMemberships.AsNoTracking()
                                   join o in db.Organizations.AsNoTracking() on m.OrganizationId equals o.Id
                                   where m.UserId == userId
                                   select new OrganizationSummary(o.Id, o.Name, o.Type, m.Role))
                                  .ToListAsync(cancellationToken);
        return [.. organizations.OrderBy(o => o.Name, NameOrder).ThenBy(o => o.Id)];
    }

    public Task<MembershipChangeResult> ChangeRoleAsync(
        Guid organizationId, string userId, OrganizationRole role, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown organisation role.");
        }

        return ChangeLockedAsync(organizationId, userId, role, cancellationToken);
    }

    public Task<MembershipChangeResult> RemoveAsync(Guid organizationId, string userId, CancellationToken cancellationToken) =>
        ChangeLockedAsync(organizationId, userId, newRole: null, cancellationToken);

    /// <summary>Gives the membership <paramref name="newRole"/>, or deletes it for <c>null</c>, under the organisation's lock.</summary>
    private async Task<MembershipChangeResult> ChangeLockedAsync(
        Guid organizationId, string userId, OrganizationRole? newRole, CancellationToken cancellationToken)
    {
        // The context retries on transient failures, so a transaction of our own runs inside its
        // execution strategy, and a retry starts again from the database.
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
            async ct =>
            {
                db.ChangeTracker.Clear();
                await using var transaction = await db.Database.BeginTransactionAsync(ct);

                // Visible through the filter first: a non-member's request never takes the lock.
                if (!await db.Organizations.AnyAsync(o => o.Id == organizationId, ct))
                {
                    return MembershipChangeResult.NotFound;
                }

                // Serialises every role change, removal and invitation of this organisation until the commit.
                await OrganizationRowLock.LockAsync(db, organizationId, ct);

                var membership = await db.OrganizationMemberships
                                         .SingleOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId, ct);
                if (membership is null)
                {
                    return MembershipChangeResult.NotFound;
                }

                if (membership.Role == newRole)
                {
                    return MembershipChangeResult.Done;
                }

                var owners = await db.OrganizationMemberships
                                     .CountAsync(m => m.OrganizationId == organizationId && m.Role == OrganizationRole.Owner, ct);
                if (OrganizationMembership.LeavesNoOwner(membership.Role, newRole, owners))
                {
                    return MembershipChangeResult.LastOwner;
                }

                if (newRole is { } role)
                {
                    membership.ChangeRole(role);
                }
                else
                {
                    db.OrganizationMemberships.Remove(membership);
                }

                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return MembershipChangeResult.Done;
            },
            cancellationToken);
    }
}
