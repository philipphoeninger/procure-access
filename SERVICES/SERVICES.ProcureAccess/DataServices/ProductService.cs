namespace SERVICES.ProcureAccess.DataServices;

public class ProductService : BaseService<Product, ProductDto>, IProductService
{
    public ProductService(IProductRepo repo, IMapper mapper) : base(repo, mapper)
    {
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
        product.ToApprove = false;

        return MainRepo.SaveChanges();
    }
}
