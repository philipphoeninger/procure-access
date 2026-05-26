namespace MODELS.ProcureAccess.Entities.Configuration;

public class CriteriaFilterConfiguration : BaseEntityConfiguration<CriteriaFilter>
{
    public void Configure(EntityTypeBuilder<CriteriaFilter> builder)
    {
        base.Configure(builder);
        
        // Query Filters
        builder.HasQueryFilter(x => !x.FilterType.IsDeleted);

        // properties
        builder.HasIndex(
            c => new { c.Name }).IsUnique();
        
        builder.Property(x => x.Description).HasColumnType("text");

        builder
            .HasOne(x => x.FilterType)
            .WithMany(ft => ft.CriteriaFilters)
            .HasForeignKey(x => x.FilterTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // temporal
        // builder.ToTable(b => b.IsTemporal(tb =>
        // {
        //     tb.HasPeriodEnd("ValidTo");
        //     tb.HasPeriodStart("ValidFrom");
        //     tb.UseHistoryTable("CriteriaFiltersAudit");
        // }));
    }
}