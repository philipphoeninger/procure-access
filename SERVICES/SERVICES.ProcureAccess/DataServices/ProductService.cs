namespace SERVICES.ProcureAccess.DataServices;

public class ProductService : BaseService<Product, ProductDto>, IProductService
{
    public ProductService(IProductRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }

    public override async Task<Result<IEnumerable<ProductDto>>> ReadAll()
    {
        List<Product> entities = MainRepo.Context.Products.Where(x => 
            x.Proposal == null || x.Proposal.Status == ProposalStatus.Approved).ToList();
        List<ProductDto> dtos = new List<ProductDto>();
        entities.ForEach(x => dtos.Add(Mapper.Map<ProductDto>(x)));
        return Result<IEnumerable<ProductDto>>.Success(dtos);
    }
}
