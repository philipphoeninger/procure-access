// namespace MODELS.ProcureAccess.Entities.Configuration;

// public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
// {
//     public void Configure(EntityTypeBuilder<Favorite> builder)
//     {
//         // properties

//         // temporal
//         builder.ToTable(b => b.IsTemporal(tb =>
//         {
//             tb.HasPeriodEnd("ValidTo");
//             tb.HasPeriodStart("ValidFrom");
//             tb.UseHistoryTable("CriteriaAudit");
//         }));
//     }
// }
