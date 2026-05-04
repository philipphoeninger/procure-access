namespace MODELS.ProcureAccess.Entities.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasQueryFilter(u => !u.IsDeleted);

        // properties
        builder.OwnsOne(
            u => u.UICustomization,
            ui =>
            {
                ui.Property(x => x.ForegroundColor)
                    .HasMaxLength(10);
                ui.Property(x => x.BackgroundColor)
                    .HasMaxLength(10);
                ui.Property(x => x.TextColor)
                    .HasMaxLength(10);

                ui.ToJson();
            }
        );
    }
}
