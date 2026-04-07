namespace SERVICES.ProcureAccess.DataServices;

public class ProductPartService : BaseService<ProductPart, ProductPartDto>, IProductPartService
{
    public ProductPartService(IProductPartRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }

    public override IEnumerable<ProductPartDto> ReadAll()
    {
        List<ProductPart> entities = MainRepo.GetAll().ToList();
        entities = entities.FindAll(x => x.Product.Proposal == null || 
                                            x.Product.Proposal.IsApproved);
        List<ProductPartDto> dtos = new List<ProductPartDto>();
        entities.ForEach(x => dtos.Add(Mapper.Map<ProductPartDto>(x)));
        return dtos;
    }
}
