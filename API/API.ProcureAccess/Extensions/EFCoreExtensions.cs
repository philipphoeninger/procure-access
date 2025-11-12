namespace API.ProcureAccess.Extensions;

public static class EFCoreExtensions
{
    public static IServiceCollection AddSqlServerConnection(this IServiceCollection services, IConfiguration config)
    {
        string? connectionString = config.GetConnectionString("ProcureAccess");
        services.AddSqlServer<ApplicationDBContext>(connectionString, options =>
        {
            options.EnableRetryOnFailure().CommandTimeout(60);
        });
        return services;
    }
}
