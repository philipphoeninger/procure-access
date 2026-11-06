// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class CriterionProcedureTextConfiguration : IEntityTypeConfiguration<CriterionProcedureText>
{
    /// <summary>At least one of the six rating levels and nothing else.</summary>
    public const string AllowedRatingsCheck = "CK_CriterionProcedureTexts_AllowedRatings";

    public void Configure(EntityTypeBuilder<CriterionProcedureText> builder)
    {
        builder.ToTable("CriterionProcedureTexts", CatalogDbContext.Schema, table =>
            table.HasCheckConstraint(
                AllowedRatingsCheck,
                $"""
                "AllowedRatings" > 0 AND ("AllowedRatings" & ~{(int)Rating.All}) = 0
                """));
        builder.ConfigureCatalogEntity();

        builder.Property(x => x.OriginalNumber).HasMaxLength(CriterionProcedureText.OriginalNumberMaxLength);
        builder.Property(x => x.OriginalTitleDe).HasMaxLength(CriterionProcedureText.OriginalTitleMaxLength);
        builder.Property(x => x.SourceUrl).HasMaxLength(CriterionProcedureText.SourceUrlMaxLength);

        // Flags as their number: a set does not fit a string conversion.
        builder.Property(x => x.AllowedRatings).HasConversion<int>();

        builder.HasOne(x => x.TestType).WithMany().HasForeignKey(x => x.TestTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.CriterionRevisionId, x.TestTypeId }).IsUnique();
    }
}
