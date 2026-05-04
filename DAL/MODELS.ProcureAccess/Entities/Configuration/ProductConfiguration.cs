namespace MODELS.ProcureAccess.Entities.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Query Filters
        builder.HasQueryFilter(x => !x.IsDeleted);

        // properties
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GetDate()");
        builder.Property(x => x.Display)
            .HasComputedColumnSql("[Name]", stored: true);

        builder.HasIndex(
            x => new { x.Name }).IsUnique();
        builder.HasIndex(
            x => new { x.Link }).IsUnique();
        
        builder.HasMany(x => x.ProductCriteriaFilters)
            .WithOne(pcf => pcf.Product)
            .HasForeignKey(pcf => pcf.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // temporal
        builder.ToTable(b => b.IsTemporal(tb =>
        {
            tb.HasPeriodEnd("ValidTo");
            tb.HasPeriodStart("ValidFrom");
            tb.UseHistoryTable("ProductsAudit");
        }));
    }
}
