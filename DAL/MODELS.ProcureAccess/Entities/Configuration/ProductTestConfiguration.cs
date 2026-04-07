namespace MODELS.ProcureAccess.Entities.Configuration;

public class ProductTestConfiguration : IEntityTypeConfiguration<ProductTest>
{
    public void Configure(EntityTypeBuilder<ProductTest> builder)
    {
        // Query Filters
        builder.HasQueryFilter(x => !x.Product.IsDeleted);

        builder.HasKey(x => new { x.Id, x.ProductId, x.CriteriaFilterId });

        builder
            .HasOne(x => x.CriteriaFilter)
            .WithMany(cf => cf.ProductTests)
            .HasForeignKey(x => x.CriteriaFilterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
