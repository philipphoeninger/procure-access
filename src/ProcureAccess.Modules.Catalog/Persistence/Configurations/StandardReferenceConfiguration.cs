// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class StandardReferenceConfiguration : IEntityTypeConfiguration<StandardReference>
{
    public void Configure(EntityTypeBuilder<StandardReference> builder)
    {
        builder.ToTable("StandardReferences", CatalogDbContext.Schema);
        builder.ConfigureCatalogEntity();

        builder.Property(x => x.Standard).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Clause).HasMaxLength(StandardReference.ClauseMaxLength);
        builder.Property(x => x.Url).HasMaxLength(StandardReference.UrlMaxLength);
        builder.HasIndex(x => new { x.Standard, x.Clause }).IsUnique();
    }
}
