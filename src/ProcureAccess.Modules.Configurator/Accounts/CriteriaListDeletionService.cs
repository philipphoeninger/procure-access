// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Configurator.Persistence;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

namespace ProcureAccess.Modules.Configurator.Accounts;

/// <summary>
/// The Configurator part of the account deletion (<see cref="IOrganizationDataDeletion"/>): the
/// criteria lists of the organisations that end, with their rows (cascade).
/// </summary>
internal sealed class CriteriaListDeletionService(ConfiguratorDbContext db) : IOrganizationDataDeletion
{
    public async Task DeleteOfOrganizationsAsync(IReadOnlyCollection<Guid> organizationIds, CancellationToken cancellationToken)
    {
        if (organizationIds.Count == 0)
        {
            return;
        }

        // The rows of a list (selection values, criteria) go with it: their foreign keys cascade.
        await db.CriteriaConfigurations
                .Where(c => organizationIds.Contains(c.OrganizationId))
                .ExecuteDeleteAsync(cancellationToken);
    }
}
