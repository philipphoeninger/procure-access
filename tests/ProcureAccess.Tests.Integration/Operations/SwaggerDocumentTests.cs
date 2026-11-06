// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// Swagger UI and the OpenAPI document of the running API. They are served in <c>Development</c>
/// only (owner decision O-S3-2, #183), so every test starts its own API in the environment it
/// needs, on a database of its own: <c>Development</c> may rebuild the database (SEC-06).
/// In <c>Development</c> the XML documentation is found in the build output (#74) and
/// <c>SwaggerDefaultValues</c> completes the parameters (#73). Since S3-19 the API version is read
/// from the URL segment only, so no operation lists <c>api-version</c> or <c>v</c> (#153), and the
/// UI lists the documents of controllers and versioned minimal APIs alike (#176).
/// </summary>
public sealed partial class SwaggerDocumentTests(ApiFixture api) : IAsyncDisposable
{
    private const string UiPath = "/swagger/index.html";
    private const string DocumentPath = "/swagger/v1/swagger.json";

    private readonly string _connectionString = api.ConnectionStringForNewDatabase();

    [Theory]
    [InlineData(ProcureAccessApiFactory.EnvironmentName)]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task Swagger_OutsideDevelopment_AnswersNotFound(string environment)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = CreateFactory(environment);
        using var client = factory.CreateClient();

        using var ui = await client.GetAsync(UiPath, cancellationToken);
        using var document = await client.GetAsync(DocumentPath, cancellationToken);

        ui.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        document.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Control for the theory above: the same requests in <c>Development</c> find both, so the
    /// theory cannot pass merely because the paths are wrong.
    /// </summary>
    [Fact]
    public async Task Swagger_InDevelopment_ServesUiAndDocument()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = CreateFactory("Development");
        using var client = factory.CreateClient();

        using var ui = await client.GetAsync(UiPath, cancellationToken);
        using var document = await client.GetAsync(DocumentPath, cancellationToken);

        ui.StatusCode.ShouldBe(HttpStatusCode.OK);
        ui.Content.Headers.ContentType?.MediaType.ShouldBe("text/html");
        document.StatusCode.ShouldBe(HttpStatusCode.OK);
        document.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }

    [Fact]
    public async Task SwaggerDocument_V1_CarriesXmlCommentsAndNoQueryOrHeaderVersion()
    {
        var document = await ReadDocumentAsync();

        var operation = document["paths"]!["/api/Criteria/{id}"]!["get"]!;
        operation["summary"]!.GetValue<string>().ShouldBe("Gets a single record");
        Parameter(operation["parameters"]!.AsArray(), "id")["description"]!.GetValue<string>().ShouldBe("Primary key of the record");

        // #153: the query, header and media-type version readers listed api-version and v twice.
        var parameters = Operations(document)
            .SelectMany(op => (op.Operation["parameters"]?.AsArray() ?? []).Select(p => (
                op.Name,
                Parameter: $"{p!["in"]!.GetValue<string>()}:{p["name"]!.GetValue<string>()}")))
            .ToList();
        parameters.ShouldNotBeEmpty();
        parameters.Select(p => p.Parameter).ShouldNotContain(p => p.EndsWith(":api-version", StringComparison.Ordinal) || p.EndsWith(":v", StringComparison.Ordinal));
        parameters.GroupBy(p => p).Where(g => g.Count() > 1).Select(g => g.Key).ShouldBeEmpty();
    }

    [Fact]
    public async Task SwaggerDocument_V1_DescribesTheFilterOptionsEndpoint()
    {
        var document = await ReadDocumentAsync();

        var operation = document["paths"]!["/api/v1/catalog/filter-options"]!["get"]!;
        operation["tags"]!.AsArray().Select(t => t!.GetValue<string>()).ShouldBe(["Catalog"]);
        operation["parameters"]!.AsArray()
            .Select(p => $"{p!["in"]!.GetValue<string>()}:{p["name"]!.GetValue<string>()}")
            .ShouldBe(["query:release", "query:useCase", "query:appTypeIds[]", "query:productPartIds[]"]);
        operation["responses"]!.AsObject().Select(r => r.Key).ShouldBe(["200", "304", "400", "404"], ignoreOrder: true);
    }

    /// <summary>
    /// #176: the UI lists its documents through <c>app.DescribeApiVersions()</c>, the documents come
    /// from <c>IApiVersionDescriptionProvider</c>. A versioned minimal API mapped after
    /// <c>ConfigureSwaggerExplorer()</c> would get a document but no entry in the UI.
    /// </summary>
    [Fact]
    public async Task SwaggerUi_Urls_MatchTheGeneratedDocuments()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = CreateFactory("Development");
        using var client = factory.CreateClient();
        var documents = factory.Services.GetRequiredService<IOptions<SwaggerGenOptions>>()
            .Value.SwaggerGeneratorOptions.SwaggerDocs.Keys;

        var script = await client.GetStringAsync("/swagger/index.js", cancellationToken);

        var match = ConfigObject().Match(script);
        match.Success.ShouldBeTrue("index.js has no configObject");
        using var config = JsonDocument.Parse(match.Groups["json"].Value);
        var urls = config.RootElement.GetProperty("urls").EnumerateArray()
            .Select(u => u.GetProperty("url").GetString())
            .ToList();
        documents.ShouldNotBeEmpty();
        urls.ShouldBe(documents.Select(d => $"/swagger/{d}/swagger.json"), ignoreOrder: true);
    }

    private async Task<JsonNode> ReadDocumentAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = CreateFactory("Development");
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(DocumentPath, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken))!;
    }

    private static IEnumerable<(string Name, JsonNode Operation)> Operations(JsonNode document) =>
        document["paths"]!.AsObject().SelectMany(path => path.Value!.AsObject()
            .Select(method => ($"{method.Key} {path.Key}", method.Value!)));

    [GeneratedRegex(@"var configObject = (?<json>\{.*\});")]
    private static partial Regex ConfigObject();

    private ProcureAccessApiFactory CreateFactory(string environment) =>
        new(_connectionString, api.Clock, environment);

    private static JsonNode Parameter(JsonArray parameters, string name) =>
        parameters.First(parameter => parameter!["name"]!.GetValue<string>() == name)!;

    public async ValueTask DisposeAsync()
    {
        await using var db = new ApplicationDBContext(new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseNpgsql(_connectionString)
            .Options);
        await db.Database.EnsureDeletedAsync();
    }
}
