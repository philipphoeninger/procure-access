namespace DAL.ProcureAccess.Repos;

public class ProductTypeRepo : TemporalTableBaseRepo<ProductType>, IProductTypeRepo
{
    #region ctors
    public ProductTypeRepo(ApplicationDBContext context) : base(context) { }
    internal ProductTypeRepo(DbContextOptions<ApplicationDBContext> options) : base(options) { }
    #endregion

    #region methods
    // TODO

    // public override IEnumerable<ProductType> GetAll()
    // {
    //     return Context.ProductTypes;
    // }
    #endregion
}
