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
}
