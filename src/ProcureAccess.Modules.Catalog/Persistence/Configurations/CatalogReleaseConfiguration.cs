// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class CatalogReleaseConfiguration : IEntityTypeConfiguration<CatalogRelease>
{
    public void Configure(EntityTypeBuilder<CatalogRelease> builder)
    {
        builder.ToTable("CatalogReleases", CatalogDbContext.Schema);
        builder.ConfigureCatalogEntity();

        builder.Property(x => x.Version).HasMaxLength(CatalogRelease.VersionMaxLength);
        builder.HasIndex(x => x.Version).IsUnique();

        // Only published releases are stored (owner, S3-12 decision 1).
        builder.Property(x => x.PublishedAt).HasColumnType("timestamptz").IsRequired();

        builder.Ignore(x => x.IsPublished);
        builder.HasMany(x => x.Criteria)
            .WithOne(x => x.Release)
            .HasForeignKey(x => x.CatalogReleaseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(x => x.Criteria).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Exclusions)
            .WithOne(x => x.Release)
            .HasForeignKey(x => x.CatalogReleaseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(x => x.Exclusions).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Procedures)
            .WithOne(x => x.Release)
            .HasForeignKey(x => x.CatalogReleaseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(x => x.Procedures).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
