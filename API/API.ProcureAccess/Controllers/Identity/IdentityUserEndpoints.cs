// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Controllers.Identity;

/// <summary>
/// Legacy identity endpoints (removed with S5-10, replaced by S5-12 and S5-13). <c>/signIn</c> and
/// <c>/refresh</c> went with the JWT in S5-08; sign-in is <c>POST /api/v1/auth/login</c>. The
/// remaining anonymous POSTs carry <c>DisableAntiforgery()</c>: a request without a session cannot
/// be forged into one (S5-07, owner decision Q1 (A)); the SEC-02 test lists them.
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
