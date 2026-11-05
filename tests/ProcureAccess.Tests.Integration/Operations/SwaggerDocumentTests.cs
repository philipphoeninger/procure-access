// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json.Nodes;
using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// Swagger UI and the OpenAPI document of the running API. They are served in <c>Development</c>
/// only (owner decision O-S3-2, #183), so every test starts its own API in the environment it
/// needs, on a database of its own: <c>Development</c> may rebuild the database (SEC-06).
/// In <c>Development</c> the XML documentation is found in the build output (#74) and
/// <c>SwaggerDefaultValues</c> completes the parameters (#73).
/// </summary>
public sealed class SwaggerDocumentTests(ApiFixture api) : IAsyncDisposable
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
    public async Task SwaggerDocument_V1_CarriesXmlCommentsAndParameterDefaults()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = CreateFactory("Development");
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(DocumentPath, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var document = JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken))!;
        var operation = document["paths"]!["/api/Criteria/{id}"]!["get"]!;
        operation["summary"]!.GetValue<string>().ShouldBe("Gets a single record");
        var parameters = operation["parameters"]!.AsArray();
        Parameter(parameters, "id")["description"]!.GetValue<string>().ShouldBe("Primary key of the record");
        Parameter(parameters, "api-version")["schema"]!["default"]!.GetValue<string>().ShouldBe("1.0");
    }

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
