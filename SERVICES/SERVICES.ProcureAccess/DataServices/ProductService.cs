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
        return dtos;
    }

    public int Approve(ProductDto dto)
    {
        var product = MainRepo.FindIgnoreQueryFilters(dto.Id);

        if (dto.Name != null)
            product.Name = dto.Name;
        if (dto.Link != null)
            product.Link = dto.Link;
        if (dto.Description != null)
            product.Description = dto.Description;

        if (dto.IsDeleted.HasValue)
            product.IsDeleted = dto.IsDeleted.Value;

        // approve
        product.Proposal.IsApproved = true;

        return MainRepo.SaveChanges();
    }
}
