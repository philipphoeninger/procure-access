namespace SERVICES.ProcureAccess.DataServices;

public class ProductCriteriaFilterService : BaseService<ProductCriteriaFilter, ProductCriteriaFilterDto>, IProductCriteriaFilterService
{
    public ProductCriteriaFilterService(IProductCriteriaFilterRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }

    public override async Task<Result<IEnumerable<ProductCriteriaFilterDto>>> ReadAll()
    {
        List<ProductCriteriaFilter> entities = MainRepo.Context.ProductCriteriaFilters
            .Include(x => x.Product)
            .Include(x => x.Product.Proposal)
            .Where(x => 
                x.Product.Proposal == null || 
                x.Product.Proposal.Status == ProposalStatus.Approved
            ).ToList();
        List<ProductCriteriaFilterDto> dtos = new List<ProductCriteriaFilterDto>();
        entities.ForEach(x => dtos.Add(Mapper.Map<ProductCriteriaFilterDto>(x)));
        return Result<IEnumerable<ProductCriteriaFilterDto>>.Success(dtos);
    }
}
