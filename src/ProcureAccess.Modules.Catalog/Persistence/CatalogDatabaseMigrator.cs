// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Platform.Contracts.Persistence;

namespace ProcureAccess.Modules.Catalog.Persistence;

/// <summary>Applies the migrations of <see cref="CatalogDbContext"/> at startup (ADR-014).</summary>
internal sealed class CatalogDatabaseMigrator(CatalogDbContext db) : IModuleDatabaseMigrator
{
    public string ModuleName => "Catalog";

    public Task MigrateAsync(CancellationToken cancellationToken) => db.Database.MigrateAsync(cancellationToken);
}
