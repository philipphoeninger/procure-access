namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilterTypesController : BaseCrudController<FilterType, FilterTypesController>
{
    public FilterTypesController(IAppLogging<FilterTypesController> logger, IFilterTypeRepo repo) : base(logger, repo)
    {
    }

    // TODO: put more individual Requests here
}