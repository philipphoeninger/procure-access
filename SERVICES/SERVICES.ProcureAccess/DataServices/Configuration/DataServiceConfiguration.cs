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
        services.AddScoped<IProductCriteriaFilterRepo, ProductCriteriaFilterRepo>();
        services.AddScoped<IUICustomizationRepo, UICustomizationRepo>();
        services.AddScoped<IUserRepo, UserRepo>();
        services.AddScoped<IProposalRepo, ProposalRepo>();
        services.AddScoped<ICriteriaFilterExclusionRepo, CriteriaFilterExclusionRepo>();
        
        return services;
    }

    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        // Add DataServices:
        services.AddScoped<ICriteriaFilterService, CriteriaFilterService>();
        services.AddScoped<ICriterionService, CriterionService>();
        services.AddScoped<IFilterTypeService, FilterTypeService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductCriteriaFilterService, ProductCriteriaFilterService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IProposalService, ProposalService>();
        services.AddScoped<ICriteriaFilterExclusionService, CriteriaFilterExclusionService>();
        
        return services;
    }
}
