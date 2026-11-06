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
    /// O-S3-2, #183): elsewhere, staging included, <c>/swagger</c> answers 404. Call it after every
    /// <c>Map…</c> call (#176).
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
            // One entry per API version of the controllers and of the minimal APIs mapped so far:
            // versioned minimal APIs must be mapped before this method is called (#176), or the UI
            // misses their document (SwaggerDocumentTests).
            foreach (var description in app.DescribeApiVersions())
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
        });
        return app;
    }
}
