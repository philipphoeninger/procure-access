namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductPartsController : BaseCrudController<ProductPart, ProductPartDto, ProductPartsController>
{
    public ProductPartsController(
        IAppLogging<ProductPartsController> logger, 
        IProductPartService service) : base(logger, service)
    {
    }

    // TODO: put more individual Requests here
}
