// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class CriterionApplicabilityConfiguration : IEntityTypeConfiguration<CriterionApplicability>
{
    public void Configure(EntityTypeBuilder<CriterionApplicability> builder)
    {
        builder.ToTable("CriterionApplicability", CatalogDbContext.Schema);
        builder.ConfigureCatalogEntity();

        builder.HasOne(x => x.AppType).WithMany().HasForeignKey(x => x.AppTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ProductPart).WithMany().HasForeignKey(x => x.ProductPartId).OnDelete(DeleteBehavior.Restrict);

        // One row per app type and product part; „no product part“ counts as a value of its own.
        builder.HasIndex(x => new { x.CriterionRevisionId, x.AppTypeId, x.ProductPartId })
            .IsUnique()
            .AreNullsDistinct(false);
    }
}
