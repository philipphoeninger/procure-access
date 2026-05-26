namespace MODELS.ProcureAccess.Entities;

[Table("FilterTypes", Schema = "public")]
[EntityTypeConfiguration(typeof(FilterTypeConfiguration))]
public partial class FilterType : BaseEntity
{
    #region fields
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    public ICollection<CriteriaFilter> CriteriaFilters { get; set; } = new List<CriteriaFilter>();

    [Required]
    public bool IsDeleted { get; set; } = false;

    [NotMapped]
    public string Display => Name;
    #endregion

    #region ctors
    public FilterType() { }

    public FilterType(string pName, string pDescription) : this()
    {
        Name = pName;
        Description = pDescription;
    }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The Filter Type {Name} has the ID {Id}";
    }
    #endregion
}
