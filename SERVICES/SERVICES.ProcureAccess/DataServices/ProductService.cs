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

    public Result<IEnumerable<ProductDto>> GetByCriteriaFilterIds(int[] criteriaFilterIds)
    {
        List<Product> products = 
            MainRepo.Context.Products
                .Include(p => p.ProductCriteriaFilters)
                .ToList();
        List<Product> filteredProducts = products.FindAll(x =>
        {
            var productCriteriaFilterIds = 
                x.ProductCriteriaFilters.Select(y => y.CriteriaFilterId);
            for (int i = 0; i < criteriaFilterIds.Length; i++)
            {
                if (!productCriteriaFilterIds.Any(a => 
                    a == criteriaFilterIds[i]))
                    return false;
            }
            return true;
        });
        List<ProductDto> productDtos = new List<ProductDto>();
        filteredProducts.ForEach(x => productDtos.Add(Mapper.Map<ProductDto>(x)));
        return Result<IEnumerable<ProductDto>>.Success(productDtos);
    }
}
