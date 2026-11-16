// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.Configurator.Persistence.Configurations;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;

namespace ProcureAccess.Modules.Configurator.Persistence;

/// <summary>
/// The Configurator module's own database context (ARCHITECTURE ADR-014) in the
/// <c>configurator</c> schema: the saved criteria lists with their selection and decisions (§5.3;
/// S5-19). No other module and no legacy code uses it; other modules read lists through
/// <c>IConfigurationReader</c>.
/// </summary>
/// <remarks>
/// Its history table is <c>configurator."__EFMigrationsHistory"</c>. The lists carry the global
/// query filter of ADR-007, bound to <paramref name="organizations"/> (TEN-01): the context sees
/// only the lists of the request's organisations. The rows of a list are owned by it and have no
/// set of their own, so they are reached only through a filtered list. A context built outside a
/// request (design-time factory, fixtures) gets <see cref="FixedOrganizationContext.None"/>.
/// </remarks>
internal sealed class ConfiguratorDbContext(
    DbContextOptions<ConfiguratorDbContext> options, IOrganizationContext organizations)
    : DbContext(options)
{
    // A field, not the primary-constructor parameter: EF turns a member chain rooted at the context
    // instance into a query parameter, so the filter is evaluated per query, never cached in the model.
    private readonly IOrganizationContext _organizations = organizations;

    public const string Schema = "configurator";
    public const string HistoryTable = "__EFMigrationsHistory";

    /// <summary>The saved lists; filtered to the organisations of the request (TEN-01).</summary>
    public DbSet<CriteriaConfiguration> CriteriaConfigurations => Set<CriteriaConfiguration>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // As in the other module contexts: set here, so every instance (application, design-time
        // tools, tests) uses the module's history table.
        optionsBuilder.UseNpgsql(npgsql => npgsql.MigrationsHistoryTable(HistoryTable, Schema));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new CriteriaConfigurationConfiguration());

        // The organisation filter (ADR-007, TEN-01): a list of an organisation the request's user
        // is no member of does not exist for this context.
        modelBuilder.Entity<CriteriaConfiguration>()
                    .HasQueryFilter(c => _organizations.OrganizationIds.Contains(c.OrganizationId));
    }
}
