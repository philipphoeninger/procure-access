// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Swagger;

/// <summary>
/// Completes each operation with what the API explorer knows: deprecation, the response content
/// types an action really produces, and parameter descriptions, defaults and required flags.
/// </summary>
/// <remarks>
/// Microsoft.OpenApi 2.x exposes parameters and schemas as interfaces. Only the generated objects
/// (<see cref="OpenApiParameter"/>, <see cref="OpenApiSchema"/>) are writable; references point to
/// shared components, and OpenAPI 3.0 ignores every property next to a <c>$ref</c>. Such
/// parameters and schemas are left unchanged and logged, so a missing value can be traced.
/// </remarks>
public class SwaggerDefaultValues(ILogger<SwaggerDefaultValues> logger) : IOperationFilter
{
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
            var description = apiDescription.ParameterDescriptions.FirstOrDefault(p => p.Name == parameter.Name);
            if (description is null)
            {
                continue;
            }

            if (parameter is not OpenApiParameter writableParameter)
            {
                logger.LogWarning(
                    "Swagger: parameter {Parameter} of {Path} is a reference; description, default and required flag are not applied.",
                    parameter.Name,
                    apiDescription.RelativePath);
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
            else if (writableParameter.Schema is not null)
            {
                logger.LogWarning(
                    "Swagger: the schema of parameter {Parameter} of {Path} is a reference; its default value is not applied.",
                    parameter.Name,
                    apiDescription.RelativePath);
            }
        }
    }
}
