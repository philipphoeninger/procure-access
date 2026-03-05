namespace API.ProcureAccess.Extensions;

public static class AppConfigExtensions
{
    public static WebApplication ConfigureCORS(this WebApplication app, IConfiguration config)
    {
        app.UseCors(options => options.WithOrigins("https://procureaccess.tech").AllowAnyMethod().AllowAnyHeader());
        return app;
    }

    public static IServiceCollection AddAppConfig(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JWTSettings>(config.GetSection("JWTSettings"));
        return services;
    }
}
