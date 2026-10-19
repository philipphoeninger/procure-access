// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Configuration;

public class ProductConfiguration : BaseEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        // Query Filters
        builder.HasQueryFilter(x => !x.IsDeleted);

        // properties
        builder.HasIndex(
            x => new { x.Name }).IsUnique();
        builder.HasIndex(
            x => new { x.Link }).IsUnique();
        
        builder.Property(x => x.Description).HasColumnType("text");
        
        builder.HasMany(x => x.ProductCriteriaFilters)
            .WithOne(pcf => pcf.Product)
            .HasForeignKey(pcf => pcf.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Data
        builder.HasData(ProductSeed.Data);

        // temporal
        // builder.ToTable(b => b.IsTemporal(tb =>
        // {
        //     tb.HasPeriodEnd("ValidTo");
        //     tb.HasPeriodStart("ValidFrom");
        //     tb.UseHistoryTable("ProductsAudit");
        // }));
    }
}
