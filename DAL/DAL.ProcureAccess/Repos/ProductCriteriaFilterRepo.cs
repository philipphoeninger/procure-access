namespace DAL.ProcureAccess.Repos;

public class ProductCriteriaFilterRepo : TemporalTableBaseRepo<ProductCriteriaFilter>, IProductCriteriaFilterRepo
{
    #region ctors
    public ProductCriteriaFilterRepo(ApplicationDBContext context) : base(context) { }
    internal ProductCriteriaFilterRepo(DbContextOptions<ApplicationDBContext> options) : base(options) { }
    #endregion

    #region methods
    // TODO

    // public override IEnumerable<ProductCriteriaFilter> GetAll()
    // {
    //     return Context.ProductCriteriaFilters;
    // }
    #endregion
}
