namespace SERVICES.ProcureAccess.DataServices.Configuration;

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
        services.AddScoped<IProductTypeRepo, ProductTypeRepo>();
        services.AddScoped<IUICustomizationRepo, UICustomizationRepo>();
        services.AddScoped<IUserRepo, UserRepo>();
        
        return services;
    }

    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        // Add DataServices:
        services.AddScoped<ICriteriaFilterService, CriteriaFilterService>();
        services.AddScoped<ICriterionService, CriterionService>();
        services.AddScoped<IFilterTypeService, FilterTypeService>();
        services.AddScoped<IProductPartService, ProductPartService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductTestService, ProductTestService>();
        services.AddScoped<IProductTypeService, ProductTypeService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        
        return services;
    }
}
