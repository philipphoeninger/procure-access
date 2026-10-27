// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Extensions;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerExplorer(this IServiceCollection services, IConfiguration config)
    {
        services.AddEndpointsApiExplorer();
        services.AddAndConfigureSwagger(
            config,
            Path.Combine(
                AppContext.BaseDirectory,
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"),
            true);
        return services;
    }

    public static WebApplication ConfigureSwaggerExplorer(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            // build a swagger endpoint for each API version of the mapped endpoints (controllers and
            // minimal APIs); the endpoints must be mapped before this method is called
            foreach (var description in app.DescribeApiVersions())
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
        });
        return app;
    }
}
