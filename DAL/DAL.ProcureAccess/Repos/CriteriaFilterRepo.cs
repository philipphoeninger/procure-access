namespace DAL.ProcureAccess.Repos;

public class CriteriaFilterRepo : TemporalTableBaseRepo<CriteriaFilter>, ICriteriaFilterRepo
{
    #region ctors
    public CriteriaFilterRepo(ApplicationDBContext context) : base(context) { }
    internal CriteriaFilterRepo(DbContextOptions<ApplicationDBContext> options) : base(options) { }
    #endregion

    #region methods
    // TODO

    // public override IEnumerable<CriteriaFilter> GetAll()
    // {
    //     return Context.CriteriaFilters;
    // }
    #endregion
}
