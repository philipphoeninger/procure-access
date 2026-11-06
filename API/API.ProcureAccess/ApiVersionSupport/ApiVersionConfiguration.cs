// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.ApiVersionSupport;

public static class ApiVersionConfiguration
{
    public static IServiceCollection AddProcureAccessApiVersionConfiguration(this IServiceCollection services, ApiVersion? defaultVersion = null)
    {
        defaultVersion ??= ApiVersion.Default;

        services.AddApiVersioning(
            options =>
            {
                //Set default version
                options.DefaultApiVersion = defaultVersion;
                options.AssumeDefaultVersionWhenUnspecified = true;
                //options.UseApiBehavior = true;

                //reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                options.ReportApiVersions = true;

                // The version is read from the URL segment only (/api/v1/..., ARCHITECTURE §7.1, #153).
                // The query, header and media-type readers made Swagger list the parameters
                // api-version and v twice on every operation. Unversioned legacy routes (/api/...)
                // fall back to the default version through AssumeDefaultVersionWhenUnspecified.
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            // Asp.Versioning 10 expects MVC (controller) versioning to be registered explicitly (AV0013).
            .AddMvc()
            .AddApiExplorer(
            options =>
            {
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }
}
