namespace SERVICES.ProcureAccess.DataServices.Interfaces;

public interface IProductService : IBaseService<Product, ProductDto>
{
    Result<IEnumerable<ProductDto>> GetByCriteriaFilterIds(int[] criteriaFilterIds);
}
