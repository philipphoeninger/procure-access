namespace MODELS.ProcureAccess.Entities.Configuration;

public class CriterionConfiguration : IEntityTypeConfiguration<Criterion>
{
    public void Configure(EntityTypeBuilder<Criterion> builder)
    {
        // properties
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GetDate()");
        builder.Property(x => x.Display)
            .HasComputedColumnSql("[Name]", stored: true);

        builder.HasIndex(
            x => new { x.Name }).IsUnique();

        // temporal
        builder.ToTable(b => b.IsTemporal(tb =>
        {
            tb.HasPeriodEnd("ValidTo");
            tb.HasPeriodStart("ValidFrom");
            tb.UseHistoryTable("CriteriaAudit");
        }));
    }
}