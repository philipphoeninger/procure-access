// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Controllers.Identity;

/// <summary>
/// Legacy identity endpoints of the JWT client (removed with S5-10). The anonymous POSTs carry
/// <c>DisableAntiforgery()</c>: the legacy client has no token cookie, and a request without a
/// session cannot be forged into one (S5-07, owner decision Q1 (A)); the SEC-02 test lists them.
/// </summary>
public static class IdentityUserEndpoints
{
    public static IEndpointRouteBuilder MapIdentityUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me", async (
            IUserService service) =>
        {
            return await service.GetCurrentUserAsync();
        });

        app.MapPost("/signIn", async (
            IUserService service,
            LoginRequest request) =>
        {
            var result = await service.SignInAsync(request);

            return result == null
                ? Results.BadRequest(new
                {
                    code = "InvalidLogin"
                })
                : Results.Ok(result);
        }).DisableAntiforgery();

        app.MapPost("/signUp", async (
            IUserService service,
            RegistrationRequest request) =>
        {
            var result = await service.SignUpAsync(request);

            return result.Succeeded
                ? Results.Ok(result)
                : Results.BadRequest(new
                {
                    code = "SignUpFailure",
                    message = result.Errors
                });
        }).DisableAntiforgery();

        app.MapPost("/forgot-password", async (
            IUserService service,
            ForgotPWRequest request) =>
        {
            await service.RequestPasswordResetAsync("ok@testing.lettermint.co"); //request.Email
            return Results.Ok();
        }).DisableAntiforgery();

        app.MapPost("/reset-password", async (
            IUserService service,
            ResetPasswordDto dto) =>
        {
            var result = await service.ResetPasswordAsync(dto);

            return result.Succeeded
                ? Results.Ok(result)
                : Results.BadRequest(result.Errors);
        }).DisableAntiforgery();

        app.MapPost("/refresh", async (
            IUserService service,
            string refreshToken) =>
        {
            var result = await service.RefreshTokenAsync(refreshToken);

            return result == null
                ? Results.BadRequest(new
                {
                    code = "InvalidRefreshToken"
                })
                : Results.Ok(result);
        }).DisableAntiforgery();

        app.MapGet("/confirm-email", async (
            IUserService service,
            string userId,
            string token) =>
        {
            var result = await service.ConfirmEmailAsync(userId, token);

            return result.Succeeded
                ? Results.Ok("Email confirmed")
                : Results.BadRequest(new
                {
                    code = "EmailConfirmFailure",
                    message = result.Errors
                });
        });

        return app;
    }
}
