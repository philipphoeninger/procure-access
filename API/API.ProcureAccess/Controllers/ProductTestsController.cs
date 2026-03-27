namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductTestsController : BaseCrudController<ProductTest, ProductTestDto, ProductTestsController>
{
    public ProductTestsController(
        IAppLogging<ProductTestsController> logger, 
        IProductTestService service) : base(logger, service)
    {
    }

    // TODO: put more individual Requests here
}
