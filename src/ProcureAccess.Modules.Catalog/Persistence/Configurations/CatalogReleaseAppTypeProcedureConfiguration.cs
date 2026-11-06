// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class CatalogReleaseAppTypeProcedureConfiguration : IEntityTypeConfiguration<CatalogReleaseAppTypeProcedure>
{
    public void Configure(EntityTypeBuilder<CatalogReleaseAppTypeProcedure> builder)
    {
        builder.ToTable("CatalogReleaseAppTypeProcedures", CatalogDbContext.Schema);
        builder.HasKey(x => new { x.CatalogReleaseId, x.AppTypeId, x.TestTypeId });

        builder.HasOne(x => x.AppType).WithMany().HasForeignKey(x => x.AppTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TestType).WithMany().HasForeignKey(x => x.TestTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}
