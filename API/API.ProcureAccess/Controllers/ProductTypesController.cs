namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductTypesController : BaseCrudController<ProductType, ProductTypeDto, ProductTypesController>
{
    public ProductTypesController(
        IAppLogging<ProductTypesController> logger, 
        IProductTypeService service) : base(logger, service)
    {
    }

    // TODO: put more individual Requests here
}
