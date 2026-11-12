// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Tests.Integration.Api;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Export;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;
using Keys = ProcureAccess.Tests.Shared.Fixtures.MiniCatalog.Keys;

namespace ProcureAccess.Tests.Integration.Export;

/// <summary>
/// <c>POST /api/v1/exports</c> on the test releases (R-LST-12): the file of a draft, the server's own
/// priorities and the request check (EXP-07), the selection check against the rules of the release
/// (CAT-12, export part), the active release only (owner decision O-S4-60 (a)) and the body limit
/// (SPEC §7.2). Nothing stored: <see cref="AnonymousExportPersistsNothingTests"/>.
/// </summary>
public sealed class ExportEndpointTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await Api.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Theory(DisplayName = "INV:EXP-07 " + nameof(Export_DraftOfTheActiveRelease_ReturnsTheFileAsAttachment))]
    [Trait("Invariant", "EXP-07")]
    [InlineData("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData("csv", "text/csv")]
    public async Task Export_DraftOfTheActiveRelease_ReturnsTheFileAsAttachment(string format, string mediaType)
    {
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        using var client = CreateClient();
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);

        using var response = await PostAsync(client, Body(format, MiniCatalog.T1Version, WebSelection, criteria));

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        response.Content.Headers.ContentType!.MediaType.ShouldBe(mediaType);
        var disposition = response.Content.Headers.ContentDisposition.ShouldNotBeNull();
        disposition.DispositionType.ShouldBe("attachment");
        var day = Clock.GetUtcNow().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        disposition.FileNameStar.ShouldBe($"ProcureAccess_Kriterien_T.1_{day}.{format}");
    }

    [Fact(DisplayName = "INV:EXP-03 " + nameof(Export_Csv_ListsTheIncludedCriteriaWithTheReleaseAndTheUsersDecisions))]
    [Trait("Invariant", "EXP-03")]
    public async Task Export_Csv_ListsTheIncludedCriteriaWithTheReleaseAndTheUsersDecisions()
    {
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        using var client = CreateClient();
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var keys = criteria.Select(c => c!["key"]!.GetValue<string>()).ToList();
        var (left, changed) = (keys[0], keys[1]);
        var body = Body("csv", MiniCatalog.T1Version, WebSelection, criteria, (key, entry) =>
        {
            if (key == left)
            {
                entry["isIncluded"] = false;
            }
            else if (key == changed)
            {
                entry["overrideLevel"] = "Info";
                entry["note"] = "Mit Anbieter klären; \"bald\"";
            }
        });

        using var response = await PostAsync(client, body);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        var records = CsvReader.Read(await response.Content.ReadAsByteArrayAsync(Cancellation));
        var header = records[0].ToList();
        var rows = records.Skip(1).ToList();
        string Cell(IReadOnlyList<string> row, string column) => row[header.IndexOf(column)];
        rows.Select(row => Cell(row, "Katalogversion")).ShouldAllBe(release => release == MiniCatalog.T1Version);
        rows.Select(row => Cell(row, "Nr.")).ShouldBe(Enumerable.Range(1, keys.Count - 1).Select(n => n.ToString(CultureInfo.InvariantCulture)));
        rows.Select(row => Cell(row, "Kriterium")).ShouldBe(keys.Where(key => key != left));
        rows.Select(row => Cell(row, "Kriterium")).ShouldContain("PROC-softwareTest");
        var changedRow = rows.Single(row => Cell(row, "Kriterium") == changed);
        Cell(changedRow, "Priorität (Ihre Wahl)").ShouldBe("Hinweis");
        Cell(changedRow, "Ihre Notiz").ShouldBe("Mit Anbieter klären; \"bald\"");
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Export_RecommendedLevelInTheRequest_IsRefused))]
    [Trait("Invariant", "EXP-07")]
    public async Task Export_RecommendedLevelInTheRequest_IsRefused()
    {
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        using var client = CreateClient();
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var body = Body("xlsx", MiniCatalog.T1Version, WebSelection, criteria, (key, entry) =>
        {
            if (key == Keys.PriorityAllLevels)
            {
                entry["resolvedPriority"] = new JsonObject { ["level"] = "Info" };
            }
        });

        using var response = await PostAsync(client, body);

        var problem = await ProblemResponse.ReadAsync(response, Cancellation);
        problem.Deviations(HttpStatusCode.BadRequest, TypeUri(ProblemTypes.Validation), requireErrors: true).ShouldBeEmpty();
        problem.ErrorKeys.ShouldBe(["criteria[0].resolvedPriority"]);
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Export_RevisionOfAnotherRelease_IsAFieldError))]
    [Trait("Invariant", "EXP-07")]
    public async Task Export_RevisionOfAnotherRelease_IsAFieldError()
    {
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        using var client = CreateClient();
        var t1 = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var t2 = await CriteriaAsync(client, MiniCatalog.T2Version, WebSelection, Cancellation);
        var t1Revisions = t1.Select(c => c!["revisionId"]!.GetValue<int>()).ToHashSet();
        var foreign = t2.Select(c => c!["revisionId"]!.GetValue<int>()).First(id => !t1Revisions.Contains(id));
        var body = Body("xlsx", MiniCatalog.T1Version, WebSelection, t1);
        body["criteria"]!.AsArray().Add(new JsonObject { ["criterionRevisionId"] = foreign, ["isIncluded"] = true });

        using var response = await PostAsync(client, body);

        var problem = await ProblemResponse.ReadAsync(response, Cancellation);
        problem.Deviations(HttpStatusCode.BadRequest, TypeUri(ProblemTypes.Validation), requireErrors: true).ShouldBeEmpty();
        problem.ErrorKeys.ShouldBe([$"criteria[{t1.Count}].criterionRevisionId"]);
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Export_FieldErrors_AreKeyedByTheirPathInTheBody))]
    [Trait("Invariant", "EXP-07")]
    public async Task Export_FieldErrors_AreKeyedByTheirPathInTheBody()
    {
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        using var client = CreateClient();
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var body = Body("pdf", MiniCatalog.T1Version, WebSelection, criteria, (key, entry) =>
        {
            if (key == Keys.PriorityAllLevels)
            {
                entry["overrideWeight"] = 11;
                entry["note"] = new string('x', 1001);
            }
        });
        body["selection"]!["appTypeIds"]!.AsArray().Add("unknown");

        using var response = await PostAsync(client, body);

        var problem = await ProblemResponse.ReadAsync(response, Cancellation);
        problem.Deviations(HttpStatusCode.BadRequest, TypeUri(ProblemTypes.Validation), requireErrors: true).ShouldBeEmpty();
        problem.ErrorKeys.ShouldBe(
            ["format", "selection.appTypeIds[1]", "criteria[0].overrideWeight", "criteria[0].note"], ignoreOrder: true);
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Export_ReleaseNoLongerActive_Returns400ReleaseMismatch))]
    [Trait("Invariant", "EXP-07")]
    public async Task Export_ReleaseNoLongerActive_Returns400ReleaseMismatch()
    {
        // Owner decision O-S4-60 (a): T.2 replaced T.1 while the draft was open.
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T2Version, Cancellation);
        using var client = CreateClient();
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);

        using var response = await PostAsync(client, Body("xlsx", MiniCatalog.T1Version, WebSelection, criteria));

        var problem = await ProblemResponse.ReadAsync(response, Cancellation);
        problem.Deviations(HttpStatusCode.BadRequest, TypeUri(ProblemTypes.ReleaseMismatch), requireErrors: true).ShouldBeEmpty();
        problem.ErrorKeys.ShouldBe(["selection.releaseVersion"]);
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Export_UnknownRelease_Returns404))]
    [Trait("Invariant", "EXP-07")]
    public async Task Export_UnknownRelease_Returns404()
    {
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        using var client = CreateClient();

        using var response = await PostAsync(client, Body("xlsx", "9.9", WebSelection, []));

        var problem = await ProblemResponse.ReadAsync(response, Cancellation);
        problem.Deviations(HttpStatusCode.NotFound, TypeUri(ProblemTypes.NotFound), requireErrors: false).ShouldBeEmpty();
    }

    /// <summary>One rule per pair of layers of <c>T.1</c> (MiniCatalog); the error names the excluded value as sent.</summary>
    [Theory(DisplayName = "INV:CAT-12 " + nameof(Export_SourceAndTargetOfARule_Returns400SelectionExcluded))]
    [Trait("Invariant", "CAT-12")]
    [InlineData("hr", "web,hardware", "base", "", "selection.appTypeIds[1]")]
    [InlineData("hr", "web", "base,video", "", "selection.productPartIds[1]")]
    [InlineData("databaseApplication", "softwareOpen", "base", "appTest", "selection.testTypeIds[0]")]
    [InlineData(null, "mobileOpen", "base,audio", "", "selection.productPartIds[1]")]
    [InlineData(null, "web", "base", "appTest", "selection.testTypeIds[0]")]
    [InlineData(null, "softwareOpen", "base,audio", "softwareTest", "selection.testTypeIds[0]")]
    public async Task Export_SourceAndTargetOfARule_Returns400SelectionExcluded(
        string? useCase, string appTypes, string productParts, string testTypes, string errorKey)
    {
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        using var client = CreateClient();
        var selection = new JsonObject
        {
            ["useCaseTemplateId"] = useCase,
            ["appTypeIds"] = List(appTypes),
            ["productPartIds"] = List(productParts),
            ["testTypeIds"] = List(testTypes),
        };

        using var response = await PostAsync(client, Body("xlsx", MiniCatalog.T1Version, selection, []));

        var problem = await ProblemResponse.ReadAsync(response, Cancellation);
        problem.Deviations(HttpStatusCode.BadRequest, TypeUri(ProblemTypes.SelectionExcluded), requireErrors: true).ShouldBeEmpty();
        problem.ErrorKeys.ShouldBe([errorKey]);

        static JsonArray List(string list) =>
            [.. list.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(k => (JsonNode?)JsonValue.Create(k))];
    }

    /// <summary>
    /// SPEC §7.2: at most 1 MB. Kestrel applies the limit from the endpoint metadata and answers 413;
    /// the test server has no body-size feature, so the test checks the metadata (413 by hand, S4-19
    /// protocol §4).
    /// </summary>
    [Fact(DisplayName = "INV:EXP-07 " + nameof(Export_Endpoint_LimitsTheBodyToOneMegabyte))]
    [Trait("Invariant", "EXP-07")]
    public void Export_Endpoint_LimitsTheBodyToOneMegabyte()
    {
        var endpoint = Api.Factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Single(e => e.RoutePattern.RawText?.EndsWith("/exports", StringComparison.Ordinal) == true
                && e.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains("POST") == true);

        endpoint.Metadata.GetMetadata<IRequestSizeLimitMetadata>().ShouldNotBeNull().MaxRequestBodySize.ShouldBe(1024 * 1024);
    }

    [Fact]
    public async Task Export_BodyThatIsNotJson_Returns400Validation()
    {
        using var client = CreateClient();
        using var content = new StringContent("{ \"format\": \"xlsx\", ", Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(ExportPath, content, Cancellation);

        var problem = await ProblemResponse.ReadAsync(response, Cancellation);
        problem.Deviations(HttpStatusCode.BadRequest, TypeUri(ProblemTypes.Validation), requireErrors: true).ShouldBeEmpty();
    }

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, JsonObject body) =>
        client.PostAsJsonAsync(ExportPath, body, Cancellation);

    private string TypeUri(string name) =>
        $"{Api.Factory.Services.GetRequiredService<IConfiguration>()["ProblemTypes:BaseUri"]}/problems/{name}";
}
