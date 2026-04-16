namespace API.ProcureAccess.Controllers.Identity;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    protected readonly IAppLogging<UsersController> Logger;
    protected readonly HttpContext? HContext;
    protected readonly IUserRepo Repo;
    protected readonly UserManager<User> UserManager;

    public UsersController(
        IAppLogging<UsersController> logger,
        IHttpContextAccessor hAccess,
        IUserRepo repo,
        UserManager<User> userManager)
    {
        Logger = logger;
        HContext = hAccess.HttpContext;
        Repo = repo;
        UserManager = userManager;
    }

    /// <summary>
    /// Gets a single record
    /// </summary>
    /// <param name="id">Primary key of the record</param>
    /// <returns>Single record</returns>
    // [HttpGet("{id}")]
    // [ApiVersion("1.0")]
    // [Produces("application/json")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    // [SwaggerResponse(200, "The execution was successful")]
    // [SwaggerResponse(204, "No content")]
    // [SwaggerResponse(400, "The request was invalid")]
    // [SwaggerResponse(401, "Unauthorized access attempted")]
    // public async Task<IResult> GetOneAsync(int id)
    // {
    //     ClaimsPrincipal cp = HContext!.User;
    //     string userID = cp.Claims.First(x => x.Type == "UserID").Value;
    //     User? userDetails = await UserManager.FindByIdAsync(userID);

    //     return Results.Ok(
    //         new
    //         {
    //             userDetails?.Email
    //         });
    // }

    [HttpPost("change-email")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await Repo.ChangeEmailAsync(
            userId,
            request.NewEmail);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    [HttpPost("change-password")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await Repo.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    [HttpPost("reset-password")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public async Task<IActionResult> ResetPassword(ResetPWRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await Repo.ResetPasswordAsync(
            request.Email,
            request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    [HttpPost("revoke-sessions")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public async Task<IActionResult> RevokeSessions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await Repo.RevokeSessionsAsync(userId);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("All sessions revoked.");
    }

    [HttpDelete]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public async Task<IActionResult> DeleteUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await Repo.DeleteUserAsync(userId);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }
}
