// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Platform.Analytics;
using ProcureAccess.Modules.Platform.Logging;
using ProcureAccess.Modules.Platform.Persistence.Configurations;

namespace ProcureAccess.Modules.Platform.Persistence;

/// <summary>
/// The Platform module's own database context (ARCHITECTURE ADR-014): the log table
/// (<c>logging</c> schema) and the usage events (<c>analytics</c> schema). No other module uses it.
/// </summary>
/// <remarks>
/// Its migrations have a history table of their own, <c>platform."__EFMigrationsHistory"</c>,
/// apart from the legacy <c>ApplicationDBContext</c>'s <c>public."__EFMigrationsHistory"</c>.
/// </remarks>
internal sealed class PlatformDbContext(DbContextOptions<PlatformDbContext> options) : DbContext(options)
{
    public const string HistorySchema = "platform";
    public const string HistoryTable = "__EFMigrationsHistory";

    public DbSet<SeriLogEntry> SeriLogEntries => Set<SeriLogEntry>();
    public DbSet<UsageEvent> UsageEvents => Set<UsageEvent>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Set here and not at the registration, so that every instance (application, design-time
        // tools, tests) uses the module's history table. With the default it would share the
        // legacy context's table and see its migrations as unknown.
        optionsBuilder.UseNpgsql(npgsql => npgsql.MigrationsHistoryTable(HistoryTable, HistorySchema));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SeriLogEntryConfiguration());
        modelBuilder.ApplyConfiguration(new UsageEventConfiguration());
    }
}
