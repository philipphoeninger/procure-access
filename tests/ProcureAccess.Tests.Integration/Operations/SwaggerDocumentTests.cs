// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json.Nodes;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// The Swagger document of the running API: the XML documentation is found in the build output
/// (#74) and <c>SwaggerDefaultValues</c> completes the parameters (#73).
/// </summary>
public sealed class SwaggerDocumentTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact]
    public async Task SwaggerDocument_V1_CarriesXmlCommentsAndParameterDefaults()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = CreateClient();

        using var response = await client.GetAsync("/swagger/v1/swagger.json", cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var document = JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken))!;
        var operation = document["paths"]!["/api/Criteria/{id}"]!["get"]!;
        operation["summary"]!.GetValue<string>().ShouldBe("Gets a single record");
        var parameters = operation["parameters"]!.AsArray();
        Parameter(parameters, "id")["description"]!.GetValue<string>().ShouldBe("Primary key of the record");
        Parameter(parameters, "api-version")["schema"]!["default"]!.GetValue<string>().ShouldBe("1.0");
    }

    private static JsonNode Parameter(JsonArray parameters, string name) =>
        parameters.First(parameter => parameter!["name"]!.GetValue<string>() == name)!;
}
