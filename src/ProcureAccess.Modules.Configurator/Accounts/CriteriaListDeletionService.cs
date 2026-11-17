// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Configurator.Contracts.Accounts;
using ProcureAccess.Modules.Configurator.Persistence;

namespace ProcureAccess.Modules.Configurator.Accounts;

/// <inheritdoc cref="ICriteriaListDeletion"/>
internal sealed class CriteriaListDeletionService(ConfiguratorDbContext db) : ICriteriaListDeletion
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
