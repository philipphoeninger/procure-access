namespace MODELS.ProcureAccess.Entities;

[Table("Criteria", Schema = "dbo")]
[EntityTypeConfiguration(typeof(CriterionConfiguration))]
public partial class Criterion : BaseEntity
{
    #region fields
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    [StringLength(6000)]
    public string Description { get; set; } = string.Empty;

    public int CriteriaFilterId { get; set; }
    public CriteriaFilter CriteriaFilter { get; set; }

    public Proposal? Proposal { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public bool IsDeleted { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public string? Display { get; set; }
    #endregion

    #region ctors
    public Criterion()
    {
        // TODO: generate Name
    }

    public Criterion(string pName, string pDescription)
    {
        Name = pName;
        Description = pDescription;
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The Criterion {Name} has the ID {Id}";
    }
    #endregion
}
