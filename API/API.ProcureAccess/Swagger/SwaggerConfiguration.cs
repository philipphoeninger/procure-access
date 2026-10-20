// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Swagger;

public static class SwaggerConfiguration
{
    public static void AddAndConfigureSwagger(
        this IServiceCollection services,
        IConfiguration config,
        string xmlPathAndFile,
        bool addBasicSecurity)
    {
        services.Configure<SwaggerApplicationSettings>(config.GetSection(nameof(SwaggerApplicationSettings)));
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();
            options.OperationFilter<SwaggerDefaultValues>();
            options.IncludeXmlComments(xmlPathAndFile);
            if (!addBasicSecurity)
            {
                return;
            }
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });
    }
}
