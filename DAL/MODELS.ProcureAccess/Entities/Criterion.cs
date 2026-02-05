using System.ComponentModel.DataAnnotations;

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
    }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The Criterion {Name} has the ID {Id}";
    }
    #endregion
}
