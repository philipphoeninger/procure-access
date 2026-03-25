namespace DAL.ProcureAccess.Repos;

public class ProductRepo : TemporalTableBaseRepo<Product>, IProductRepo
{
    #region ctors
    public ProductRepo(ApplicationDBContext context) : base(context) { }
    internal ProductRepo(DbContextOptions<ApplicationDBContext> options) : base(options) { }
    #endregion

    #region methods
    // public override IEnumerable<Product> GetAll()
    // {
    //     return Context.Products;
    // }

    public int Approve(ProductDto dto)
    {
        var product = FindIgnoreQueryFilters(dto.Id);

        if (dto.Name != null)
            product.Name = dto.Name;
        if (dto.Link != null)
            product.Link = dto.Link;
        if (dto.Description != null)
            product.Description = dto.Description;
        if (dto.TypeId != null)
            product.TypeId = (int)dto.TypeId;

        if (dto.IsDeleted.HasValue)
            product.IsDeleted = dto.IsDeleted.Value;

        // approve
        product.ToApprove = false;

        return SaveChanges();
    }
    #endregion
}
