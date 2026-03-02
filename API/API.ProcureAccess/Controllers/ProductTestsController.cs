namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductTestsController : BaseCrudController<ProductTest, ProductTestsController>
{
    public ProductTestsController(IAppLogging<ProductTestsController> logger, IProductTestRepo repo) : base(logger, repo)
    {
    }

    // TODO: put more individual Requests here
}
