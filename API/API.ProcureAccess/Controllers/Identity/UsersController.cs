// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace API.ProcureAccess.Controllers.Identity;

/// <summary>
/// The legacy account deletion of the profile page (until S61 rebuilds it, ARCHITECTURE §7.1). It
/// needs a signed-in user (S4-03, #205) and answers failures as Problem Details (API-01). S5-10
/// removed <c>change-email</c>, <c>change-password</c> and <c>revoke-sessions</c>, which no screen
/// called (owner decision S5-10 2a); a request to them answers 404 like any unknown route.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController(IUserRepo repo) : ControllerBase
{
    [HttpDelete]
    [ApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponse(200, "The execution was successful")]
    [SwaggerResponse(401, "Unauthorized access attempted")]
    [SwaggerResponse(404, "The signed-in user does not exist (any more)")]
    public async Task<IActionResult> DeleteUserAsync()
    {
        if (CurrentUserId() is not { } userId) return Unauthorized();

        var result = await repo.DeleteUserAsync(userId);

        if (result.Succeeded) return Ok();

        if (result.Errors.Any(e => e.Code == UserRepo.UserNotFoundCode))
            return Problem(statusCode: StatusCodes.Status404NotFound, type: ProblemTypes.NotFound);

        // Anything else is a fault of the server: the exception handler answers 500 Problem
        // Details without the error text.
        throw new InvalidOperationException(
            $"Deleting the account failed: {string.Join(", ", result.Errors.Select(e => e.Code))}");
    }

    /// <summary>
    /// The id of the signed-in user; null when the principal carries none, which counts as not
    /// signed in.
    /// </summary>
    private string? CurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } userId ? userId : null;
}
