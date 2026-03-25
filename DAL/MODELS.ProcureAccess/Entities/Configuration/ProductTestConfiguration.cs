namespace MODELS.ProcureAccess.Entities.Configuration;

public class ProductTestConfiguration : IEntityTypeConfiguration<ProductTest>
{
    public void Configure(EntityTypeBuilder<ProductTest> builder)
    {
        builder.HasQueryFilter(x => !x.Product.ToApprove);

        builder.HasKey(x => new { x.Id, x.ProductId, x.CriteriaFilterId });

        builder
            .HasOne(x => x.CriteriaFilter)
            .WithMany(cf => cf.ProductTests)
            .HasForeignKey(x => x.CriteriaFilterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
