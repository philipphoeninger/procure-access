namespace MODELS.ProcureAccess.Entities;

[Table("ProductCriteriaFilters", Schema = "public")]
public class ProductCriteriaFilter
{
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int CriteriaFilterId { get; set; }
    public CriteriaFilter CriteriaFilter { get; set; }
}
