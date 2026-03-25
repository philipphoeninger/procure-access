namespace MODELS.ProcureAccess.Entities.Configuration;

public class ProductPartConfiguration : IEntityTypeConfiguration<ProductPart>
{
    public void Configure(EntityTypeBuilder<ProductPart> builder)
    {
        builder.HasQueryFilter(x => !x.Product.ToApprove);

        builder.HasKey(x => new { x.Id, x.ProductId, x.CriteriaFilterId });

        builder
            .HasOne(x => x.CriteriaFilter)
            .WithMany(cf => cf.ProductParts)
            .HasForeignKey(x => x.CriteriaFilterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
