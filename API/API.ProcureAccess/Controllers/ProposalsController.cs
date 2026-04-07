namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProposalsController : BaseCrudController<Proposal, ProposalDto, ProposalsController>
{
    public ProposalsController(
        IAppLogging<ProposalsController> logger, 
        IProposalService service) : base(logger, service)
    {
    }

    // TODO: put more individual Requests here
}
