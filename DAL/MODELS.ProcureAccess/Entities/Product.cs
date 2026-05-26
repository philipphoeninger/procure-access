namespace MODELS.ProcureAccess.Entities;

[Table("Products", Schema = "public")]
[EntityTypeConfiguration(typeof(ProductConfiguration))]
public partial class Product : BaseEntity
{
    #region fields
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Url]
    [StringLength(500)]
    public string? Link { get; set; }

    [StringLength(6000)]
    public string? Description { get; set; }

    public ICollection<ProductCriteriaFilter> ProductCriteriaFilters { get; set; } = new List<ProductCriteriaFilter>();

    public Proposal? Proposal { get; set; }

    [Required]
    public bool IsDeleted { get; set; } = false;

    [NotMapped]
    public string Display => Name;
    #endregion

    #region ctors
    public Product() { }

    public Product(string pName)
    {
        Name = pName;
    }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The Product {Name} has the ID {Id}";
    }
    #endregion
}
