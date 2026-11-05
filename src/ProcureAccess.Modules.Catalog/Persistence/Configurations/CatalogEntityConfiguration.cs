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
    public static void ConfigureCatalogEntity<T>(this EntityTypeBuilder<T> builder)
        where T : CatalogEntity
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamptz");

        // PostgreSQL system column xmin as optimistic concurrency token, mapped as in the legacy
        // BaseEntityConfiguration (Npgsql 10 has no UseXminAsConcurrencyToken any more).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion();
    }
}
