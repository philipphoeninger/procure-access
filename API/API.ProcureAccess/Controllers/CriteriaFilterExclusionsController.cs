namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CriteriaFilterExclusionsController : BaseCrudController<CriteriaFilterExclusion, CriteriaFilterExclusionDto, CriteriaFilterExclusionsController>
{
    public CriteriaFilterExclusionsController(
        IAppLogging<CriteriaFilterExclusionsController> logger, 
        ICriteriaFilterExclusionService service) : base(logger, service)
    {
    }

    // TODO: put more individual Requests here
}
