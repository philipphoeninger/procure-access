namespace DAL.ProcureAccess.Repos;

public class ProductTestRepo : TemporalTableBaseRepo<ProductTest>, IProductTestRepo
{
    #region ctors
    public ProductTestRepo(ApplicationDBContext context) : base(context) { }
    internal ProductTestRepo(DbContextOptions<ApplicationDBContext> options) : base(options) { }
    #endregion

    #region methods
    // TODO

    // public override IEnumerable<ProductTest> GetAll()
    // {
    //     return Context.ProductTests;
    // }
    #endregion
}
