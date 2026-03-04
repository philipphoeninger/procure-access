namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductPartsController : BaseCrudController<ProductPart, ProductPartsController>
{
    public ProductPartsController(IAppLogging<ProductPartsController> logger, IProductPartRepo repo) : base(logger, repo)
    {
    }

    // TODO: put more individual Requests here
}
