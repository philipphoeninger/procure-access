namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CriteriaController : BaseCrudController<Criterion, CriteriaController>
{
    protected readonly ICriterionRepo Repo;

    public CriteriaController(IAppLogging<CriteriaController> logger, ICriterionRepo repo) : base(logger, repo)
    {
        Repo = repo;
    }

    /// <summary>
    /// Updates a single record
    /// </summary>
    /// <remarks>
    /// Sample body:
    /// <pre>
    /// {
    ///     ...
    /// }
    /// </pre>
    /// </remarks>
    /// <param name="id">Primary key of the record to update</param>
    /// <param name="entity">Entity to update</param>
    /// <returns>Single record</returns>
    [HttpPut("{id}")]
    [ApiVersion("1.0")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public override IActionResult UpdateOne(int id, Criterion entity)
    {
        if (id != entity.Id) return BadRequest();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            MainRepo.Update(entity);
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
        return Ok(entity);
    }

    /// <summary>
    /// Adds a single record
    /// </summary>
    /// <remarks>
    /// Sample body:
    /// <pre>
    /// {
    ///     ...
    /// }
    /// </pre>
    /// </remarks>
    /// <param name="entity">Record to add</param>
    /// <returns>Added record</returns>
    [HttpPost]
    [ApiVersion("1.0")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(201, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public override ActionResult<Criterion> AddOne(Criterion entity)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            MainRepo.Add(entity);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
        return CreatedAtAction(nameof(GetOne), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Deletes a single record
    /// </summary>
    /// <remarks>
    /// Sample body:
    /// </remarks>
    /// <param name="id"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ApiVersion("1.0")]
    [Authorize]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public override ActionResult<Criterion> DeleteOne(int id, Criterion entity)
    {
        if (id != entity.Id) return BadRequest();
        try
        {
            MainRepo.Delete(entity);
        }
        catch (Exception ex)
        {
            // TODO: handle more gracefully
            return new BadRequestObjectResult(ex.GetBaseException()?.Message);
        }
        return Ok();
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
    public ActionResult<List<Criterion>> ByCriteriaFilterIds([FromQuery] int[] criteriaFilterIds)
    {
        var filteredCriteria = Repo.GetByCriteriaFilterIds(criteriaFilterIds);
        return Ok(filteredCriteria);
    }

    /// <summary>
    /// Updates a single record
    /// </summary>
    /// <remarks>
    /// Sample body:
    /// <pre>
    /// {
    ///     ...
    /// }
    /// </pre>
    /// </remarks>
    /// <param name="id">Primary key of the record to update</param>
    /// <param name="entity">Entity to update</param>
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
    public IActionResult ApproveOne(CriterionDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            Repo.Approve(dto);
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
        return Ok(dto);
    }
}
