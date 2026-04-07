namespace SERVICES.ProcureAccess.DataServices;

public class ProductTestService : BaseService<ProductTest, ProductTestDto>, IProductTestService
{
    public ProductTestService(IProductTestRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }

    public override IEnumerable<ProductTestDto> ReadAll()
    {
        List<ProductTest> entities = MainRepo.GetAll().ToList();
        entities = entities.FindAll(x => x.Product.Proposal == null || 
                                            x.Product.Proposal.IsApproved);
        List<ProductTestDto> dtos = new List<ProductTestDto>();
        entities.ForEach(x => dtos.Add(Mapper.Map<ProductTestDto>(x)));
        return dtos;
    }
}
