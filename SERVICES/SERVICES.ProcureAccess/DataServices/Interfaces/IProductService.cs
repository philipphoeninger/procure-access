namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface IProductService : IBaseService<Product, ProductDto>
{
    Task<Result<IEnumerable<ProductDto>>> GetByCriteriaFilterIds(int[] criteriaFilterIds);
}
