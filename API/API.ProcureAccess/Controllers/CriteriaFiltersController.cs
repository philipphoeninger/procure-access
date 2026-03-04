namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CriteriaFiltersController : BaseCrudController<CriteriaFilter, CriteriaFiltersController>
{
    public CriteriaFiltersController(IAppLogging<CriteriaFiltersController> logger, ICriteriaFilterRepo repo) : base(logger, repo)
    {
    }

    // TODO: put more individual Requests here
}
