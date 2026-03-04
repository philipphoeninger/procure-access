namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CriteriaController : BaseCrudController<Criterion, CriteriaController>
{
    public CriteriaController(IAppLogging<CriteriaController> logger, ICriterionRepo repo) : base(logger, repo)
    {
    }

    // TODO: put more individual Requests here
}