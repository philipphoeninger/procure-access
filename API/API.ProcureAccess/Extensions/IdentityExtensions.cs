// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Extensions;

public static class IdentityExtensions
{
    /// <summary>HMAC-SHA256 needs a key of at least 256 bits; shorter keys fail at the first sign-in.</summary>
    public const int MinimumJwtKeyBytes = 32;

    /// <summary>
    /// The default authentication scheme: a policy scheme that forwards to the legacy JWT handler
    /// when the request carries a Bearer header and to the Identity cookie otherwise (S5-07;
    /// S5-08 removes the JWT and with it this scheme).
    /// </summary>
    public const string CookieOrBearerScheme = "CookieOrBearer";

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
    public static IServiceCollection AddIdentityAuth(this IServiceCollection services, IConfiguration config)
    {
        // Checked at startup like the connection string, so a missing or placeholder key stops
        // the API instead of failing every sign-in (legacy until migration step 3, roadmap M3.1).
        string? jwtKey = config["JWTSettings:Key"];
        if (jwtKey is null || Encoding.UTF8.GetByteCount(jwtKey) < MinimumJwtKeyBytes)
        {
            throw new InvalidOperationException(
                $"JWTSettings:Key must be at least {MinimumJwtKeyBytes} bytes. " +
                "Set the environment variable JWTSettings__Key (JWT_KEY in .env), e.g. openssl rand -base64 64.");
        }

        // Cookie session next to the legacy JWT for one PR (S5-07, owner decision Q1 (A)): the
        // default scheme picks the JWT handler for a request with a Bearer header and the Identity
        // cookie (registered by the IdentityAccess module) for every other one. S5-08 moves the
        // client to the cookie and removes the JWT with this selector.
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
            options.DefaultChallengeScheme =
            options.DefaultScheme = CookieOrBearerScheme;
        })
        .AddPolicyScheme(CookieOrBearerScheme, "Identity cookie, or the legacy JWT when a Bearer header is sent", options =>
        {
            options.ForwardDefaultSelector = context =>
                context.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? JwtBearerDefaults.AuthenticationScheme
                    : IdentityConstants.ApplicationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["JWTSettings:Issuer"],
                ValidAudience = config["JWTSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.FromMinutes(2)
            };
        });
        services.AddAuthorization(options =>
        {
            // No scheme named: [Authorize] takes the principal of the default scheme above, so the
            // cookie and the legacy JWT both satisfy it (S5-07).
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
