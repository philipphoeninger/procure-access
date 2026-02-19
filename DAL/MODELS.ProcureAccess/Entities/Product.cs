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
    public string Link { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    [StringLength(6000)]
    public string Description { get; set; }

    public virtual CriteriaFilter Type { get; set; }

    public ICollection<CriteriaFilter> Parts { get; set; }

    public ICollection<CriteriaFilter> Tests { get; set; }

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
    }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The Product {Name} has the ID {Id}";
    }
    #endregion
}
