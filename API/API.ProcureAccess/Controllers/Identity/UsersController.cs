// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Controllers.Identity;

/// <summary>
/// Account actions of the signed-in user (legacy until migration step 3, which rebuilds them with
/// the cookie session, ARCHITECTURE §7.1). Every action needs a signed-in user (S4-03, #205);
/// password reset goes through the token-based <c>POST /api/reset-password</c>.
/// </summary>
[ApiController]
[Authorize]
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

        if (CurrentUserId() is not { } userId) return Unauthorized();

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

        if (CurrentUserId() is not { } userId) return Unauthorized();

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
        if (CurrentUserId() is not { } userId) return Unauthorized();

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
        if (CurrentUserId() is not { } userId) return Unauthorized();

        var result = await Repo.DeleteUserAsync(userId);

        if (!result.Succeeded)
            return BadRequest(new
            {
                code = "UserDeleteFailure",
                message = result.Errors
            });

        return Ok();
    }

    /// <summary>
    /// The id of the signed-in user; null when the principal carries none, which counts as not
    /// signed in.
    /// </summary>
    private string? CurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } userId ? userId : null;
}
