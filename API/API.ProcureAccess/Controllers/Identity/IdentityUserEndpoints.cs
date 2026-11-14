// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Controllers.Identity;

/// <summary>
/// Legacy identity endpoints: only <c>GET /api/me</c> is left (until the users move, M3.1).
/// <c>/signIn</c> and <c>/refresh</c> went with the JWT in S5-08 (sign-in is
/// <c>POST /api/v1/auth/login</c>), <c>/signUp</c> and <c>/confirm-email</c> with S5-12b (registration
/// is <c>POST /api/v1/auth/register</c> and <c>/confirm-email</c>), <c>/forgot-password</c> and
/// <c>/reset-password</c> with S5-13b (owner decision S5-13 4 (a); the reset is
/// <c>POST /api/v1/auth/password-reset</c> and <c>/password-reset/complete</c>).
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

        return app;
    }
}
