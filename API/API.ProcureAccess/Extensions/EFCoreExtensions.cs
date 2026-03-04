namespace API.ProcureAccess.Extensions;

public static class EFCoreExtensions
{
    public static IServiceCollection AddSqlServerConnection(this IServiceCollection services, IConfiguration config)
    {
        string? connectionString = @"Server=db;Database=ProcureAccessDb;User Id=sa;Password=YourStrongPassw0rd_h3r3;Encrypt=False;TrustServerCertificate=True;";//config.GetConnectionString("ProcureAccess");
        services.AddSqlServer<ApplicationDBContext>(connectionString, options =>
        {
            options.EnableRetryOnFailure().CommandTimeout(60);
        });
        return services;
    }
}
