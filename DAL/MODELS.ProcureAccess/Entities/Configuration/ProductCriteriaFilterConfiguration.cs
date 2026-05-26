namespace MODELS.ProcureAccess.Entities.Configuration;

public class ProductCriteriaFilterConfiguration : BaseEntityConfiguration<ProductCriteriaFilter>
{
    public void Configure(EntityTypeBuilder<ProductCriteriaFilter> builder)
    {
        base.Configure(builder);

        // Query Filters
        builder.HasQueryFilter(x => !x.Product.IsDeleted);

        builder.HasKey(x => new { x.Id, x.ProductId, x.CriteriaFilterId });

        builder
            .HasOne(x => x.CriteriaFilter)
            .WithMany(cf => cf.Products)
            .HasForeignKey(x => x.CriteriaFilterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
