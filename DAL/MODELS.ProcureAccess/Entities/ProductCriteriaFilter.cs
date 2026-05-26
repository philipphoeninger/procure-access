namespace MODELS.ProcureAccess.Entities;

[Table("ProductCriteriaFilters", Schema = "public")]
[EntityTypeConfiguration(typeof(ProductCriteriaFilterConfiguration))]
public class ProductCriteriaFilter : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int CriteriaFilterId { get; set; }
    public CriteriaFilter CriteriaFilter { get; set; }
}
