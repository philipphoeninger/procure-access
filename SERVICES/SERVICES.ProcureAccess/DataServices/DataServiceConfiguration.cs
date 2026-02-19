namespace SERVICES.ProcureAccess.DataServices;

public static class DataServiceConfiguration
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Add Repos:
        // services.AddScoped<ISomeRepo, SomeRepo>();
        services.AddScoped<IFilterTypeRepo, FilterTypeRepo>();
        services.AddScoped<ICriterionRepo, CriterionRepo>();
        services.AddScoped<ICriteriaFilterRepo, CriteriaFilterRepo>();
        services.AddScoped<IProductRepo, ProductRepo>();
        
        return services;
    }
}
