namespace MODELS.ProcureAccess.Entities;

[Table("CriteriaFilters", Schema = "dbo")]
[EntityTypeConfiguration(typeof(CriteriaFilterConfiguration))]
public partial class CriteriaFilter : BaseEntity
{
    #region fields
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [ForeignKey("FilterType")]
    public int FilterTypeId { get; set; }

    public FilterType FilterType { get; set; }

    public ICollection<Criterion> Criteria { get; set; } = new List<Criterion>();

    public ICollection<ProductType> ProductTypes { get; set; } = new List<ProductType>();

    public ICollection<ProductPart> ProductParts { get; set; } = new List<ProductPart>();

    public ICollection<ProductTest> ProductTests { get; set; } = new List<ProductTest>();

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public bool IsDeleted { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string? Display { get; set; }

    #endregion

    #region ctors
    public CriteriaFilter()
    {
        // TODO: generate Name
    }

    public CriteriaFilter(string pName, int pFilterTypeId, string pDescription)
    {
        Name = pName;
        Description = pDescription;
        FilterTypeId = pFilterTypeId;
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The CriteriaFilter {Name} has the ID {Id}";
    }
    #endregion
}
