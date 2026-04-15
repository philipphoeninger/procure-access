namespace SERVICES.ProcureAccess.DataServices;

public class ProductTestService : BaseService<ProductTest, ProductTestDto>, IProductTestService
{
    public ProductTestService(IProductTestRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }

    public override async Task<Result<IEnumerable<ProductTestDto>>> ReadAll()
    {
        List<ProductTest> entities = MainRepo.GetAll().ToList();
        entities = entities.FindAll(x => x.Product.Proposal == null || 
                                            x.Product.Proposal.Status == ProposalStatus.Approved);
        List<ProductTestDto> dtos = new List<ProductTestDto>();
        entities.ForEach(x => dtos.Add(Mapper.Map<ProductTestDto>(x)));
        return Result<IEnumerable<ProductTestDto>>.Success(dtos);
    }
}
