// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class CatalogReleaseFilterExclusionConfiguration : IEntityTypeConfiguration<CatalogReleaseFilterExclusion>
{
    public void Configure(EntityTypeBuilder<CatalogReleaseFilterExclusion> builder)
    {
        builder.ToTable("CatalogReleaseFilterExclusions", CatalogDbContext.Schema);
        builder.HasKey(x => new { x.CatalogReleaseId, x.FilterExclusionId });

        // A rule is shared by several releases (CAT-19), as revisions are.
        builder.HasOne(x => x.Exclusion)
            .WithMany()
            .HasForeignKey(x => x.FilterExclusionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
