namespace MODELS.ProcureAccess.Entities.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // properties
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("GetDate()");
        builder.Property(p => p.Display)
            .HasComputedColumnSql("[Name]", stored: true);

        builder.HasIndex(
            p => new { p.Name }).IsUnique();
        builder.HasIndex(
            p => new { p.Link }).IsUnique();

        // temporal
        builder.ToTable(b => b.IsTemporal(tb =>
        {
            tb.HasPeriodEnd("ValidTo");
            tb.HasPeriodStart("ValidFrom");
            tb.UseHistoryTable("ProductsAudit");
        }));
    }
}
