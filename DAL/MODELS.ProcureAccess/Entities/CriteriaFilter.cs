namespace MODELS.ProcureAccess.Entities;

[Table("CriteriaFilters", Schema = "dbo")]
[EntityTypeConfiguration(typeof(CriteriaFilterConfiguration))]
public partial class CriteriaFilter : BaseEntity
{
    #region fields
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    [ForeignKey("FilterType")]
    public int FilterTypeId { get; set; }

    public virtual FilterType FilterTypes { get; set; }

    public ICollection<Criterion> Criteria { get; } = new List<Criterion>();

    public int? ProductIdPart { get; set; }
    public Product? ProductPart { get; set; }

    public int? ProductIdTest { get; set; }
    public Product? ProductTest { get; set; }

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

    public CriteriaFilter(string pName)
    {
        Name = pName;
        CreatedAt = DateTime.Now;
    }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The CriteriaFilter {Name} has the ID {Id}";
    }
    #endregion
}
