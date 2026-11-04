// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Reflection;
using API.ProcureAccess.Swagger;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using NSubstitute;
using Shouldly;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace ProcureAccess.Tests.Unit.Swagger;

/// <summary>
/// The Swagger operation filter (#73): cases Swashbuckle does not generate for today's API must
/// neither throw nor be skipped without a trace.
/// </summary>
public sealed class SwaggerDefaultValuesTests
{
    private const string Json = "application/json";

    private readonly ILogger<SwaggerDefaultValues> _logger = Substitute.For<ILogger<SwaggerDefaultValues>>();

    [Fact]
    public void Apply_ResponseTypeWithoutGeneratedResponse_LeavesOperationUnchanged()
    {
        var description = Description(responseTypes: [ResponseType(404, Json)]);
        var operation = new OpenApiOperation { Responses = new OpenApiResponses { ["200"] = Response(Json) } };

        Should.NotThrow(() => Filter().Apply(operation, Context(description)));

        operation.Responses.Keys.ShouldBe(["200"]);
    }

    [Fact]
    public void Apply_ContentTypesTheActionDoesNotProduce_RemovesOnlyThose()
    {
        var description = Description(responseTypes: [ResponseType(200, Json)]);
        var response = Response("text/plain", Json, "text/json");
        var operation = new OpenApiOperation { Responses = new OpenApiResponses { ["200"] = response } };

        Filter().Apply(operation, Context(description));

        response.Content!.Keys.ShouldBe([Json]);
    }

    [Fact]
    public void Apply_DescribedParameterWithDefault_SetsDefaultAndRequired()
    {
        var description = Description(parameters: [Parameter("page", defaultValue: 1, isRequired: true)]);
        var schema = new OpenApiSchema();
        var parameter = new OpenApiParameter { Name = "page", Schema = schema };
        var operation = new OpenApiOperation { Parameters = [parameter] };

        Filter().Apply(operation, Context(description));

        schema.Default!.GetValue<int>().ShouldBe(1);
        parameter.Required.ShouldBeTrue();
        WarningCount().ShouldBe(0);
    }

    [Fact]
    public void Apply_ParameterWithoutApiExplorerDescription_LeavesItUnchangedAndWarnsOncePerFilter()
    {
        var description = Description();
        var parameter = new OpenApiParameter { Name = "unknown" };
        var operation = new OpenApiOperation { Parameters = [parameter] };
        var filter = Filter();

        filter.Apply(operation, Context(description));
        filter.Apply(operation, Context(description));

        parameter.Required.ShouldBeFalse();
        parameter.Description.ShouldBeNull();
        WarningCount().ShouldBe(1);
    }

    [Fact]
    public void Apply_ParameterReference_LeavesItUnchangedAndWarns()
    {
        var description = Description(parameters: [Parameter("shared", defaultValue: 1, isRequired: true)]);
        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Parameters = new Dictionary<string, IOpenApiParameter> { ["shared"] = new OpenApiParameter { Name = "shared" } }
            }
        };
        var operation = new OpenApiOperation { Parameters = [new OpenApiParameterReference("shared", document)] };

        Should.NotThrow(() => Filter().Apply(operation, Context(description)));

        WarningCount().ShouldBe(1);
    }

    [Fact]
    public void Apply_ParameterWithSchemaReference_KeepsSchemaAndWarns()
    {
        var description = Description(parameters: [Parameter("filter", defaultValue: 1, isRequired: false)]);
        var schemaReference = new OpenApiSchemaReference("Filter", new OpenApiDocument());
        var operation = new OpenApiOperation
        {
            Parameters = [new OpenApiParameter { Name = "filter", Schema = schemaReference }]
        };

        Should.NotThrow(() => Filter().Apply(operation, Context(description)));

        operation.Parameters[0].Schema.ShouldBeSameAs(schemaReference);
        WarningCount().ShouldBe(1);
    }

    private SwaggerDefaultValues Filter() => new(_logger);

    // ILogger.Log<TState> is generic over an internal state type, so count the calls instead of
    // matching them with Received() (as in UsageEventWriterTests).
    private int WarningCount() => _logger.ReceivedCalls()
        .Count(call => call.GetMethodInfo().Name == nameof(ILogger.Log)
            && call.GetArguments()[0] is LogLevel.Warning);

    private static ApiDescription Description(
        ApiResponseType[]? responseTypes = null,
        ApiParameterDescription[]? parameters = null)
    {
        var description = new ApiDescription
        {
            ActionDescriptor = new ActionDescriptor(),
            HttpMethod = "GET",
            RelativePath = "api/test",
        };
        foreach (var responseType in responseTypes ?? [])
        {
            description.SupportedResponseTypes.Add(responseType);
        }
        foreach (var parameter in parameters ?? [])
        {
            description.ParameterDescriptions.Add(parameter);
        }
        return description;
    }

    private static ApiResponseType ResponseType(int statusCode, params string[] mediaTypes)
    {
        var responseType = new ApiResponseType { StatusCode = statusCode };
        foreach (var mediaType in mediaTypes)
        {
            responseType.ApiResponseFormats.Add(new ApiResponseFormat { MediaType = mediaType });
        }
        return responseType;
    }

    private static OpenApiResponse Response(params string[] mediaTypes) => new()
    {
        Content = mediaTypes.ToDictionary(mediaType => mediaType, _ => new OpenApiMediaType())
    };

    private static ApiParameterDescription Parameter(string name, object defaultValue, bool isRequired) => new()
    {
        Name = name,
        DefaultValue = defaultValue,
        IsRequired = isRequired,
        ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(defaultValue.GetType()),
    };

    private static OperationFilterContext Context(ApiDescription description) => new(
        description,
        Substitute.For<ISchemaGenerator>(),
        new SchemaRepository(),
        new OpenApiDocument(),
        typeof(SwaggerDefaultValuesTests).GetMethod(nameof(Context), BindingFlags.NonPublic | BindingFlags.Static)!);
}
