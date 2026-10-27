// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Extensions;

public static class AppConfigExtensions
{
    private const string FrontendCorsPolicy = "frontend";

    /// <summary>
    /// Registers the CORS policy for the Angular client. Allowed origins come from
    /// the configuration key <c>Cors:AllowedOrigins</c> (array). No configured origin
    /// means no cross-origin request is allowed.
    /// </summary>
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration config)
    {
        var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
            options.AddPolicy(FrontendCorsPolicy, policy => policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()));
        return services;
    }

    /// <summary>
    /// Applies the CORS policy registered by <see cref="AddFrontendCors"/>.
    /// Must run before authentication so preflight requests are answered.
    /// </summary>
    public static WebApplication ConfigureCORS(this WebApplication app)
    {
        app.UseCors(FrontendCorsPolicy);
        return app;
    }

    public static IServiceCollection AddAppConfig(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JWTSettings>(config.GetSection("JWTSettings"));
        return services;
    }

    public static IServiceCollection AddEmailConfig(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<EmailSettings>(config.GetSection("Email"));
        return services;
    }
}
