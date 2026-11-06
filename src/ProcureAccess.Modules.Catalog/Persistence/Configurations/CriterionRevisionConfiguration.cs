// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class CriterionRevisionConfiguration : IEntityTypeConfiguration<CriterionRevision>
{
    public void Configure(EntityTypeBuilder<CriterionRevision> builder)
    {
        builder.ToTable("CriterionRevisions", CatalogDbContext.Schema);
        builder.ConfigureCatalogEntity();

        builder.HasOne(x => x.Criterion)
            .WithMany()
            .HasForeignKey(x => x.CriterionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.CriterionId, x.RevisionNumber }).IsUnique();

        builder.Ignore(x => x.IsSealed);

        // m:n: a reference is shared by every revision that cites it (ARCHITECTURE §5.1).
        builder.HasMany(x => x.StandardReferences)
            .WithMany()
            .UsingEntity(
                "CriterionRevisionStandardReferences",
                r => r.HasOne(typeof(StandardReference)).WithMany().HasForeignKey("StandardReferenceId").OnDelete(DeleteBehavior.Restrict),
                l => l.HasOne(typeof(CriterionRevision)).WithMany().HasForeignKey("CriterionRevisionId").OnDelete(DeleteBehavior.Restrict),
                j =>
                {
                    j.ToTable("CriterionRevisionStandardReferences", CatalogDbContext.Schema);
                    j.HasKey("CriterionRevisionId", "StandardReferenceId");
                });
        builder.Navigation(x => x.StandardReferences).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Rows the revision owns (S3-13); fixed with it once a published release contains it (CAT-05).
        builder.HasMany(x => x.Applicability)
            .WithOne(x => x.Revision)
            .HasForeignKey(x => x.CriterionRevisionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(x => x.Applicability).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Priorities)
            .WithOne(x => x.Revision)
            .HasForeignKey(x => x.CriterionRevisionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(x => x.Priorities).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.ProcedureTexts)
            .WithOne(x => x.Revision)
            .HasForeignKey(x => x.CriterionRevisionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(x => x.ProcedureTexts).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
