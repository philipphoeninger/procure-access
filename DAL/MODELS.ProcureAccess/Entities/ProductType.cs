namespace MODELS.ProcureAccess.Entities;

[Table("ProductTypes", Schema = "dbo")]
[EntityTypeConfiguration(typeof(ProductTypeConfiguration))]
public class ProductType : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int CriteriaFilterId { get; set; }
    public CriteriaFilter CriteriaFilter { get; set; }
}
