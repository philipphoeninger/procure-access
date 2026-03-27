namespace API.ProcureAccess.Controllers.Base;

[ApiController]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BaseCrudController<TEntity, TDto, TController> : ControllerBase
    where TEntity : BaseEntity, new()
    where TDto : BaseDto, new()
    where TController : class
{
    protected readonly IBaseService<TEntity, TDto> MainService;
    protected readonly IAppLogging<TController> Logger;

    protected BaseCrudController(
        IAppLogging<TController> logger, 
        IBaseService<TEntity, TDto> service)
    {
        MainService = service;
        Logger = logger;
    }

    /// <summary>
    /// Adds a single record
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="dto">Record to add</param>
    /// <returns>Added record</returns>
    [HttpPost]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(201, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public virtual ActionResult<TDto> AddOne(TDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            MainService.Create(dto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
        return CreatedAtAction(nameof(GetOne), new { id = dto.Id });
    }

    /// <summary>
    /// Gets a single record
    /// </summary>
    /// <param name="id">Primary key of the record</param>
    /// <returns>Single record</returns>
    [HttpGet("{id}")]
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
    public ActionResult<TDto> GetOne(int id)
    {
        TDto? dto = MainService.Read(id);

        return dto == null ? NoContent() : Ok(dto);
    }

    /// <summary>
    /// Gets all records
    /// </summary>
    /// <returns>All records</returns>
    [HttpGet]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    public virtual ActionResult<IEnumerable<TDto>> GetAll()
    {
        return Ok(MainService.ReadAll());
    }

    /// <summary>
    /// Updates a single record
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="dto">Dto of Entity to update</param>
    /// <returns>Single record</returns>
    [HttpPut("{id}")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public virtual IActionResult UpdateOne(TDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            MainService.Update(dto);
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

    /// <summary>
    /// Deletes a single record
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public virtual ActionResult<int> DeleteOne(int id)
    {
        int? result = null;
        try
        {
            result = MainService.Delete(id);
        }
        catch (Exception ex)
        {
            // TODO: handle more gracefully
            return new BadRequestObjectResult(ex.GetBaseException()?.Message);
        }
        return result != null ? Ok(result) : BadRequest("Deletion failed");
    }
}
