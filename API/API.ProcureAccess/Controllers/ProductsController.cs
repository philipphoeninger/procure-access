namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : BaseCrudController<Product, ProductDto, ProductsController>
{
    private readonly IProductService _productService;

    public ProductsController(
        IAppLogging<ProductsController> logger, 
        IProductService service) : base(logger, service)
    {
        _productService = service;
    }
}
