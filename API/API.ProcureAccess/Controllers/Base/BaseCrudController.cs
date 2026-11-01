// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

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
    public virtual async Task<ActionResult<TDto>> AddOne(TDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            await MainService.Create(dto);
        }
        catch (Exception ex)
        {
            return Failure(ex, "The record could not be created.");
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
    public async Task<ActionResult<TDto>> GetOne(int id)
    {
        Result<TDto?> dto = await MainService.Read(id);

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
    public virtual async Task<ActionResult<IEnumerable<TDto>>> GetAll()
    {
        return Ok(await MainService.ReadAll());
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
    public virtual async Task<IActionResult> UpdateOne(TDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            await MainService.Update(dto);
        }
        catch (Exception ex)
        {
            return Failure(ex, "The record could not be updated.");
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
            return Failure(ex, "The record could not be deleted.");
        }
        return result != null ? Ok(result) : Problem(statusCode: StatusCodes.Status400BadRequest, detail: "The record could not be deleted.");
    }

    /// <summary>
    /// A failed write as 400 Problem Details (TECH_DEBT_v1 §5, S2-10). The exception is logged and
    /// its text never reaches the client. Legacy: the generic CRUD endpoints are rebuilt for catalog
    /// administration in migration step 2, with status codes per failure.
    /// </summary>
    protected ObjectResult Failure(Exception exception, string detail)
    {
        Logger.LogAppError(exception, detail);
        return Problem(statusCode: StatusCodes.Status400BadRequest, detail: detail);
    }
}
