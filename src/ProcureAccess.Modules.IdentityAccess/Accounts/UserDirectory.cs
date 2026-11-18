// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Accounts;

/// <inheritdoc cref="IUserDirectory"/>
internal sealed class UserDirectory(IdentityAccessDbContext db) : IUserDirectory
{
    public async Task<IReadOnlyDictionary<string, string>> GetDisplayNamesAsync(
        IReadOnlyCollection<string> userIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userIds);
        if (userIds.Count == 0)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        // Through the context's filter: a deleted account is not found, so it has no name.
        var ids = userIds.Distinct(StringComparer.Ordinal).ToList();
        return await db.Users.AsNoTracking()
                       .Where(u => ids.Contains(u.Id) && u.Email != null)
                       .ToDictionaryAsync(u => u.Id, u => u.Email!, StringComparer.Ordinal, cancellationToken);
    }
}
