// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Persistence.Configurations;

namespace ProcureAccess.Modules.Workspace.Persistence;

/// <summary>
/// The Workspace module's own database context (ARCHITECTURE ADR-014) in the <c>workspace</c>
/// schema: the procurement projects (§5.4; S6-11). No other module and no legacy code uses it.
/// </summary>
/// <remarks>
/// Its history table is <c>workspace."__EFMigrationsHistory"</c>. The projects carry the global
/// query filter of ADR-007, bound to <paramref name="organizations"/> (TEN-01): the context sees
/// only the projects of the request's organisations. A context built outside a request
/// (design-time factory, fixtures) gets <see cref="FixedOrganizationContext.None"/>.
/// </remarks>
internal sealed class WorkspaceDbContext(
    DbContextOptions<WorkspaceDbContext> options, IOrganizationContext organizations)
    : DbContext(options)
{
    // A field, not the primary-constructor parameter: EF turns a member chain rooted at the context
    // instance into a query parameter, so the filter is evaluated per query, never cached in the model.
    private readonly IOrganizationContext _organizations = organizations;

    public const string Schema = "workspace";
    public const string HistoryTable = "__EFMigrationsHistory";

    /// <summary>The projects; filtered to the organisations of the request (TEN-01).</summary>
    public DbSet<ProcurementProject> ProcurementProjects => Set<ProcurementProject>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // As in the other module contexts: set here, so every instance (application, design-time
        // tools, tests) uses the module's history table.
        optionsBuilder.UseNpgsql(npgsql => npgsql.MigrationsHistoryTable(HistoryTable, Schema));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new ProcurementProjectConfiguration());

        // The organisation filter (ADR-007, TEN-01): a project of an organisation the request's
        // user is no member of does not exist for this context.
        modelBuilder.Entity<ProcurementProject>()
                    .HasQueryFilter(p => _organizations.OrganizationIds.Contains(p.OrganizationId));
    }
}
