namespace SERVICES.ProcureAccess.DataServices;

public class ProductTypeService : BaseService<ProductType, ProductTypeDto>, IProductTypeService
{
    public ProductTypeService(IProductTypeRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }

    public override IEnumerable<ProductTypeDto> ReadAll()
    {
        List<ProductType> entities = MainRepo.GetAll().ToList();
        entities = entities.FindAll(x => x.Product.Proposal == null || 
                                            x.Product.Proposal.IsApproved);
        List<ProductTypeDto> dtos = new List<ProductTypeDto>();
        entities.ForEach(x => dtos.Add(Mapper.Map<ProductTypeDto>(x)));
        return dtos;
    }
}
