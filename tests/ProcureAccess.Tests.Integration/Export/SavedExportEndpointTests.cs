// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using ClosedXML.Excel;
using ProcureAccess.Modules.Catalog;
using ProcureAccess.Modules.Configurator.Configurations;
using ProcureAccess.Modules.Platform.Analytics;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Export;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;

namespace ProcureAccess.Tests.Integration.Export;

/// <summary>
/// <c>POST /api/v1/configurations/{id}/exports</c> (S5-21a; R-LST-12, SPEC §8.3; ARCHITECTURE §4.4
/// Flow A2): the file of a saved list, compared with the list (EXP-01) and under the release the
/// list is pinned to (EXP-03, CAT-06), named with the slug of its name. The content parity with the
/// anonymous export is <c>ExportSourceParityTests</c> (EXP-06); the authorisation matrix is
/// <c>AuthorizationMatrixTests</c> (TEN-05); the rate limit is <c>RateLimitTests</c> (API-05).
/// </summary>
public sealed class SavedExportEndpointTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Path = "/api/v1/configurations";

    private Tenants? _tenants;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        _tenants = Tenants.Build(Clock.GetUtcNow());
        // The accounts first: organisations and memberships reference them (foreign keys, S6-10b).
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => TestAccounts.Of(u, confirmed: true))]);
        await StoreAsync(_tenants.Entities);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:EXP-01 " + nameof(Export_Xlsx_ReopenedWithClosedXml_MatchesTheSavedList))]
    [Trait("Invariant", "EXP-01")]
    public async Task Export_Xlsx_ReopenedWithClosedXml_MatchesTheSavedList()
    {
        using var client = CreateClient(TestUsers.EditorA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var keys = criteria.Select(c => c!["key"]!.GetValue<string>()).ToList();
        var (left, changed) = (keys[0], keys[1]);
        var id = await SaveAsync(client, "Vergabe Büro-Software 2027", criteria);
        // Changed after saving, so the file shows what is stored now.
        await DecideAsync(client, id, criteria, left, new { isIncluded = false });
        await DecideAsync(client, id, criteria, changed, new { isIncluded = true, overrideLevel = "B", overrideWeight = 9, note = "=Mit Anbieter klären" });
        var list = (await client.GetFromJsonAsync<ConfigurationResponse>($"{Path}/{id}", Cancellation)).ShouldNotBeNull();
        using var reader = CreateClient(TestUsers.ReaderA);

        using var response = await reader.PostAsJsonAsync($"{Path}/{id}/exports", new { format = "xlsx" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
        response.Content.Headers.ContentDisposition.ShouldNotBeNull().FileNameStar
            .ShouldBe($"ProcureAccess_Kriterien_Vergabe-Buero-Software-2027_T.1_{Day()}.xlsx");
        using var workbook = new XLWorkbook(new MemoryStream(await response.Content.ReadAsByteArrayAsync(Cancellation)));
        var table = workbook.Table("Kriterien");
        var header = table.HeadersRow().Cells().Select(c => c.GetString()).ToList();
        var rows = table.DataRange.Rows().ToList();
        IXLCell Cell(IXLRangeRow row, string column) => row.Cell(header.IndexOf(column) + 1);
        var decisions = criteria.Zip(keys).ToDictionary(
            p => p.Second, p => list.Criteria.Single(c => c.CriterionRevisionId == p.First!["revisionId"]!.GetValue<int>()));
        // Exactly the included criteria of the list, in the catalog's order, each with what the list stores.
        rows.Select(row => Cell(row, "Kriterium").GetString()).ShouldBe(keys.Where(key => decisions[key].IsIncluded));
        foreach (var row in rows)
        {
            var decision = decisions[Cell(row, "Kriterium").GetString()];
            Cell(row, "Priorität (empfohlen)").GetString().ShouldBe(Label(decision.ResolvedLevel));
            Cell(row, "Priorität (Ihre Wahl)").GetString().ShouldBe(Label(decision.OverrideLevel ?? decision.ResolvedLevel));
            Cell(row, "Ihre Notiz").GetString().ShouldBe(decision.Note ?? string.Empty);
        }

        var changedRow = rows.Single(row => Cell(row, "Kriterium").GetString() == changed);
        Cell(changedRow, "Gewichtung").GetValue<int>().ShouldBe(9);
        Cell(changedRow, "Ihre Notiz").HasFormula.ShouldBeFalse();
        InfoRelease(workbook).ShouldBe(MiniCatalog.T1Version);
    }

    [Theory(DisplayName = "INV:EXP-03 " + nameof(Export_ListOfAnOlderRelease_RecordsItsOwnRelease))]
    [Trait("Invariant", "EXP-03")]
    [InlineData("csv")]
    [InlineData("xlsx")]
    public async Task Export_ListOfAnOlderRelease_RecordsItsOwnRelease(string format)
    {
        using var client = CreateClient(TestUsers.EditorA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var id = await SaveAsync(client, "Liste", criteria);
        await ActivateAsync(MiniCatalog.T2Version);

        using var response = await client.PostAsJsonAsync($"{Path}/{id}/exports", new { format }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        response.Content.Headers.ContentDisposition.ShouldNotBeNull().FileNameStar
            .ShouldBe($"ProcureAccess_Kriterien_Liste_T.1_{Day()}.{format}");
        var file = await response.Content.ReadAsByteArrayAsync(Cancellation);
        if (format == "csv")
        {
            var records = CsvReader.Read(file);
            records[0][0].ShouldBe("Katalogversion");
            records.Count.ShouldBe(criteria.Count + 1);
            records.Skip(1).ShouldAllBe(record => record[0] == MiniCatalog.T1Version);
        }
        else
        {
            using var workbook = new XLWorkbook(new MemoryStream(file));
            InfoRelease(workbook).ShouldBe(MiniCatalog.T1Version);
        }
    }

    [Fact]
    public async Task Export_Created_StoresOneSignedInExportCreated()
    {
        using var client = CreateClient(TestUsers.ReaderA);
        using var editor = CreateClient(TestUsers.EditorA);
        var id = await SaveAsync(editor, "Liste", await CriteriaAsync(editor, MiniCatalog.T1Version, WebSelection, Cancellation));
        await UsageEventRows.DeleteAllAsync(Api);

        using var response = await client.PostAsJsonAsync($"{Path}/{id}/exports", new { format = "csv" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var stored = (await UsageEventRows.WaitForAsync(Api, 1)).ShouldHaveSingleItem();
        stored.Dimension1.ShouldBe(UsageEventWriter.Dimensions.Csv);
        stored.Dimension2.ShouldBe(UsageEventWriter.Dimensions.SignedIn);
    }

    [Theory]
    [InlineData("pdf")]
    [InlineData(null)]
    public async Task Export_UnknownFormat_Returns400AtFormat(string? format)
    {
        using var client = CreateClient(TestUsers.EditorA);
        var id = await SaveAsync(client, "Liste", await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation));

        using var response = await client.PostAsJsonAsync($"{Path}/{id}/exports", new { format }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        body.RootElement.GetProperty("type").GetString().ShouldNotBeNull().ShouldEndWith("/problems/" + ProblemTypes.Validation);
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name).ShouldBe(["format"]);
    }

    [Fact]
    public async Task Export_UnknownList_Returns404()
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PostAsJsonAsync($"{Path}/{Guid.NewGuid()}/exports", new { format = "csv" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<Guid> SaveAsync(HttpClient client, string name, JsonArray criteria)
    {
        var body = Body("xlsx", MiniCatalog.T1Version, WebSelection, criteria);
        body.Remove("format");
        body["organizationId"] = Fixture.A.Id;
        body["name"] = name;
        using var response = await client.PostAsJsonAsync(Path, body, Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
        return (await response.Content.ReadFromJsonAsync<CreatedConfiguration>(Cancellation)).ShouldNotBeNull().Id;
    }

    private static async Task DecideAsync(HttpClient client, Guid id, JsonArray criteria, string key, object decision)
    {
        var revision = criteria.Single(c => c!["key"]!.GetValue<string>() == key)!["revisionId"]!.GetValue<int>();
        using var response = await client.PatchAsJsonAsync($"{Path}/{id}/criteria/{revision}", decision, Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync(Cancellation));
    }

    private async Task ActivateAsync(string version)
    {
        var error = new StringWriter();
        var exitCode = await CatalogModule.RunCommandAsync(
            Api.Factory.Services, ["catalog", "activate", version], TextWriter.Null, error, Cancellation);
        exitCode.ShouldBe(0, error.ToString());
    }

    private string Day() => Clock.GetUtcNow().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string InfoRelease(XLWorkbook workbook) =>
        workbook.Worksheet("Info").RowsUsed().Single(row => row.Cell(1).GetString() == "Katalogversion").Cell(2).GetString();

    private static string Label(string level) => level switch
    {
        "A" => "A – Mindestanforderung",
        "B" => "B – Bewertungskriterium",
        _ => "Hinweis",
    };
}
