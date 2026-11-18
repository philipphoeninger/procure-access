// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Platform.Contracts.Persistence;

namespace ProcureAccess.Modules.Workspace.Persistence;

/// <summary>Applies the migrations of <see cref="WorkspaceDbContext"/> at startup (ADR-014).</summary>
internal sealed class WorkspaceDatabaseMigrator(WorkspaceDbContext db) : IModuleDatabaseMigrator
{
    public string ModuleName => "Workspace";

    public Task MigrateAsync(CancellationToken cancellationToken) => db.Database.MigrateAsync(cancellationToken);
}
