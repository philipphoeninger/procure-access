namespace MODELS.ProcureAccess.Entities;

[Table("CriteriaFilterExclusions", Schema = "public")]
[EntityTypeConfiguration(typeof(CriteriaFilterExclusionConfiguration))]
public partial class CriteriaFilterExclusion : BaseEntity
{
    #region fields
    [Required]
    public int CriteriaFilterId { get; set; }
    public CriteriaFilter CriteriaFilter { get; set; }

    [Required]
    public int ExclusionId { get; set; }
    public CriteriaFilter Exclusion { get; set; }
    #endregion

    #region ctors
    public CriteriaFilterExclusion() { }

    public CriteriaFilterExclusion(
        int pCriteriaFilterId,
        int pExclusionId) : this()
    {
        CriteriaFilterId = pCriteriaFilterId;
        ExclusionId = pExclusionId;
    }
    #endregion

    #region methods
    public override string ToString()
    {
        return $"The CriteriaFilterExclusion has the ID {Id}";
    }
    #endregion
}
