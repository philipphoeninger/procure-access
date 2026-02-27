namespace MODELS.ProcureAccess.Entities;

[Table("ProductParts", Schema = "dbo")]
[EntityTypeConfiguration(typeof(ProductPartConfiguration))]
public class ProductPart
{
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int CriteriaFilterId { get; set; }
    public CriteriaFilter CriteriaFilter { get; set; }
}
