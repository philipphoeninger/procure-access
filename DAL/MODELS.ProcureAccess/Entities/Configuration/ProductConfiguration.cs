namespace MODELS.ProcureAccess.Entities.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasQueryFilter(x => !x.ToApprove);

        // properties
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GetDate()");
        builder.Property(x => x.Display)
            .HasComputedColumnSql("[Name]", stored: true);

        builder.HasIndex(
            x => new { x.Name }).IsUnique();
        builder.HasIndex(
            x => new { x.Link }).IsUnique();

        
        builder.HasMany(x => x.Types)
            // .WithMany(cf => cf.ProductTypes);
            .WithOne(pp => pp.Product)
            .HasForeignKey(pp => pp.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Parts)
            // .WithMany(cf => cf.ProductParts);
            .WithOne(pp => pp.Product)
            .HasForeignKey(pp => pp.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(x => x.Tests)
            // .WithMany(p => p.ProductTests);
            .WithOne(pt => pt.Product)
            .HasForeignKey(pt => pt.ProductId)
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
