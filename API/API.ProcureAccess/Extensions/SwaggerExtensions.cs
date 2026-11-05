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

    /// <summary>
    /// Serves the Swagger UI and the OpenAPI documents in <c>Development</c> only (owner decision
    /// O-S3-2, #183): elsewhere, staging included, <c>/swagger</c> answers 404.
    /// </summary>
    public static WebApplication ConfigureSwaggerExplorer(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            using var scope = app.Services.CreateScope();
            var versionProvider = scope.ServiceProvider.GetRequiredService<IApiVersionDescriptionProvider>();
            // build a swagger endpoint for each discovered API version
            foreach (var description in versionProvider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
        });
        return app;
    }
}
