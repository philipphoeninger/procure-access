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

    /// <summary>One row per source, target and reason (S3-16); named, because the generated name is too long.</summary>
    public const string UniqueRuleIndex = "IX_FilterExclusions_Rule";

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

        // One row per rule: the import reuses it in every release that applies it (S3-13 P1, S3-16).
        // A changed reason is a new rule, so the reason is part of the key; the unused source and
        // target columns are null and count as one value.
        builder.HasIndex(x => new
        {
            x.SourceUseCaseId,
            x.SourceAppTypeId,
            x.SourceProductPartId,
            x.TargetAppTypeId,
            x.TargetProductPartId,
            x.TargetTestTypeId,
            x.ReasonDe,
        })
            .IsUnique()
            .AreNullsDistinct(false)
            .HasDatabaseName(UniqueRuleIndex);
    }
}
