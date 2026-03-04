namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : BaseCrudController<Product, ProductsController>
{
    public ProductsController(IAppLogging<ProductsController> logger, IProductRepo repo) : base(logger, repo)
    {
    }

    // TODO: put more individual Requests here
}
