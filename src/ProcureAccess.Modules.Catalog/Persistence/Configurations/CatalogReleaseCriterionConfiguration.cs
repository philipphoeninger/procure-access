// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class CatalogReleaseCriterionConfiguration : IEntityTypeConfiguration<CatalogReleaseCriterion>
{
    public void Configure(EntityTypeBuilder<CatalogReleaseCriterion> builder)
    {
        builder.ToTable("CatalogReleaseCriteria", CatalogDbContext.Schema);
        builder.HasKey(x => new { x.CatalogReleaseId, x.CriterionRevisionId });

        // A revision is shared by several releases (CAT-07); deleting one never removes the other's rows.
        builder.HasOne(x => x.Revision)
            .WithMany()
            .HasForeignKey(x => x.CriterionRevisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
