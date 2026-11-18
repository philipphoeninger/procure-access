// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Extensions;

public static class IdentityExtensions
{
    // The user and role stores, the sign-in manager and the password and lockout rules are
    // registered by the IdentityAccess module since S6-10 (AddIdentityAccessModule).

    // Auth = Authentication + Authorization
    /// <summary>
    /// The Identity application cookie (registered by the IdentityAccess module, S5-07) is the only
    /// authentication scheme since S5-08 removed the legacy JWT (ARCHITECTURE §9.1, ADR-006;
    /// SEC-01): there is no token in JavaScript and no Bearer header.
    /// </summary>
    public static IServiceCollection AddIdentityAuth(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
            options.DefaultChallengeScheme =
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
        });
        services.AddAuthorization(options =>
        {
            // No scheme named: [Authorize] takes the principal of the default scheme above.
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                                            .RequireAuthenticatedUser()
                                            .Build();

            // ensures [AllowAnonymous] is respected properly
            options.FallbackPolicy = null;
        });
        return services;
    }

    public static WebApplication AddIdentityAuthMiddlewares(this WebApplication app)
    {
        app.UseAuthentication();
        // The memberships of the signed-in user, loaded once per request between authentication
        // and authorization (S5-15, ARCHITECTURE §5.4): the organisation query filters and the
        // role requirements both read them.
        app.UseIdentityAccessOrganizationContext();
        app.UseAuthorization();
        return app;
    }
}
