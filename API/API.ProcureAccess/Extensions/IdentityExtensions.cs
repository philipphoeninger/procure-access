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
                .AddDefaultTokenProviders()
                .AddPasswordValidator<MaximumLengthPasswordValidator>();
        return services;
    }

    /// <summary>
    /// Unique e-mail addresses and the password rule of <see cref="PasswordPolicy"/> (S5-12a): a
    /// 🔧 minimum length, no composition rules. The user name is the e-mail address, so it may hold
    /// every character an address may (the default allows only letters, digits and <c>-._@+</c>).
    /// </summary>
    public static IServiceCollection ConfigureIdentityOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var minLength = PasswordPolicy.MinLengthFrom(configuration);
        services.Configure<IdentityOptions>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "";
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = minLength;
            options.Password.RequiredUniqueChars = 1;
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
