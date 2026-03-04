namespace DAL.ProcureAccess.Repos;

public class ProductPartRepo : TemporalTableBaseRepo<ProductPart>, IProductPartRepo
{
    #region ctors
    public ProductPartRepo(ApplicationDBContext context) : base(context) { }
    internal ProductPartRepo(DbContextOptions<ApplicationDBContext> options) : base(options) { }
    #endregion

    #region methods
    // TODO

    // public override IEnumerable<ProductPart> GetAll()
    // {
    //     return Context.ProductParts;
    // }
    #endregion
}
