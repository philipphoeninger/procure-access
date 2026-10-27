// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// The Swagger UI lists its versions through <c>app.DescribeApiVersions()</c> when the pipeline is
/// built, the Swagger documents come from <c>IApiVersionDescriptionProvider</c>. Both lists must
/// match. A versioned minimal API mapped after <c>ConfigureSwaggerExplorer()</c> gets a document
/// but no entry in the UI, so this test fails (checked with a temporary v2 endpoint when it was
/// written). Today all versions come from controllers, which are found in any order; moving the
/// unversioned minimal APIs changes nothing this test can see.
/// </summary>
public sealed partial class SwaggerUiTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact]
    public async Task SwaggerUi_Urls_MatchTheGeneratedDocuments()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var documents = Api.Factory.Services.GetRequiredService<IOptions<SwaggerGenOptions>>()
            .Value.SwaggerGeneratorOptions.SwaggerDocs.Keys;
        using var client = CreateClient();

        var script = await client.GetStringAsync("/swagger/index.js", cancellationToken);

        var match = ConfigObject().Match(script);
        match.Success.ShouldBeTrue("index.js has no configObject");
        using var config = JsonDocument.Parse(match.Groups["json"].Value);
        var urls = config.RootElement.GetProperty("urls").EnumerateArray()
            .Select(u => u.GetProperty("url").GetString())
            .ToList();
        documents.ShouldNotBeEmpty();
        urls.ShouldBe(documents.Select(d => $"/swagger/{d}/swagger.json"), ignoreOrder: true);
        foreach (var url in urls)
        {
            using var response = await client.GetAsync(url, cancellationToken);
            response.StatusCode.ShouldBe(HttpStatusCode.OK, url);
        }
    }

    [GeneratedRegex(@"var configObject = (?<json>\{.*\});")]
    private static partial Regex ConfigObject();
}
