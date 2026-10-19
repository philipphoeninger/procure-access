// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Configuration;

public class FilterTypeConfiguration : BaseEntityConfiguration<FilterType>
{
    public override void Configure(EntityTypeBuilder<FilterType> builder)
    {
        base.Configure(builder);

        // Query Filters
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Properties
        builder.HasIndex(
            x => new { x.Name }).IsUnique();

        builder.Property(x => x.Description).HasColumnType("text");
        
        // Data
        builder.HasData(FilterTypeSeed.Data);
        
        // temporal
        // builder.ToTable(b => b.IsTemporal(tb =>
        // {
        //     tb.HasPeriodEnd("ValidTo");
        //     tb.HasPeriodStart("ValidFrom");
        //     tb.UseHistoryTable("FilterTypesAudit");
        // }));
    }
}