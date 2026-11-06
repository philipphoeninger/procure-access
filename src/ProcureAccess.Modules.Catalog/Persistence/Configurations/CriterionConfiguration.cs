// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence.Configurations;

internal sealed class CriterionConfiguration : IEntityTypeConfiguration<Criterion>
{
    public const string TestTypeOnlyForProcedureCheck = "CK_Criteria_TestTypeOnlyForProcedure";

    public void Configure(EntityTypeBuilder<Criterion> builder)
    {
        // A procedure criterion names its test type, a requirement never does (ARCHITECTURE §5.1, C30).
        builder.ToTable("Criteria", CatalogDbContext.Schema, table => table.HasCheckConstraint(
            TestTypeOnlyForProcedureCheck,
            $"(\"TestTypeId\" IS NOT NULL) = (\"Kind\" = '{nameof(CriterionKind.TestProcedure)}')"));
        builder.ConfigureCatalogEntity();

        builder.Property(x => x.Key).HasMaxLength(Criterion.KeyMaxLength);
        builder.HasIndex(x => x.Key).IsUnique();
        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Category).HasMaxLength(Criterion.CategoryMaxLength);
        builder.Property(x => x.SupersededByKey).HasMaxLength(Criterion.KeyMaxLength);
        builder.HasOne(x => x.TestType).WithMany().HasForeignKey(x => x.TestTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}
