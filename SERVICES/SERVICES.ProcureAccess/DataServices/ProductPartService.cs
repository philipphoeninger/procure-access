namespace SERVICES.ProcureAccess.DataServices;

public class ProductPartService : BaseService<ProductPart, ProductPartDto>, IProductPartService
{
    public ProductPartService(IProductPartRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }
}
