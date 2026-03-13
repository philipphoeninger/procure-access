namespace MODELS.ProcureAccess.Entities.Configuration;

public class UICustomizationConfiguration : IEntityTypeConfiguration<UICustomization>
{
    public void Configure(EntityTypeBuilder<UICustomization> builder)
    {
        // properties
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GetDate()");

        builder.Property(x => x.ForegroundColor).HasDefaultValue("#111111");
        builder.Property(x => x.BackgroundColor).HasDefaultValue("#dbdedf");
        builder.Property(x => x.TextColor).HasDefaultValue("#111111");
        builder.Property(x => x.OrientationVertical).HasDefaultValue(true);

        builder
            .HasOne(x => x.User)
            .WithOne(u => u.UICustomization)
            .HasForeignKey<UICustomization>(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasIndex(x => x.UserId)
            .IsUnique();

        // temporal
        builder.ToTable(b => b.IsTemporal(tb =>
        {
            tb.HasPeriodEnd("ValidTo");
            tb.HasPeriodStart("ValidFrom");
            tb.UseHistoryTable("UICustomizationAudit");
        }));
    }
}
