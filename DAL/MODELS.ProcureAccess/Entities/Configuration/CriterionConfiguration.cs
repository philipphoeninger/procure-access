namespace MODELS.ProcureAccess.Entities.Configuration;

public class CriterionConfiguration : BaseEntityConfiguration<Criterion>
{
    public void Configure(EntityTypeBuilder<Criterion> builder)
    {
        base.Configure(builder);

        // Query Filters
        builder.HasQueryFilter(x => !x.IsDeleted);
        
        // properties
        builder.HasIndex(
            x => new { x.Name }).IsUnique();

        builder.Property(x => x.Description).HasColumnType("text");
        
        builder
            .HasOne(x => x.CriteriaFilter)
            .WithMany(cf => cf.Criteria)
            .HasForeignKey(x => x.CriteriaFilterId)
            .OnDelete(DeleteBehavior.Restrict);

        // temporal
        // builder.ToTable(b => b.IsTemporal(tb =>
        // {
        //     tb.HasPeriodEnd("ValidTo");
        //     tb.HasPeriodStart("ValidFrom");
        //     tb.UseHistoryTable("CriteriaAudit");
        // }));
    }
}