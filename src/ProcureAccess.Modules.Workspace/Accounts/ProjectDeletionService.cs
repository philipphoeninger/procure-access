// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.Workspace.Persistence;

namespace ProcureAccess.Modules.Workspace.Accounts;

/// <summary>
/// The Workspace part of the account deletion (<see cref="IOrganizationDataDeletion"/>; SPEC
/// R-AUTH-10; TEN-11; S6-25): the procurement projects of the organisations that end, with
/// everything below them.
/// </summary>
/// <remarks>
/// Only the projects are deleted here; the database deletes what belongs to them through its
/// cascading foreign keys: candidates, their private sections, assessments, test evidences and
/// their private notes. A table added below a project or a candidate needs a cascading foreign key too, or its
/// own delete here. A project's copy of a criteria list is a list of the organisation and goes
/// with the Configurator's part.
/// </remarks>
internal sealed class ProjectDeletionService(WorkspaceDbContext db) : IOrganizationDataDeletion
{
    public async Task DeleteOfOrganizationsAsync(IReadOnlyCollection<Guid> organizationIds, CancellationToken cancellationToken)
    {
        if (organizationIds.Count == 0)
        {
            return;
        }

        // Through the organisation filter of the request (TEN-01).
        await db.ProcurementProjects
                .Where(p => organizationIds.Contains(p.OrganizationId))
                .ExecuteDeleteAsync(cancellationToken);
    }
}
