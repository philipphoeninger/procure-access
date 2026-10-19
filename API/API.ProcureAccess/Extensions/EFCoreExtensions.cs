namespace API.ProcureAccess.Extensions;

public static class EFCoreExtensions
{
    public static IServiceCollection AddPostgreSQLConnection(this IServiceCollection services, IConfiguration config)
    {
        // Set via the environment variable ConnectionStrings__ProcureAccess (see .env.example).
        // Secrets never live in appsettings*.json: the repository is public.
        string? connectionString = config.GetConnectionString("ProcureAccess");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'ProcureAccess' is not configured. " +
                "Set the environment variable ConnectionStrings__ProcureAccess (see .env.example).");
        }

        services.AddNpgsql<ApplicationDBContext>(connectionString, options =>
        {
            options.EnableRetryOnFailure().CommandTimeout(60);
        });
        return services;
    }
}
