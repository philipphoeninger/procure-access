// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Platform.Contracts.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Persistence;

/// <summary>Applies the migrations of <see cref="IdentityAccessDbContext"/> at startup (ADR-014).</summary>
internal sealed class IdentityAccessDatabaseMigrator(IdentityAccessDbContext db) : IModuleDatabaseMigrator
{
    public string ModuleName => "IdentityAccess";

    public Task MigrateAsync(CancellationToken cancellationToken) => db.Database.MigrateAsync(cancellationToken);
}
