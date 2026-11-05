// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class CriterionConfiguration : IEntityTypeConfiguration<Criterion>
{
    public void Configure(EntityTypeBuilder<Criterion> builder)
    {
        builder.ToTable("Criteria", CatalogDbContext.Schema);
        builder.ConfigureCatalogEntity();

        builder.Property(x => x.Key).HasMaxLength(Criterion.KeyMaxLength);
        builder.HasIndex(x => x.Key).IsUnique();
        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Category).HasMaxLength(Criterion.CategoryMaxLength);
        builder.Property(x => x.SupersededByKey).HasMaxLength(Criterion.KeyMaxLength);
    }
}
