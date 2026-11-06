// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence.Seed;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class ProductPartConfiguration : IEntityTypeConfiguration<ProductPart>
{
    public void Configure(EntityTypeBuilder<ProductPart> builder)
    {
        builder.ConfigureFilterValue("ProductParts");
        builder.Property(x => x.ExplanationDe).HasMaxLength(ProductPart.ExplanationMaxLength);
        builder.HasData(FilterValueSeed.ProductParts);
    }
}
