namespace MODELS.ProcureAccess.Entities.Configuration;

public class CriteriaFilterConfiguration : IEntityTypeConfiguration<CriteriaFilter>
{
    public void Configure(EntityTypeBuilder<CriteriaFilter> builder)
    {
        // properties
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("GetDate()");
        builder.Property(c => c.Display)
            .HasComputedColumnSql("[Name]", stored: true);

        builder.HasIndex(
            c => new { c.Name }).IsUnique();

        builder.HasOne(c => c.ProductPart)
            .WithMany(p => p.Parts)
            .HasForeignKey(c => c.ProductIdPart);
        
        builder.HasOne(c => c.ProductTest)
            .WithMany(p => p.Tests)
            .HasForeignKey(c => c.ProductIdTest);

        // temporal
        builder.ToTable(b => b.IsTemporal(tb =>
        {
            tb.HasPeriodEnd("ValidTo");
            tb.HasPeriodStart("ValidFrom");
            tb.UseHistoryTable("CriteriaFiltersAudit");
        }));
    }
}