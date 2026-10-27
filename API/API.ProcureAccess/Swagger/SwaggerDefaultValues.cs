// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;

namespace API.ProcureAccess.Swagger;

/// <summary>
/// Completes each operation with what the API explorer knows: deprecation, the response content
/// types an action really produces, and parameter descriptions, defaults and required flags.
/// </summary>
/// <remarks>
/// Microsoft.OpenApi 2.x exposes parameters and schemas as interfaces. Only the generated objects
/// (<see cref="OpenApiParameter"/>, <see cref="OpenApiSchema"/>) are writable; references point to
/// shared components, and OpenAPI 3.0 ignores every property next to a <c>$ref</c>. Such
/// parameters and schemas, and parameters the API explorer does not describe, are left unchanged
/// and logged as a warning, so a missing value can be traced. Swashbuckle generates the document
/// on every request, so each case is logged once per process.
/// </remarks>
public class SwaggerDefaultValues(ILogger<SwaggerDefaultValues> logger) : IOperationFilter
{
    private static readonly ConcurrentDictionary<string, byte> Reported = new();

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;
        operation.Deprecated |= apiDescription.IsDeprecated;

        if (operation.Responses is not null)
        {
            foreach (var responseType in apiDescription.SupportedResponseTypes)
            {
                var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
                if (!operation.Responses.TryGetValue(responseKey, out var response) || response.Content is null)
                {
                    continue;
                }

                // copy the keys: the content map is changed inside the loop
                foreach (var contentType in response.Content.Keys.ToList())
                {
                    if (responseType.ApiResponseFormats.All(x => x.MediaType != contentType))
                    {
                        response.Content.Remove(contentType);
                    }
                }
            }
        }

        if (operation.Parameters is null)
        {
            return;
        }

        foreach (var parameter in operation.Parameters)
        {
            var path = $"{apiDescription.HttpMethod} /{apiDescription.RelativePath}";
            var description = apiDescription.ParameterDescriptions.FirstOrDefault(p => p.Name == parameter.Name);
            if (description is null)
            {
                if (FirstReport("undescribed", path, parameter.Name))
                {
                    logger.LogWarning(
                        "Swagger: parameter {Parameter} of {Path} has no API explorer description; description, default and required flag are not applied.",
                        parameter.Name,
                        path);
                }
                continue;
            }

            if (parameter is not OpenApiParameter writableParameter)
            {
                if (FirstReport("parameter-reference", path, parameter.Name))
                {
                    logger.LogWarning(
                        "Swagger: parameter {Parameter} of {Path} is a reference; description, default and required flag are not applied.",
                        parameter.Name,
                        path);
                }
                continue;
            }

            writableParameter.Description ??= description.ModelMetadata?.Description;
            writableParameter.Required |= description.IsRequired;

            if (description.DefaultValue is null || description.DefaultValue == DBNull.Value || description.ModelMetadata is null)
            {
                continue;
            }

            if (writableParameter.Schema is OpenApiSchema schema)
            {
                if (schema.Default is null)
                {
                    var options = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.Preserve
                    };
                    var json = JsonSerializer.Serialize(description.DefaultValue, description.ModelMetadata.ModelType, options);
                    schema.Default = JsonNode.Parse(json);
                }
            }
            else if (writableParameter.Schema is not null && FirstReport("schema-reference", path, parameter.Name))
            {
                logger.LogWarning(
                    "Swagger: the schema of parameter {Parameter} of {Path} is a reference; its default value is not applied.",
                    parameter.Name,
                    path);
            }
        }
    }

    private static bool FirstReport(string reason, string path, string? parameter) =>
        Reported.TryAdd($"{reason}|{path}|{parameter}", 0);
}
