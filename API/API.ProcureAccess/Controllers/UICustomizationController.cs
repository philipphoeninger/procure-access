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
        UserManager<User> userManager
        )
    {
        Repo = repo;
        UserManager = userManager;
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
    public async Task<IActionResult> UpdateOne(UpdateUICustomizationDto dto)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!ModelState.IsValid) return ValidationProblem(ModelState);
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
