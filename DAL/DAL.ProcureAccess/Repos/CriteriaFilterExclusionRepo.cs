namespace DAL.ProcureAccess.Repos;

public class CriteriaFilterExclusionRepo : TemporalTableBaseRepo<CriteriaFilterExclusion>, ICriteriaFilterExclusionRepo
{
    #region ctors
    public CriteriaFilterExclusionRepo(ApplicationDBContext context) : base(context) { }
    internal CriteriaFilterExclusionRepo(DbContextOptions<ApplicationDBContext> options) : base(options) { }
    #endregion

    #region methods
    // TODO

    // public override IEnumerable<CriteriaFilterExclusion> GetAll()
    // {
    //     return Context.CriteriaFilterExclusions;
    // }
    #endregion
}
