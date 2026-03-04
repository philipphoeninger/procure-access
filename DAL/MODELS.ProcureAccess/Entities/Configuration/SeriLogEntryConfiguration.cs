namespace MODELS.ProcureAccess.Entities.Configuration;

public class SeriLogEntryConfiguration : IEntityTypeConfiguration<SeriLogEntry>
{
    public void Configure(EntityTypeBuilder<SeriLogEntry> builder)
    {
        builder.Property(x => x.Properties).HasColumnType("Xml");
        builder.Property(x => x.TimeStamp).HasDefaultValueSql("GetDate()");
    }
}
