namespace MODELS.ProcureAccess.Entities;

[Table("FilterTypes", Schema = "dbo")]
[EntityTypeConfiguration(typeof(FilterTypeConfiguration))]
public partial class FilterType : BaseEntity
{
    #region fields
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public bool IsDeleted { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string? Display { get; set; }
    #endregion

    #region ctors
    public FilterType()
    {
        // TODO: generate Name
    }

    public FilterType(string pName)
    {
        Name = pName;
    }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The Filter Type {Name} has the ID {Id}";
    }
    #endregion
}
