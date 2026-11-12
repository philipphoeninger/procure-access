// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// <see cref="ICatalogSelectionResolver"/> (S4-19) on the stored test releases: for the same
/// selection it gives what <c>GET /api/v1/catalog/criteria</c> answers — same criteria, same
/// recommended priorities (CAT-02) — plus the names the export file needs, and it reports which
/// release is active.
/// </summary>
public sealed class CatalogSelectionResolverTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Query = "useCase=hr&appTypeIds[]=web&productPartIds[]=base&testTypeIds[]=softwareTest&testTypeIds[]=pdfTest";

    private static readonly CatalogSelectionQuery Selection = new("hr", ["web"], ["base"], ["softwareTest", "pdfTest"]);

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:CAT-02 " + nameof(Resolve_Selection_AgreesWithTheCriteriaEndpoint))]
    [Trait("Invariant", "CAT-02")]
    public async Task Resolve_Selection_AgreesWithTheCriteriaEndpoint()
    {
        using var client = CreateClient();
        var answer = await GetAsync(client, $"/api/v1/catalog/criteria?release=T.1&{Query}");
        var options = (await GetAsync(client, $"/api/v1/catalog/filter-options?release=T.1&{Query}"))["layers"]!;

        var result = await ResolveAsync(MiniCatalog.T1Version, Selection);

        result.Outcome.ShouldBe(CatalogSelectionOutcome.Resolved);
        result.Release.ShouldBe(new CatalogReleaseSummary(MiniCatalog.T1Version, true, 15));
        var resolved = result.Selection.ShouldNotBeNull();
        resolved.UseCase.ShouldBe(new CatalogValueName("hr", Name(options, "useCases", "hr")));
        resolved.AppTypes.ShouldBe([new CatalogValueName("web", Name(options, "appTypes", "web"))]);
        resolved.ProductParts.ShouldBe([new CatalogValueName("base", Name(options, "productParts", "base"))]);
        resolved.TestTypes.Select(t => t.Key).ShouldBe(["softwareTest", "pdfTest"]);

        var expected = answer["criteria"]!.AsArray();
        resolved.Criteria.Count.ShouldBe(expected.Count);
        foreach (var (criterion, json) in resolved.Criteria.Zip(expected))
        {
            var priority = json!["resolvedPriority"]!;
            var texts = json["procedureTexts"]!.AsArray();
            var part = json["productPart"]?.GetValue<string>();
            criterion.ShouldSatisfyAllConditions(
                c => c.RevisionId.ShouldBe(json["revisionId"]!.GetValue<int>()),
                c => c.Key.ShouldBe(json["key"]!.GetValue<string>()),
                c => c.Kind.ShouldBe(json["kind"]!.GetValue<string>() == "testProcedure" ? CriterionKind.TestProcedure : CriterionKind.Requirement),
                c => c.Number.ShouldBe(json["number"]?.GetValue<string>()),
                c => c.Title.ShouldBe(json["title"]!.GetValue<string>()),
                c => c.PlainLanguageTitle.ShouldBe(json["plainLanguage"]?.GetValue<string>()),
                c => c.ProductPart.ShouldBe(part is null ? null : Name(options, "productParts", part)),
                c => c.RecommendedPriority.Level.ToString().ShouldBe(priority["level"]!.GetValue<string>()),
                c => c.RecommendedPriority.Weight.ShouldBe(priority["weight"]?.GetValue<int>()),
                c => c.RecommendedPriority.Rationale.ShouldBe(priority["rationale"]!.GetValue<string>()),
                c => c.OriginalTitle.ShouldBe(texts.Count > 0 ? texts[0]!["originalTitle"]!.GetValue<string>() : null),
                c => c.TestStepUrl.ShouldBe(texts.Count > 0
                    ? texts[0]!["sourceUrl"]!.GetValue<string>()
                    : c.Kind == CriterionKind.TestProcedure ? Url(options, c.Key["PROC-".Length..]) : null));
        }

        resolved.Criteria.ShouldContain(c => c.OriginalTitle != null);
        resolved.Criteria.ShouldContain(c => c.ProductPart != null);
        resolved.Criteria.ShouldContain(c => c.Kind == CriterionKind.TestProcedure);
    }

    [Fact]
    public async Task Resolve_ReleaseThatIsNotActive_SaysSo()
    {
        var result = await ResolveAsync(MiniCatalog.T2Version, Selection);

        result.Outcome.ShouldBe(CatalogSelectionOutcome.Resolved);
        result.Release.ShouldBe(new CatalogReleaseSummary(MiniCatalog.T2Version, false, 14));
    }

    [Fact]
    public async Task Resolve_UnknownRelease_IsReleaseNotFound()
    {
        var result = await ResolveAsync("9.9", Selection);

        result.ShouldSatisfyAllConditions(
            r => r.Outcome.ShouldBe(CatalogSelectionOutcome.ReleaseNotFound),
            r => r.Release.ShouldBeNull(),
            r => r.Selection.ShouldBeNull());
    }

    [Fact(DisplayName = "INV:CAT-12 " + nameof(Resolve_ExcludedValue_IsExcludedSelectionKeyedAsSent))]
    [Trait("Invariant", "CAT-12")]
    public async Task Resolve_ExcludedValue_IsExcludedSelectionKeyedAsSent()
    {
        var result = await ResolveAsync(MiniCatalog.T1Version, new CatalogSelectionQuery("hr", ["web", "hardware"], ["base"], []));

        result.Outcome.ShouldBe(CatalogSelectionOutcome.ExcludedSelection);
        result.Errors.Keys.ShouldBe(["appTypeIds[1]"]);
    }

    [Fact]
    public async Task Resolve_IncompleteSelection_IsInvalidSelection()
    {
        var result = await ResolveAsync(MiniCatalog.T1Version, new CatalogSelectionQuery(null, [], ["base"], []));

        result.Outcome.ShouldBe(CatalogSelectionOutcome.InvalidSelection);
        result.Errors.Keys.ShouldBe(["appTypeIds"]);
    }

    private async Task<CatalogSelectionResult> ResolveAsync(string release, CatalogSelectionQuery selection)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ICatalogSelectionResolver>()
            .ResolveAsync(release, selection, Cancellation);
    }

    private static async Task<JsonNode> GetAsync(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url, Cancellation);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonNode>(Cancellation))!;
    }

    private static string Name(JsonNode layers, string layer, string id) =>
        layers[layer]!.AsArray().Single(v => v!["id"]!.GetValue<string>() == id)!["name"]!.GetValue<string>();

    private static string Url(JsonNode layers, string id) =>
        layers["testTypes"]!.AsArray().Single(v => v!["id"]!.GetValue<string>() == id)!["url"]!.GetValue<string>();

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;
}
