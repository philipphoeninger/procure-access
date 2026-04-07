namespace MODELS.ProcureAccess.Entities;

[Table("Products", Schema = "dbo")]
[EntityTypeConfiguration(typeof(ProductConfiguration))]
public partial class Product : BaseEntity
{
    #region fields
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(500)]
    public string? Link { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    [StringLength(6000)]
    public string? Description { get; set; }

    public ICollection<ProductType> Types { get; set; } = new List<ProductType>();

    public ICollection<ProductPart> Parts { get; set; } = new List<ProductPart>();

    public ICollection<ProductTest> Tests { get; set; } = new List<ProductTest>();

    public int? ProposalId { get; set; }
    public Proposal? Proposal { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public bool IsDeleted { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string? Display { get; set; }
    #endregion

    #region ctors
    public Product()
    {
        // TODO: generate Name
    }

    public Product(string pName)
    {
        Name = pName;
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The Product {Name} has the ID {Id}";
    }
    #endregion
}
