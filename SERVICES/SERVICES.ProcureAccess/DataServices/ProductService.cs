namespace SERVICES.ProcureAccess.DataServices;

public class ProductService : BaseService<Product, ProductDto>, IProductService
{
    public ProductService(IProductRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }

    public override IEnumerable<ProductDto> ReadAll()
    {
        List<Product> entities = MainRepo.GetAll().ToList();
        entities = entities.FindAll(x => x.Proposal == null || x.Proposal.IsApproved);
        List<ProductDto> dtos = new List<ProductDto>();
        entities.ForEach(x => dtos.Add(Mapper.Map<ProductDto>(x)));
        return Result<IEnumerable<ProductDto>>.Success(dtos);
    }
}
