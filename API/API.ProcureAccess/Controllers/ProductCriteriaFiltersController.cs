namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductCriteriaFiltersController : BaseCrudController<ProductCriteriaFilter, ProductCriteriaFilterDto, ProductCriteriaFiltersController>
{
    public ProductCriteriaFiltersController(
        IAppLogging<ProductCriteriaFiltersController> logger, 
        IProductCriteriaFilterService service) : base(logger, service)
    {
    }

    // TODO: put more individual Requests here
}
