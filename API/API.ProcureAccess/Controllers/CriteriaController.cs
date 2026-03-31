namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CriteriaController : BaseCrudController<Criterion, CriterionDto, CriteriaController>
{
    private readonly ICriterionService _criterionService;

    public CriteriaController(
        IAppLogging<CriteriaController> logger, 
        ICriterionService service) : base(logger, service)
    {
        _criterionService = service;
    }

    /// <summary>
    /// Gets all records, that have the provided Criteria Filter Id(s).
    /// </summary>
    /// <param name="criteriaFilterIds">To filter Criterias by Criteria Filter Ids.</param>
    /// <returns>Criteria</returns>
    [HttpGet("byCriteriaFilterIds")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(204, "No content")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public ActionResult<List<CriterionDto>> ByCriteriaFilterIds([FromQuery] int[] criteriaFilterIds)
    {
        var filteredCriteria = _criterionService.GetByCriteriaFilterIds(criteriaFilterIds);
        return Ok(filteredCriteria);
    }

    /// <summary>
    /// Updates & approves a single record
    /// </summary>
    /// <remarks>
    /// <pre>
    /// </pre>
    /// </remarks>
    /// <param name="dto">Dto of Entity to update & approve</param>
    /// <returns>Single record</returns>
    [HttpPut("approve")]
    [ApiVersion("1.0")]
    // [Authorize(Roles = $"{Roles.Admin},{Roles.Approver}")]
    [AuthApproverAttribute]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public ActionResult<bool> ApproveOne(CriterionDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        int success = -1;
        try
        {
            success = _criterionService.Approve(dto);
        }
        catch (CustomException ex)
        {
            // TODO: handle more gracefully
            return BadRequest(ex);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
        return Ok(Convert.ToBoolean(success));
    }
}
