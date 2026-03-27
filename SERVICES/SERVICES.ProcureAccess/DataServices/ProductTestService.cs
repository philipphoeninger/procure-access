namespace SERVICES.ProcureAccess.DataServices;

public class ProductTestService : BaseService<ProductTest, ProductTestDto>, IProductTestService
{
    public ProductTestService(IProductTestRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }
}
