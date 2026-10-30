// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.Persistence;

/// <summary>
/// Applies the pending migrations of one module's own <c>DbContext</c> (ARCHITECTURE ADR-014).
/// Every module that owns tables registers one. At startup the host first migrates the legacy
/// <c>ApplicationDBContext</c> and then runs every registered migrator in registration order.
/// </summary>
public interface IModuleDatabaseMigrator
{
    /// <summary>Name of the module, for the startup log.</summary>
    string ModuleName { get; }

    /// <summary>Applies all pending migrations of the module's context; creates nothing else.</summary>
    Task MigrateAsync(CancellationToken cancellationToken);
}
