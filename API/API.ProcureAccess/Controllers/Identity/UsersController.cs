// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

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

    [HttpPost("change-email")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(400, "The request was invalid")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    public async Task<IActionResult> ChangeEmailAsync(ChangeEmailRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await Repo.ChangeEmailAsync(
            userId,
            request.NewEmail);

        if (!result.Succeeded)
            return BadRequest(new
            {
                code = "ChangeEmailFailure",
                message = result.Errors
            });

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
    public async Task<IActionResult> ChangePasswordAsync(ChangePasswordRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await Repo.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(new
            {
                code = "ChangePasswordFailure",
                message = result.Errors
            });

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
    public async Task<IActionResult> ResetPasswordAsync(ResetPWRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await Repo.ResetPasswordAsync(
            request.Email,
            request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(new
            {
                code = "ResetPasswordFailure",
                message = result.Errors
            });

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
    public async Task<IActionResult> RevokeSessionsAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await Repo.RevokeSessionsAsync(userId);

        if (!result.Succeeded)
            return BadRequest(new
            {
                code = "RevokeSessionsFailure",
                message = result.Errors
            });

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
    public async Task<IActionResult> DeleteUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await Repo.DeleteUserAsync(userId);

        if (!result.Succeeded)
            return BadRequest(new
            {
                code = "UserDeleteFailure",
                message = result.Errors
            });

        return Ok();
    }
}
