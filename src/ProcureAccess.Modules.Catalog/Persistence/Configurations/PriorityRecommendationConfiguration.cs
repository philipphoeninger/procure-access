// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class PriorityRecommendationConfiguration : IEntityTypeConfiguration<PriorityRecommendation>
{
    public void Configure(EntityTypeBuilder<PriorityRecommendation> builder)
    {
        builder.ToTable("PriorityRecommendations", CatalogDbContext.Schema);
        builder.ConfigureCatalogEntity();

        builder.Property(x => x.Level).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Source).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.RationaleDe).HasMaxLength(PriorityRecommendation.RationaleMaxLength);

        builder.HasOne(x => x.AppType).WithMany().HasForeignKey(x => x.AppTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UseCase).WithMany().HasForeignKey(x => x.UseCaseTemplateId).OnDelete(DeleteBehavior.Restrict);

        // At most one rule per level and value pair, so resolution is deterministic (CAT-01).
        builder.HasIndex(x => new { x.CriterionRevisionId, x.AppTypeId, x.UseCaseTemplateId })
            .IsUnique()
            .AreNullsDistinct(false);
    }
}
