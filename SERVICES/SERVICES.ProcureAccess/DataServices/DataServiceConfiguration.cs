namespace SERVICES.ProcureAccess.DataServices;

public static class DataServiceConfiguration
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Add Repos:
        services.AddScoped<IFilterTypeRepo, FilterTypeRepo>();
        services.AddScoped<ICriterionRepo, CriterionRepo>();
        services.AddScoped<ICriteriaFilterRepo, CriteriaFilterRepo>();
        services.AddScoped<IProductRepo, ProductRepo>();
        services.AddScoped<IProductPartRepo, ProductPartRepo>();
        services.AddScoped<IProductTestRepo, ProductTestRepo>();
        services.AddScoped<IUICustomizationRepo, UICustomizationRepo>();
        services.AddScoped<IUserRepo, UserRepo>();
        
        return services;
    }
}
