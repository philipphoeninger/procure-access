namespace MODELS.ProcureAccess.Entities.Configuration;

public class FilterTypeConfiguration : BaseEntityConfiguration<FilterType>
{
    public void Configure(EntityTypeBuilder<FilterType> builder)
    {
        base.Configure(builder);

        // Query Filters
        builder.HasQueryFilter(x => !x.IsDeleted);

        // properties
        builder.HasIndex(
            x => new { x.Name }).IsUnique();

        builder.Property(x => x.Description).HasColumnType("text");
        
        // temporal
        // builder.ToTable(b => b.IsTemporal(tb =>
        // {
        //     tb.HasPeriodEnd("ValidTo");
        //     tb.HasPeriodStart("ValidFrom");
        //     tb.UseHistoryTable("FilterTypesAudit");
        // }));
    }
}