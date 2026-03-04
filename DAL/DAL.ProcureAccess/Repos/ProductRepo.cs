namespace DAL.ProcureAccess.Repos;

public class ProductRepo : TemporalTableBaseRepo<Product>, IProductRepo
{
    #region ctors
    public ProductRepo(ApplicationDBContext context) : base(context) { }
    internal ProductRepo(DbContextOptions<ApplicationDBContext> options) : base(options) { }
    #endregion

    #region methods
    // TODO

    // public override IEnumerable<Product> GetAll()
    // {
    //     return Context.Products;
    // }
    #endregion
}
