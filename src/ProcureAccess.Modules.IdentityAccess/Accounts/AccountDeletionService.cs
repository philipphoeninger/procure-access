// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Organizations;
using ProcureAccess.Modules.IdentityAccess.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Accounts;

/// <summary>
/// <see cref="IAccountDeletion"/> on <see cref="IdentityAccessDbContext"/> (S5-28a). Each step runs
/// in one transaction that first locks the rows of the user's organisations, in the order of their
/// ids, so the rule is checked against memberships no role change, removal or acceptance changes
/// meanwhile (TEN-11), and two deletions never wait for each other.
/// </summary>
/// <remarks>
/// Every read and delete goes through the organisation query filter: the user's own memberships are
/// exactly the organisations the filter lets through. The contributor profiles and their links stay
/// until step 6 builds what R-AUTH-10 asks for them (owner decision S5-28 3).
/// </remarks>
internal sealed class AccountDeletionService(IdentityAccessDbContext db) : IAccountDeletion
{
    /// <summary>German collation for organisation names, as the header lists them.</summary>
    private static readonly StringComparer NameOrder = StringComparer.Create(CultureInfo.GetCultureInfo("de-DE"), ignoreCase: true);

    public Task<AccountDeletionStart> BeginAsync(string userId, CancellationToken cancellationToken) =>
        InTransactionAsync(
            userId,
            async (state, ct) =>
            {
                if (state.SoleOwnerOf.Count == 0 && state.OnlyMemberOf.Count > 0)
                {
                    // Only an invitation lets someone join, and only an Owner — this user — sends one.
                    await db.OrganizationInvitations
                            .Where(i => state.OnlyMemberOf.Contains(i.OrganizationId))
                            .ExecuteDeleteAsync(ct);
                }

                return new AccountDeletionStart(state.SoleOwnerOf, state.SoleOwnerOf.Count == 0 ? state.OnlyMemberOf : []);
            },
            cancellationToken);

    public Task<IReadOnlyList<string>> CompleteAsync(string userId, CancellationToken cancellationToken) =>
        InTransactionAsync(
            userId,
            async (state, ct) =>
            {
                if (state.SoleOwnerOf.Count > 0)
                {
                    return state.SoleOwnerOf;
                }

                // The organisation's memberships and invitations go with it (cascade).
                await db.Organizations.Where(o => state.OnlyMemberOf.Contains(o.Id)).ExecuteDeleteAsync(ct);
                await db.OrganizationMemberships.Where(m => m.UserId == userId).ExecuteDeleteAsync(ct);
                await db.UserPreferences.Where(p => p.UserId == userId).ExecuteDeleteAsync(ct);
                await db.AccountTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync(ct);
                return state.SoleOwnerOf;
            },
            cancellationToken);

    /// <summary>
    /// Runs <paramref name="step"/> with the user's organisations locked and examined, and commits
    /// what it changed.
    /// </summary>
    private async Task<T> InTransactionAsync<T>(
        string userId, Func<Memberships, CancellationToken, Task<T>> step, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        // The context retries on transient failures, so a transaction of our own runs inside its
        // execution strategy, and a retry starts again from the database.
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
            async ct =>
            {
                db.ChangeTracker.Clear();
                await using var transaction = await db.Database.BeginTransactionAsync(ct);
                var result = await step(await ExamineLockedAsync(userId, ct), ct);
                await transaction.CommitAsync(ct);
                return result;
            },
            cancellationToken);
    }

    /// <summary>Locks the user's organisations and sorts them by what the deletion means for them.</summary>
    private async Task<Memberships> ExamineLockedAsync(string userId, CancellationToken cancellationToken)
    {
        var organizationIds = await db.OrganizationMemberships.AsNoTracking()
                                      .Where(m => m.UserId == userId)
                                      .Select(m => m.OrganizationId)
                                      .OrderBy(id => id)
                                      .ToListAsync(cancellationToken);
        foreach (var id in organizationIds)
        {
            await OrganizationRowLock.LockAsync(db, id, cancellationToken);
        }

        // Read after the locks: what counts is the state no other change can alter any more.
        var members = await (from m in db.OrganizationMemberships.AsNoTracking()
                             join o in db.Organizations.AsNoTracking() on m.OrganizationId equals o.Id
                             where organizationIds.Contains(m.OrganizationId)
                             select new { m.OrganizationId, o.Name, m.UserId, m.Role })
                            .ToListAsync(cancellationToken);

        var soleOwnerOf = new List<string>();
        var onlyMemberOf = new List<Guid>();
        foreach (var organization in members.GroupBy(m => (m.OrganizationId, m.Name)))
        {
            var own = organization.SingleOrDefault(m => m.UserId == userId);
            if (own is null)
            {
                continue; // left or removed while this request waited for the lock
            }

            if (organization.Count() == 1)
            {
                onlyMemberOf.Add(organization.Key.OrganizationId);
            }
            else if (own.Role == OrganizationRole.Owner && organization.Count(m => m.Role == OrganizationRole.Owner) == 1)
            {
                soleOwnerOf.Add(organization.Key.Name);
            }
        }

        soleOwnerOf.Sort(NameOrder);
        return new Memberships(soleOwnerOf, onlyMemberOf);
    }

    /// <summary>The user's organisations that matter for the deletion.</summary>
    /// <param name="SoleOwnerOf">Names of the organisations with other members whose only Owner the user is.</param>
    /// <param name="OnlyMemberOf">The organisations whose only member the user is.</param>
    private sealed record Memberships(IReadOnlyList<string> SoleOwnerOf, IReadOnlyList<Guid> OnlyMemberOf);
}
