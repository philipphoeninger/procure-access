namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProposalsController : BaseCrudController<Proposal, ProposalDto, ProposalsController>
{
    private readonly IProposalService _proposalService;
 
    public ProposalsController(
        IAppLogging<ProposalsController> logger, 
        IProposalService service) : base(logger, service)
    {
        _proposalService = service;
    }

    /// <summary>
    /// Creates/Updates a single record
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="request">Request with Entities to update</param>
    /// <returns>Single record</returns>
    [HttpPost("upsert")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public async Task<ActionResult<ProposalDto>> Upsert([FromBody] UpsertProposalCommand request)
    {
        var result = await _proposalService.UpsertAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Updates proposal object of a single record
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="request">Request with object to update</param>
    /// <returns>Single record</returns>
    [HttpPost("review")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public async Task<ActionResult<ProposalDto>> Review([FromBody] ReviewProposalCommand request)
    {
        var result = await _proposalService.ReviewAsync(request);
        return Ok(result);
    }

    [HttpPost("approve")]
    [ApiVersion("1.0")]
    [AuthApproverAttribute]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public async Task<ActionResult<ProposalDto>> Approve([FromBody] ApproveProposalCommand request)
    {
        var result = await _proposalService.ApproveAsync(request);
        return Ok(result);
    }
}
