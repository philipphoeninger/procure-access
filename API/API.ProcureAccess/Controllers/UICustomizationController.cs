namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UICustomizationController : ControllerBase
{
    protected readonly IUICustomizationRepo Repo;
    protected readonly UserManager<User> UserManager;

    public UICustomizationController(
        IAppLogging<UICustomizationController> logger, 
        IUICustomizationRepo repo,
        UserManager<User> userManager)
    {
        Repo = repo;
        UserManager = userManager;
    }

    /// <summary>
    /// Gets a single record
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <returns>Single record</returns>
    [HttpGet]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public async Task<ActionResult<UICustomizationDto>> GetOne()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        UICustomizationDto dto = null;

        try
        {
            dto = await Repo.GetAsync(userId);
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
    /// Updates a single record
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <param name="dto">Dto to update</param>
    /// <returns>Single record</returns>
    [HttpPut]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public async Task<ActionResult<UICustomizationDto>> UpdateOne(UICustomizationDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        try
        {
            await Repo.UpdateAsync(userId, dto);
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
