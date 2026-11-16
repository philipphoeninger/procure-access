// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Platform.Analytics;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Modules.Platform.Logging;
using ProcureAccess.Modules.Platform.Persistence.Configurations;
using ProcureAccess.Modules.Platform.Sharing;

namespace ProcureAccess.Modules.Platform.Persistence;

/// <summary>
/// The Platform module's own database context (ARCHITECTURE ADR-014): the log table
/// (<c>logging</c> schema), the usage events (<c>analytics</c> schema) and, since S5-23, the share
/// links (<c>sharing</c> schema). No other module uses it.
/// </summary>
/// <remarks>
/// Its migrations have a history table of their own, <c>platform."__EFMigrationsHistory"</c>,
/// apart from the legacy <c>ApplicationDBContext</c>'s <c>public."__EFMigrationsHistory"</c>.
/// A context built outside a request (design-time factory, usage-event store, tests) passes
/// <see cref="FixedOrganizationContext.None"/>: it sees no share link.
/// </remarks>
internal sealed class PlatformDbContext(DbContextOptions<PlatformDbContext> options, IOrganizationContext organizations)
    : DbContext(options)
{
    // A field, not the primary-constructor parameter: EF turns a member chain rooted at the context
    // instance into a query parameter, so the filter is evaluated per query, never cached in the model.
    private readonly IOrganizationContext _organizations = organizations;

    public const string HistorySchema = "platform";
    public const string HistoryTable = "__EFMigrationsHistory";

    public DbSet<SeriLogEntry> SeriLogEntries => Set<SeriLogEntry>();
    public DbSet<UsageEvent> UsageEvents => Set<UsageEvent>();

    /// <summary>The share links; filtered to the organisations of the request (TEN-01, TEN-08).</summary>
    public DbSet<ShareLink> ShareLinks => Set<ShareLink>();

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
        modelBuilder.ApplyConfiguration(new ShareLinkConfiguration());

        // The organisation filter (ADR-007, TEN-01): a link of an organisation the request's user
        // is no member of does not exist for this context.
        modelBuilder.Entity<ShareLink>()
                    .HasQueryFilter(l => _organizations.OrganizationIds.Contains(l.OrganizationId));
    }
}
