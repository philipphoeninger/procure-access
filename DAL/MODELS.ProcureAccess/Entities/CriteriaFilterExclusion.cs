namespace MODELS.ProcureAccess.Entities;

[Table("CriteriaFilterExclusions", Schema = "public")]
public partial class CriteriaFilterExclusion
{
    public int CriteriaFilterId { get; set; }
    public CriteriaFilter CriteriaFilter { get; set; }

    public int ExclusionId { get; set; }
    public CriteriaFilter Exclusion { get; set; }
}
