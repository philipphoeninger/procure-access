// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence.Configurations;

namespace ProcureAccess.Modules.Catalog.Persistence;

/// <summary>
/// The Catalog module's own database context (ARCHITECTURE ADR-014): releases, criteria, revisions,
/// standard references, the four filter layers and their rules in the <c>catalog</c> schema. No other module and no legacy code uses it;
/// no foreign key or join leads to another schema (rule 5).
/// </summary>
/// <remarks>
/// Its history table is <c>catalog."__EFMigrationsHistory"</c>. <see cref="TimeProvider"/> sets
/// <see cref="CatalogEntity.CreatedAt"/> of every added row (CLAUDE.md §7: time only through
/// <c>TimeProvider</c>).
/// </remarks>
internal sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options, TimeProvider timeProvider)
    : DbContext(options)
{
    public const string Schema = "catalog";
    public const string HistoryTable = "__EFMigrationsHistory";

    public DbSet<CatalogRelease> Releases => Set<CatalogRelease>();
    public DbSet<Criterion> Criteria => Set<Criterion>();
    public DbSet<CriterionRevision> CriterionRevisions => Set<CriterionRevision>();
    public DbSet<StandardReference> StandardReferences => Set<StandardReference>();
    public DbSet<UseCaseTemplate> UseCaseTemplates => Set<UseCaseTemplate>();
    public DbSet<AppType> AppTypes => Set<AppType>();
    public DbSet<ProductPart> ProductParts => Set<ProductPart>();
    public DbSet<TestType> TestTypes => Set<TestType>();
    public DbSet<FilterExclusion> FilterExclusions => Set<FilterExclusion>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetCreatedAt();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetCreatedAt();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // As in PlatformDbContext: set here, so every instance (application, design-time tools,
        // tests) uses the module's history table.
        optionsBuilder.UseNpgsql(npgsql => npgsql.MigrationsHistoryTable(HistoryTable, Schema));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new CatalogReleaseConfiguration());
        modelBuilder.ApplyConfiguration(new CatalogReleaseCriterionConfiguration());
        modelBuilder.ApplyConfiguration(new CriterionConfiguration());
        modelBuilder.ApplyConfiguration(new CriterionRevisionConfiguration());
        modelBuilder.ApplyConfiguration(new StandardReferenceConfiguration());
        modelBuilder.ApplyConfiguration(new UseCaseTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new AppTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ProductPartConfiguration());
        modelBuilder.ApplyConfiguration(new TestTypeConfiguration());
        modelBuilder.ApplyConfiguration(new FilterExclusionConfiguration());
        modelBuilder.ApplyConfiguration(new CatalogReleaseFilterExclusionConfiguration());
        modelBuilder.ApplyConfiguration(new CatalogReleaseAppTypeProcedureConfiguration());
        modelBuilder.ApplyConfiguration(new CriterionApplicabilityConfiguration());
        modelBuilder.ApplyConfiguration(new PriorityRecommendationConfiguration());
        modelBuilder.ApplyConfiguration(new CriterionProcedureTextConfiguration());
    }

    private void SetCreatedAt()
    {
        var now = timeProvider.GetUtcNow();
        foreach (var entry in ChangeTracker.Entries<CatalogEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(x => x.CreatedAt).CurrentValue = now;
            }
        }
    }
}
