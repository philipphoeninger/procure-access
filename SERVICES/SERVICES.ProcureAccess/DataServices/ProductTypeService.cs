namespace SERVICES.ProcureAccess.DataServices;

public class ProductTypeService : BaseService<ProductType, ProductTypeDto>, IProductTypeService
{
    public ProductTypeService(IProductTypeRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }
}
