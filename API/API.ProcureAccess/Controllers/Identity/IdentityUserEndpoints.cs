namespace API.ProcureAccess.Controllers.Identity;

public static class IdentityUserEndpoints
{
    public static IEndpointRouteBuilder MapIdentityUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/signIn", async (
            IUserService service,
            LoginRequest request) =>
        {
            var result = await service.SignInAsync(request);

            return result == null
                ? Results.BadRequest("Invalid login")
                : Results.Ok(result);
        });

        app.MapPost("/signUp", async (
            IUserService service,
            RegistrationRequest request) =>
        {
            var result = await service.SignUpAsync(request);

            return result.Succeeded
                ? Results.Ok(result)
                : Results.BadRequest(result.Errors);
        });

        app.MapPost("/forgot-password", async (
            IUserService service,
            ForgotPWRequest request) =>
        {
            await service.RequestPasswordResetAsync(request.Email);
            return Results.Ok();
        });

        app.MapPost("/reset-password", async (
            IUserService service,
            ResetPasswordDto dto) =>
        {
            var result = await service.ResetPasswordAsync(dto);

            return result.Succeeded
                ? Results.Ok(result)
                : Results.BadRequest(result.Errors);
        });

        app.MapPost("/refresh", async (
            IUserService service,
            string refreshToken) =>
        {
            var result = await service.RefreshTokenAsync(refreshToken);

            return result == null
                ? Results.BadRequest("Invalid refresh token")
                : Results.Ok(result);
        });

        app.MapGet("/confirm-email", async (
            IUserService service,
            string userId,
            string token) =>
        {
            var result = await service.ConfirmEmailAsync(userId, token);

            return result.Succeeded
                ? Results.Ok("Email confirmed")
                : Results.BadRequest(result.Errors);
        });

        return app;
    }
}
