// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence.Seed;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

/// <summary>
/// The four filter-value tables (ARCHITECTURE §5.1): fixed ids, unique key, rows from
/// <see cref="FilterValueSeed"/> (O-S3-5 D1).
/// </summary>
internal static class FilterValueConfiguration
{
    public static void ConfigureFilterValue<T>(this EntityTypeBuilder<T> builder, string table)
        where T : FilterValue
    {
        builder.ToTable(table, CatalogDbContext.Schema);
        builder.ConfigureCatalogEntity(generateIds: false);
        builder.Property(x => x.Key).HasMaxLength(FilterValue.KeyMaxLength);
        builder.HasIndex(x => x.Key).IsUnique();
        builder.Property(x => x.NameDe).HasMaxLength(FilterValue.NameMaxLength);
        builder.Ignore(x => x.Layer);
    }
}
