// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.Catalog.Releases;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Tests.Integration.Api;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using Keys = ProcureAccess.Tests.Shared.Fixtures.MiniCatalog.Keys;

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// <c>GET /api/v1/catalog/criteria</c> through the real pipeline (SPEC §8.2, S3-20b): the JSON shape
/// with explained priorities and procedure texts (CAT-02, CAT-16, contract), the two kinds of 400
/// (CAT-11, CAT-12, CAT-19), inactive use cases (CAT-18), the revisions of a named release after a
/// newer one is active (CAT-06), the release-pinned procedure mapping (CAT-21), app types without
/// criteria (CAT-20), 404 for an incomplete release, the caching headers and the language of the
/// answer (API-06). The releases are the test folders <c>T.1</c> and <c>T.2</c> on the seeded filter
/// values.
/// </summary>
public sealed class CriteriaEndpointTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Path = "/api/v1/catalog/criteria";

    private static readonly string TestReleasesPath = System.IO.Path.GetDirectoryName(TestReleaseFolders.PathOf(MiniCatalog.T1Version))!;

    private readonly string _tempReleases = Directory.CreateTempSubdirectory("releases-").FullName;

    public async ValueTask InitializeAsync() => await Api.ResetDatabaseAsync();

    public ValueTask DisposeAsync()
    {
        Directory.Delete(_tempReleases, recursive: true);
        return ValueTask.CompletedTask;
    }

    [Fact(DisplayName = "INV:CAT-02 " + nameof(Criteria_WithSelection_HasTheShapeOfSpec82AndExplainsEveryPriority))]
    [Trait("Invariant", "CAT-02")]
    [Trait("Kind", "Contract")]
    public async Task Criteria_WithSelection_HasTheShapeOfSpec82AndExplainsEveryPriority()
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();

        using var response = await client.GetAsync(
            $"{Path}?release=T.1&useCase=hr&appTypeIds[]=web&productPartIds[]=base&testTypeIds[]=softwareTest", Cancellation);

        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        var json = await ReadJsonAsync(response);
        Names(json).ShouldBe(["release", "criteria", "appTypesWithoutCriteria"]);
        json["release"]!.GetValue<string>().ShouldBe("T.1");
        var criteria = json["criteria"]!.AsArray();
        criteria.Count.ShouldBeGreaterThan(0);
        foreach (var criterion in criteria)
        {
            Names(criterion!).ShouldBe([
                "revisionId", "key", "kind", "number", "title", "plainLanguage", "productPart", "appliesTo",
                "standardReferences", "allowedRatings", "procedureTexts", "resolvedPriority"]);
            var priority = criterion!["resolvedPriority"]!;
            Names(priority).ShouldBe(["level", "weight", "rationale", "source", "matchedRule"]);
            priority["rationale"]!.GetValue<string>().ShouldNotBeNullOrWhiteSpace();
            priority["source"]!.GetValue<string>().ShouldBe("expertJudgement");
            Names(priority["matchedRule"]!).ShouldBe(["kind", "appType", "useCase"]);
            priority["matchedRule"]!["kind"]!.GetValue<string>().ShouldBeOneOf("appTypeAndUseCase", "appType", "useCase", "default");
        }

        var allLevels = Find(json, Keys.PriorityAllLevels);
        allLevels["kind"]!.GetValue<string>().ShouldBe("requirement");
        allLevels["number"]!.GetValue<string>().ShouldBe(Keys.PriorityAllLevels);
        var resolved = allLevels["resolvedPriority"]!;
        (resolved["level"]!.GetValue<string>(), resolved["weight"], resolved["rationale"]!.GetValue<string>())
            .ShouldBe(("A", (JsonNode?)null, "Testbegründung A für web und hr."));
        resolved["matchedRule"]!.ToJsonString().ShouldBe("""{"kind":"appTypeAndUseCase","appType":"web","useCase":"hr"}""");
        allLevels["appliesTo"]!.ToJsonString().ShouldBe("""[{"id":"web","name":"Website oder Webanwendung"}]""");
        Names(Find(json, Keys.DefaultOnly)["standardReferences"]![0]!).ShouldBe(["standard", "clause", "url"]);
        var procedure = Find(json, Keys.ProcedureSoftwareTest);
        procedure["kind"]!.GetValue<string>().ShouldBe("testProcedure");
        procedure["number"].ShouldBeNull();
        procedure["appliesTo"]!.AsArray().ShouldBeEmpty();
        procedure["allowedRatings"]!.AsArray().ShouldBeEmpty();
        json["appTypesWithoutCriteria"]!.AsArray().ShouldBeEmpty();
    }

    [Fact(DisplayName = "INV:CAT-16 " + nameof(Criteria_Detail_HasTheTextOfEveryProcedureMappedToTheSelectedAppTypes))]
    [Trait("Invariant", "CAT-16")]
    [Trait("Kind", "Contract")]
    public async Task Criteria_Detail_HasTheTextOfEveryProcedureMappedToTheSelectedAppTypes()
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();

        using var response = await client.GetAsync(
            $"{Path}?appTypeIds[]=web&appTypeIds[]=mobileOpen&productPartIds[]=base&testTypeIds[]=pdfTest", Cancellation);

        var json = await ReadJsonAsync(response);
        var texts = Find(json, Keys.PriorityAllLevels)["procedureTexts"]!.AsArray();
        texts.Select(t => t!["testType"]!.GetValue<string>()).ShouldBe(["softwareTest", "appTest"]);
        Names(texts[0]!).ShouldBe([
            "testType", "originalNumber", "originalTitle", "applicability", "requirement", "rationaleWhy",
            "allowedRatings", "positiveExamples", "negativeExamples", "sourceUrl"]);
        texts.Select(t => t!["sourceUrl"]!.GetValue<string>())
            .ShouldBe(["https://example.org/softwareTest#99.1-softwareTest", "https://example.org/appTest#99.1-appTest"]);

        // Exactly one procedure criterion for the one selected test type.
        CriteriaKeys(json).Where(k => k.StartsWith("PROC-", StringComparison.Ordinal)).ShouldBe([Keys.ProcedurePdfTest]);
    }

    [Theory(DisplayName = "INV:CAT-11 " + nameof(Criteria_IncompleteSelection_Returns400ValidationKeyedAsSent))]
    [Trait("Invariant", "CAT-11")]
    [InlineData("appTypeIds[]=web", "productPartIds")]
    [InlineData("productPartIds[]=base", "appTypeIds")]
    [InlineData("useCase=hr&useCase=reporting&appTypeIds[]=web&productPartIds[]=base", "useCase")]
    [InlineData("appTypeIds[]=web&appTypeIds[]=nope&productPartIds[]=base&testTypeIds[]=nope", "appTypeIds[1]|testTypeIds[0]")]
    public async Task Criteria_IncompleteSelection_Returns400ValidationKeyedAsSent(string query, string errorKeys)
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();

        using var response = await client.GetAsync($"{Path}?{query}", Cancellation);

        var problem = await ProblemResponse.ReadAsync(response, Cancellation);
        problem.Deviations(HttpStatusCode.BadRequest, TypeUri(ProblemTypes.Validation), requireErrors: true).ShouldBeEmpty();
        problem.ErrorKeys.ShouldBe(errorKeys.Split('|'), ignoreOrder: true);
    }

    [Fact(DisplayName = "INV:CAT-12 " + nameof(Criteria_SourceAndTargetOfARule_Returns400SelectionExcluded))]
    [Trait("Invariant", "CAT-12")]
    public async Task Criteria_SourceAndTargetOfARule_Returns400SelectionExcluded()
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();

        using var response = await client.GetAsync(
            $"{Path}?useCase=hr&appTypeIds[]=web&appTypeIds[]=hardware&productPartIds[]=base", Cancellation);

        var problem = await ProblemResponse.ReadAsync(response, Cancellation);
        problem.Deviations(HttpStatusCode.BadRequest, TypeUri(ProblemTypes.SelectionExcluded), requireErrors: true).ShouldBeEmpty();
        problem.ErrorKeys.ShouldBe(["appTypeIds[1]"]);
    }

    [Fact(DisplayName = "INV:CAT-19 " + nameof(Criteria_NamedRelease_ChecksTheSelectionAgainstItsOwnRules))]
    [Trait("Invariant", "CAT-19")]
    public async Task Criteria_NamedRelease_ChecksTheSelectionAgainstItsOwnRules()
    {
        // T.2 is active and has replaced HR → Video by HR → Audio; T.1 keeps its own rule.
        await ImportAndActivateAsync(MiniCatalog.T2Version);
        using var client = CreateClient();
        const string Video = "useCase=hr&appTypeIds[]=web&productPartIds[]=base&productPartIds[]=video";

        using var t1 = await client.GetAsync($"{Path}?release=T.1&{Video}", Cancellation);
        using var active = await client.GetAsync($"{Path}?{Video}", Cancellation);

        var problem = await ProblemResponse.ReadAsync(t1, Cancellation);
        problem.Deviations(HttpStatusCode.BadRequest, TypeUri(ProblemTypes.SelectionExcluded), requireErrors: true).ShouldBeEmpty();
        problem.ErrorKeys.ShouldBe(["productPartIds[1]"]);
        (await ReadJsonAsync(active))["release"]!.GetValue<string>().ShouldBe("T.2");
    }

    [Fact(DisplayName = "INV:CAT-18 " + nameof(Criteria_InactiveUseCase_IsAcceptedAndItsRulesApply))]
    [Trait("Invariant", "CAT-18")]
    public async Task Criteria_InactiveUseCase_IsAcceptedAndItsRulesApply()
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();

        using var response = await client.GetAsync(
            $"{Path}?useCase=databaseApplication&appTypeIds[]=mobileOpen&productPartIds[]=base", Cancellation);

        var matchedRule = Find(await ReadJsonAsync(response), Keys.PriorityAllLevels)["resolvedPriority"]!["matchedRule"]!;
        matchedRule.ToJsonString().ShouldBe("""{"kind":"useCase","appType":null,"useCase":"databaseApplication"}""");
    }

    [Fact(DisplayName = "INV:CAT-06 " + nameof(Criteria_NamedRelease_ReturnsExactlyItsRevisionsAfterANewerOneIsActive))]
    [Trait("Invariant", "CAT-06")]
    public async Task Criteria_NamedRelease_ReturnsExactlyItsRevisionsAfterANewerOneIsActive()
    {
        await ImportAndActivateAsync(MiniCatalog.T2Version);
        using var client = CreateClient();
        const string Selection = "appTypeIds[]=web&appTypeIds[]=softwareOpen&productPartIds[]=base";

        using var t1Response = await client.GetAsync($"{Path}?release=T.1&{Selection}", Cancellation);
        using var activeResponse = await client.GetAsync($"{Path}?{Selection}", Cancellation);
        var t1 = await ReadJsonAsync(t1Response);
        var active = await ReadJsonAsync(activeResponse);

        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var t1Revisions = await RevisionIdsAsync(db, MiniCatalog.T1Version);
        var t2Revisions = await RevisionIdsAsync(db, MiniCatalog.T2Version);
        RevisionIds(t1).ShouldAllBe(id => t1Revisions.Contains(id));
        RevisionIds(active).ShouldAllBe(id => t2Revisions.Contains(id));
        CriteriaKeys(t1).ShouldContain(Keys.Removed);
        CriteriaKeys(active).ShouldNotContain(Keys.Removed);
        Find(t1, Keys.Changed)["title"]!.GetValue<string>().ShouldBe("Testkriterium, Fassung 1");
        Find(active, Keys.Changed)["title"]!.GetValue<string>().ShouldBe("Testkriterium, Fassung 2");

        // An unchanged criterion is the same revision in both releases (CAT-07).
        Find(t1, Keys.PriorityAllLevels)["revisionId"]!.GetValue<int>()
            .ShouldBe(Find(active, Keys.PriorityAllLevels)["revisionId"]!.GetValue<int>());
    }

    [Fact(DisplayName = "INV:CAT-21 " + nameof(Criteria_ProcedureMapping_IsThatOfTheNamedRelease))]
    [Trait("Invariant", "CAT-21")]
    public async Task Criteria_ProcedureMapping_IsThatOfTheNamedRelease()
    {
        // 99.1 is the same revision in both releases; T.2 no longer maps Mobile offen → Software-Test.
        await ImportAndActivateAsync(MiniCatalog.T2Version);
        using var client = CreateClient();
        const string Mobile = "appTypeIds[]=mobileOpen&productPartIds[]=base";

        using var t1 = await client.GetAsync($"{Path}?release=T.1&{Mobile}", Cancellation);
        using var t2 = await client.GetAsync($"{Path}?release=T.2&{Mobile}", Cancellation);

        var inT1 = Find(await ReadJsonAsync(t1), Keys.PriorityAllLevels);
        var inT2 = Find(await ReadJsonAsync(t2), Keys.PriorityAllLevels);
        TestTypes(inT1).ShouldBe(["softwareTest", "appTest"]);
        Strings(inT1["allowedRatings"]!).ShouldBe(["fulfilled", "slightRestriction", "restriction", "barrier", "blockade", "notApplicable"]);
        TestTypes(inT2).ShouldBe(["appTest"]);
        Strings(inT2["allowedRatings"]!).ShouldBe(["fulfilled", "barrier", "blockade", "notApplicable"]);
    }

    [Fact(DisplayName = "INV:CAT-20 " + nameof(Criteria_AppTypeWithoutRequirementCriteria_ReturnsAnEmptyListNotAnError))]
    [Trait("Invariant", "CAT-20")]
    [Trait("Kind", "Contract")]
    public async Task Criteria_AppTypeWithoutRequirementCriteria_ReturnsAnEmptyListNotAnError()
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();

        using var response = await client.GetAsync($"{Path}?appTypeIds[]=hardware&productPartIds[]=base", Cancellation);

        var json = await ReadJsonAsync(response);
        json["criteria"]!.AsArray().ShouldBeEmpty();
        json["appTypesWithoutCriteria"]!.ToJsonString().ShouldBe("""[{"id":"hardware","name":"Hardware"}]""");
    }

    [Fact]
    public async Task Criteria_IncompleteRelease_Returns404()
    {
        // A copy of T.1 whose criterion 99.3 has no priority: the import accepts it, activate and this endpoint do not.
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        CopyReleaseWithoutPriorities(MiniCatalog.T1Version, "T.9", Keys.DefaultOnly);
        await ImportAsync(_tempReleases, "T.9");
        using var client = CreateClient();

        using var response = await client.GetAsync($"{Path}?release=T.9&appTypeIds[]=web&productPartIds[]=base", Cancellation);
        using var active = await client.GetAsync($"{Path}?appTypeIds[]=web&productPartIds[]=base", Cancellation);

        var problem = await ProblemResponse.ReadAsync(response, Cancellation);
        problem.Deviations(HttpStatusCode.NotFound, TypeUri(ProblemTypes.NotFound), requireErrors: false).ShouldBeEmpty();
        active.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("release=2026.1&")]
    [InlineData("")]
    public async Task Criteria_UnknownOrNoActiveRelease_Returns404(string release)
    {
        using var client = CreateClient();

        using var response = await client.GetAsync($"{Path}?{release}appTypeIds[]=web&productPartIds[]=base", Cancellation);

        var problem = await ProblemResponse.ReadAsync(response, Cancellation);
        problem.Deviations(HttpStatusCode.NotFound, TypeUri(ProblemTypes.NotFound), requireErrors: false).ShouldBeEmpty();
    }

    [Theory(DisplayName = "INV:API-06 " + nameof(Criteria_AnyAcceptLanguage_AnswersGermanAndNamesIt))]
    [Trait("Invariant", "API-06")]
    [Trait("Kind", "Contract")]
    [MemberData(nameof(CatalogLanguageContract.AcceptLanguages), MemberType = typeof(CatalogLanguageContract))]
    public async Task Criteria_AnyAcceptLanguage_AnswersGermanAndNamesIt(string? acceptLanguage)
    {
        // T.1 has German texts only, so en, an unknown language and no header get the German answer.
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();
        const string Selection = "useCase=hr&appTypeIds[]=web&productPartIds[]=base&testTypeIds[]=softwareTest";

        await CatalogLanguageContract.ShouldAnswerGermanAsync(client, $"{Path}?release=T.1&{Selection}", acceptLanguage, Cancellation);
        await CatalogLanguageContract.ShouldAnswerGermanAsync(client, $"{Path}?{Selection}", acceptLanguage, Cancellation);
    }

    [Fact]
    public async Task Criteria_CachingHeaders_AreThoseOfEveryCatalogEndpoint()
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();
        const string Selection = "appTypeIds[]=web&productPartIds[]=base";

        using var named = await client.GetAsync($"{Path}?release=T.1&{Selection}", Cancellation);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Path}?release=T.1&{Selection}");
        request.Headers.IfNoneMatch.Add(named.Headers.ETag.ShouldNotBeNull());
        using var revalidated = await client.SendAsync(request, Cancellation);
        using var active = await client.GetAsync($"{Path}?{Selection}", Cancellation);

        named.Headers.CacheControl.ShouldNotBeNull().MaxAge.ShouldBe(TimeSpan.FromHours(1));
        revalidated.StatusCode.ShouldBe(HttpStatusCode.NotModified);
        active.Headers.CacheControl.ShouldNotBeNull().NoCache.ShouldBeTrue();
        active.Headers.ETag.ShouldNotBeNull();
    }

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private string TypeUri(string name) =>
        $"{Api.Factory.Services.GetRequiredService<IConfiguration>()["ProblemTypes:BaseUri"]}/problems/{name}";

    private async Task ImportAndActivateAsync(string activeVersion)
    {
        await ImportAsync(TestReleasesPath, MiniCatalog.T1Version);
        await ImportAsync(TestReleasesPath, MiniCatalog.T2Version);

        // Activates through the command, which changes only the database, as from its own process.
        var error = new StringWriter();
        var exitCode = await CatalogModule.RunCommandAsync(
            Api.Factory.Services, ["catalog", "activate", activeVersion], TextWriter.Null, error, Cancellation);
        exitCode.ShouldBe(0, error.ToString());
    }

    private async Task ImportAsync(string releasesPath, string version)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var result = await new CatalogReleaseImporter(db, Clock).ImportAsync(releasesPath, version, Cancellation);
        result.Errors.ShouldBeEmpty();
        Clock.Advance(TimeSpan.FromMinutes(1));
    }

    /// <summary>Copies a test release folder as <paramref name="newVersion"/> and drops every priority rule of <paramref name="key"/>.</summary>
    private void CopyReleaseWithoutPriorities(string version, string newVersion, string key)
    {
        var source = TestReleaseFolders.PathOf(version);
        var target = System.IO.Path.Combine(_tempReleases, newVersion);
        foreach (var file in Directory.EnumerateFiles(source, "*.json", SearchOption.AllDirectories))
        {
            var copy = System.IO.Path.Combine(target, System.IO.Path.GetRelativePath(source, file));
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(copy)!);
            File.Copy(file, copy);
        }

        var releaseFile = System.IO.Path.Combine(target, "release.json");
        var release = JsonNode.Parse(File.ReadAllText(releaseFile))!;
        release["version"] = newVersion;
        File.WriteAllText(releaseFile, release.ToJsonString());

        var criterionFile = System.IO.Path.Combine(target, "criteria", $"{key}.json");
        var criterion = JsonNode.Parse(File.ReadAllText(criterionFile))!.AsObject();
        criterion.Remove("priorities");
        File.WriteAllText(criterionFile, criterion.ToJsonString());
    }

    private static async Task<HashSet<int>> RevisionIdsAsync(CatalogDbContext db, string version) =>
        [.. await db.Releases.AsNoTracking()
            .Where(r => r.Version == version)
            .SelectMany(r => r.Criteria.Select(c => c.CriterionRevisionId))
            .ToListAsync(Cancellation)];

    private static async Task<JsonNode> ReadJsonAsync(HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        return JsonNode.Parse(await response.Content.ReadAsStringAsync(Cancellation))!;
    }

    private static List<string> Names(JsonNode node) => [.. node.AsObject().Select(p => p.Key)];

    private static JsonNode Find(JsonNode json, string key) =>
        json["criteria"]!.AsArray().Single(c => c!["key"]!.GetValue<string>() == key)!;

    private static List<string> CriteriaKeys(JsonNode json) =>
        [.. json["criteria"]!.AsArray().Select(c => c!["key"]!.GetValue<string>())];

    private static List<int> RevisionIds(JsonNode json) =>
        [.. json["criteria"]!.AsArray().Select(c => c!["revisionId"]!.GetValue<int>())];

    private static List<string> TestTypes(JsonNode criterion) =>
        [.. criterion["procedureTexts"]!.AsArray().Select(t => t!["testType"]!.GetValue<string>())];

    private static List<string> Strings(JsonNode array) => [.. array.AsArray().Select(v => v!.GetValue<string>())];
}
