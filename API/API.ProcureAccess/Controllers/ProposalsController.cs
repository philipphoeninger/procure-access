// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProposalsController : BaseCrudController<Proposal, ProposalDto>
{
    private readonly IProposalService _proposalService;

    public ProposalsController(IProposalService service) : base(service)
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
    public async Task<ActionResult<ProposalDto>> UpsertAsync([FromBody] UpsertProposalCommand request)
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
    public async Task<ActionResult<ProposalDto>> ReviewAsync([FromBody] ReviewProposalCommand request)
    {
        var result = await _proposalService.ReviewAsync(request);
        return Ok(result);
    }

    [HttpPost("approve")]
    [ApiVersion("1.0")]
    [Authorize(Policy = AuthorizationPolicies.ModerationReview)]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public async Task<ActionResult<ProposalDto>> ApproveAsync([FromBody] ApproveProposalCommand request)
    {
        var result = await _proposalService.ApproveAsync(request);
        return Ok(result);
    }
}
