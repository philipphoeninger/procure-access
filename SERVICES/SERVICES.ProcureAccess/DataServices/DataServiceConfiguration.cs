namespace SERVICES.ProcureAccess.DataServices;

public static class DataServiceConfiguration
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Add Repos:
        // services.AddScoped<ISomeRepo, SomeRepo>();
        
        return services;
    }
}
