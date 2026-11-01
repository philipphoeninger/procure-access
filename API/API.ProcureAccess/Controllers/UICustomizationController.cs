// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UICustomizationController : ControllerBase
{
    protected readonly IUICustomizationRepo Repo;
    protected readonly UserManager<User> UserManager;
    protected readonly IAppLogging<UICustomizationController> Logger;

    public UICustomizationController(
        IAppLogging<UICustomizationController> logger,
        IUICustomizationRepo repo,
        UserManager<User> userManager)
    {
        Repo = repo;
        UserManager = userManager;
        Logger = logger;
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
        catch (Exception ex)
        {
            return Failure(ex, "The UI settings could not be read.");
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
        catch (Exception ex)
        {
            return Failure(ex, "The UI settings could not be updated.");
        }
        return Ok(dto);
    }

    /// <summary>
    /// A failed read or write as 400 Problem Details (TECH_DEBT_v1 §5, S2-10). The exception is
    /// logged and its text never reaches the client. Legacy: preferences are rebuilt in step 3.
    /// </summary>
    private ObjectResult Failure(Exception exception, string detail)
    {
        Logger.LogAppError(exception, detail);
        return Problem(statusCode: StatusCodes.Status400BadRequest, detail: detail);
    }
}
