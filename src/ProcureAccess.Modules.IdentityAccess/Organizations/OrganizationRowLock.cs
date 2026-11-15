// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Organizations;

/// <summary>
/// The row lock of an organisation (S5-16a, S5-17a): every change of its memberships and open
/// invitations — role change, removal, leaving, inviting, revoking, accepting — takes it inside its
/// transaction, so the last-Owner check (TEN-11), the one open invitation per address and the single
/// acceptance of a link (SEC-11) never race. Raw SQL that reads nothing, so it passes the
/// organisation filter: the callers take it only after a filtered read found the organisation, or —
/// for accepting — after the link's token found the invitation (TEN-02 allow-list).
/// </summary>
internal static class OrganizationRowLock
{
    /// <summary>Locks the row of <paramref name="organizationId"/> until the current transaction ends.</summary>
    public static async Task LockAsync(IdentityAccessDbContext db, Guid organizationId, CancellationToken cancellationToken)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The organisation's row lock needs a transaction.");
        }

        await db.Database.ExecuteSqlAsync(
            $"""SELECT 1 FROM identity."Organizations" WHERE "Id" = {organizationId} FOR UPDATE""", cancellationToken);
    }
}
