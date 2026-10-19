namespace MODELS.ProcureAccess.Entities.Configuration;

public class ProductCriteriaFilterConfiguration
    : IEntityTypeConfiguration<ProductCriteriaFilter>
{
    public void Configure(EntityTypeBuilder<ProductCriteriaFilter> builder)
    {
        // Key
        builder.HasKey(x => new
        {
            x.ProductId,
            x.CriteriaFilterId
        });
        
        // Query Filters
        builder.HasQueryFilter(x => !x.Product.IsDeleted);

        builder.HasIndex(x => new
        {
            x.ProductId, 
            x.CriteriaFilterId
        }).IsUnique();

        builder
            .HasOne(x => x.CriteriaFilter)
            .WithMany(cf => cf.Products)
            .HasForeignKey(x => x.CriteriaFilterId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Data
        builder.HasData(ProductCriteriaFilterSeed.Data);
    }
}
