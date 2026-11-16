// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Platform.Contracts.Persistence;

namespace ProcureAccess.Modules.Configurator.Persistence;

/// <summary>Applies the migrations of <see cref="ConfiguratorDbContext"/> at startup (ADR-014).</summary>
internal sealed class ConfiguratorDatabaseMigrator(ConfiguratorDbContext db) : IModuleDatabaseMigrator
{
    public string ModuleName => "Configurator";

    public Task MigrateAsync(CancellationToken cancellationToken) => db.Database.MigrateAsync(cancellationToken);
}
