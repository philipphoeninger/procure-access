namespace MODELS.ProcureAccess.Entities;

[Table("ProductTests", Schema = "dbo")]
[EntityTypeConfiguration(typeof(ProductTestConfiguration))]
public class ProductTest : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int CriteriaFilterId { get; set; }
    public CriteriaFilter CriteriaFilter { get; set; }
}
