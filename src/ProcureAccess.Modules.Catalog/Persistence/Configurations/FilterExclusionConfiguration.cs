// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class FilterExclusionConfiguration : IEntityTypeConfiguration<FilterExclusion>
{
    /// <summary>Exactly one source and exactly one target (ARCHITECTURE §5.1, Y5).</summary>
    public const string OneSourceOneTargetCheck = "CK_FilterExclusions_OneSourceOneTarget";

    /// <summary>
    /// The target lies in a lower layer than the source (CAT-13). With one source and one target,
    /// the pairs left to refuse are app type → app type and product part → app type or product part.
    /// </summary>
    public const string TargetLayerLowerCheck = "CK_FilterExclusions_TargetLayerLower";

    public void Configure(EntityTypeBuilder<FilterExclusion> builder)
    {
        builder.ToTable("FilterExclusions", CatalogDbContext.Schema, table =>
        {
            table.HasCheckConstraint(
                OneSourceOneTargetCheck,
                """
                num_nonnulls("SourceUseCaseId", "SourceAppTypeId", "SourceProductPartId") = 1
                AND num_nonnulls("TargetAppTypeId", "TargetProductPartId", "TargetTestTypeId") = 1
                """);
            table.HasCheckConstraint(
                TargetLayerLowerCheck,
                """
                NOT ("SourceAppTypeId" IS NOT NULL AND "TargetAppTypeId" IS NOT NULL)
                AND NOT ("SourceProductPartId" IS NOT NULL AND ("TargetAppTypeId" IS NOT NULL OR "TargetProductPartId" IS NOT NULL))
                """);
        });
        builder.ConfigureCatalogEntity();

        builder.Property(x => x.ReasonDe).HasMaxLength(FilterExclusion.ReasonMaxLength);
        builder.Ignore(x => x.Source);
        builder.Ignore(x => x.Target);

        builder.HasOne(x => x.SourceUseCase).WithMany().HasForeignKey(x => x.SourceUseCaseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SourceAppType).WithMany().HasForeignKey(x => x.SourceAppTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SourceProductPart).WithMany().HasForeignKey(x => x.SourceProductPartId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TargetAppType).WithMany().HasForeignKey(x => x.TargetAppTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TargetProductPart).WithMany().HasForeignKey(x => x.TargetProductPartId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TargetTestType).WithMany().HasForeignKey(x => x.TargetTestTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}
