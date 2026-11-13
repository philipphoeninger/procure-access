// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddIdentityHandlersAndStores(this IServiceCollection services)
    {
        services.AddIdentityCore<User>(options =>
                    {
                        options.SignIn.RequireConfirmedEmail = false;

                        options.Lockout.MaxFailedAccessAttempts = 5;
                        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDBContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
        return services;
    }

    public static IServiceCollection ConfigureIdentityOptions(this IServiceCollection services)
    {
        services.Configure<IdentityOptions>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 2;
        });
        return services;
    }

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
        app.UseAuthorization();
        return app;
    }
}
