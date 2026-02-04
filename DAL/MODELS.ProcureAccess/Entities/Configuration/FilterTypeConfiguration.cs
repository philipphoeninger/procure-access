namespace MODELS.ProcureAccess.Entities.Configuration;

public class FilterTypeConfiguration : IEntityTypeConfiguration<FilterType>
{
    public void Configure(EntityTypeBuilder<FilterType> builder)
    {
        // properties
        builder.Property(f => f.CreatedAt).HasDefaultValueSql("GetDate()");
        builder.Property(f => f.Display)
            .HasComputedColumnSql("[Name]", stored: true);

        builder.HasIndex(
            f => new { f.Name }).IsUnique();

        // temporal
        builder.ToTable(b => b.IsTemporal(tb =>
        {
            tb.HasPeriodEnd("ValidTo");
            tb.HasPeriodStart("ValidFrom");
            tb.UseHistoryTable("FilterTypesAudit");
        }));
    }
}