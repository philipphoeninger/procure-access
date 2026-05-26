namespace MODELS.ProcureAccess.Entities;

[Table("Criteria", Schema = "public")]
[EntityTypeConfiguration(typeof(CriterionConfiguration))]
public partial class Criterion : BaseEntity
{
    #region fields
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    [StringLength(6000)]
    public string Description { get; set; } = string.Empty;

    public int CriteriaFilterId { get; set; }
    public CriteriaFilter CriteriaFilter { get; set; }

    public Proposal? Proposal { get; set; }

    [Required]
    public bool IsDeleted { get; set; } = false;

    [NotMapped]
    public string Display => Name;
    #endregion

    #region ctors
    public Criterion() { }

    public Criterion(string pName, string pDescription) : this()
    {
        Name = pName;
        Description = pDescription;
    }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The Criterion {Name} has the ID {Id}";
    }
    #endregion
}
