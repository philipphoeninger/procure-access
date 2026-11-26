// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Persistence;

namespace ProcureAccess.Modules.Workspace.Projects;

/// <inheritdoc cref="IProjectStatusReader"/>
internal sealed class ProjectStatusReader(WorkspaceDbContext db) : IProjectStatusReader
{
    public async Task<bool> IsArchivedAsync(Guid projectId, CancellationToken cancellationToken) =>
        await db.ProcurementProjects.AnyAsync(p => p.Id == projectId && p.Status == ProjectStatus.Archived, cancellationToken);
}
