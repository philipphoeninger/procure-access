namespace MODELS.ProcureAccess.Entities.Configuration;

public class CriterionConfiguration : IEntityTypeConfiguration<Criterion>
{
    public void Configure(EntityTypeBuilder<Criterion> builder)
    {
        // properties
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("GetDate()");
        builder.Property(c => c.Display)
            .HasComputedColumnSql("[Name]", stored: true);

        builder.HasIndex(
            c => new { c.Name }).IsUnique();

        // temporal
        builder.ToTable(b => b.IsTemporal(tb =>
        {
            tb.HasPeriodEnd("ValidTo");
            tb.HasPeriodStart("ValidFrom");
            tb.UseHistoryTable("CriteriaAudit");
        }));
    }
}