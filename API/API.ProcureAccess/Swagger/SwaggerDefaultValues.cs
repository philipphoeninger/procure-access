// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Swagger;

public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;
        operation.Deprecated |= apiDescription.IsDeprecated;
        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
            var response = operation.Responses![responseKey];
            if (response.Content is null) continue;
            foreach (var contentType in response.Content.Keys)
            {
                if (responseType.ApiResponseFormats.All(x => x.MediaType != contentType))
                {
                    response.Content.Remove(contentType);
                }
            }
        }

        if (operation.Parameters == null) return;

        // Microsoft.OpenApi 2.x exposes parameters and schemas as read-only interfaces;
        // the generated (non-reference) objects are the concrete, writable types.
        foreach (var parameter in operation.Parameters.OfType<OpenApiParameter>())
        {
            var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
            parameter.Description ??= description.ModelMetadata?.Description;
            if (parameter.Schema is OpenApiSchema schema && schema.Default == null && description.DefaultValue != null && description.ModelMetadata != null)
            {
                if (description.DefaultValue != DBNull.Value)
                {
                    var options = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.Preserve
                    };
                    var json = JsonSerializer.Serialize(description.DefaultValue, description.ModelMetadata.ModelType, options);
                    schema.Default = JsonNode.Parse(json);
                }
            }
            parameter.Required |= description.IsRequired;
        }
    }
}
