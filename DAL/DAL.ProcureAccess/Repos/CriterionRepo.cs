namespace DAL.ProcureAccess.Repos;

public class CriterionRepo : TemporalTableBaseRepo<Criterion>, ICriterionRepo
{
    #region ctors
    public CriterionRepo(ApplicationDBContext context) : base(context) { }
    internal CriterionRepo(DbContextOptions<ApplicationDBContext> options) : base(options) { }
    #endregion

    #region methods
    // public override IEnumerable<Criterion> GetAll()
    // {
    //     return Context.Criteria;
    // }
    #endregion
}
