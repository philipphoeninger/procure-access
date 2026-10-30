// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Platform.Contracts.Persistence;

namespace ProcureAccess.Modules.Platform.Persistence;

/// <summary>Applies the migrations of <see cref="PlatformDbContext"/> at startup (ADR-014).</summary>
internal sealed class PlatformDatabaseMigrator(PlatformDbContext db) : IModuleDatabaseMigrator
{
    public string ModuleName => "Platform";

    public Task MigrateAsync(CancellationToken cancellationToken) => db.Database.MigrateAsync(cancellationToken);
}
