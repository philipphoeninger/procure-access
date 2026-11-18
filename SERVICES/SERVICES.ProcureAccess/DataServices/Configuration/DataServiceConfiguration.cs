// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

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

        return services;
    }

    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        // Add DataServices:
        services.AddScoped<ICriteriaFilterService, CriteriaFilterService>();
        services.AddScoped<ICriterionService, CriterionService>();
        services.AddScoped<IFilterTypeService, FilterTypeService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
