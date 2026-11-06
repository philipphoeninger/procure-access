// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

/// <summary>Columns every <see cref="CatalogEntity"/> table has: key, creation time and <c>xmin</c>.</summary>
internal static class CatalogEntityConfiguration
{
    /// <param name="builder">The entity's builder.</param>
    /// <param name="generateIds">
    /// False for the seeded filter-value tables: only migrations write them, with fixed ids (O-S3-5 D1).
    /// </param>
    public static void ConfigureCatalogEntity<T>(this EntityTypeBuilder<T> builder, bool generateIds = true)
        where T : CatalogEntity
    {
        builder.HasKey(x => x.Id);
        if (generateIds)
        {
            builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        }
        else
        {
            builder.Property(x => x.Id).ValueGeneratedNever();
        }

        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz");

        // PostgreSQL system column xmin as optimistic concurrency token, mapped as in the legacy
        // BaseEntityConfiguration (Npgsql 10 has no UseXminAsConcurrencyToken any more).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion();
    }
}
