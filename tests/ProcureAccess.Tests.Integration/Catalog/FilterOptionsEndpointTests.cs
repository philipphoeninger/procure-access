// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
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

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// <c>GET /api/v1/catalog/filter-options</c> through the real pipeline (SPEC §8.1, S3-19): the JSON
/// shape (CAT-12, contract), inactive use cases (CAT-18), the rules of the named or the active
/// release (CAT-19), the next request after <c>catalog activate</c> (CAT-09, HTTP part), Problem
/// Details and the caching headers of ARCHITECTURE §9.6. The releases are the test folders
/// <c>T.1</c> and <c>T.2</c> on the seeded filter values; <c>T.2</c> replaces HR → Video by HR → Audio.
/// </summary>
/// <remarks>Each test starts on an empty catalog; see <see cref="CatalogActivationTests"/> for the cache.</remarks>
public sealed class FilterOptionsEndpointTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Path = "/api/v1/catalog/filter-options";

    private static readonly string TestReleasesPath = System.IO.Path.GetDirectoryName(TestReleaseFolders.PathOf(MiniCatalog.T1Version))!;

    public async ValueTask InitializeAsync() => await Api.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:CAT-12 " + nameof(FilterOptions_WithSelection_HasTheShapeOfSpec81))]
    [Trait("Invariant", "CAT-12")]
    [Trait("Kind", "Contract")]
    public async Task FilterOptions_WithSelection_HasTheShapeOfSpec81()
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();

        using var response = await client.GetAsync(
            $"{Path}?release=T.1&useCase=hr&appTypeIds[]=web&productPartIds[]=base&productPartIds[]=audio", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        var json = await ReadJsonAsync(response);
        Names(json).ShouldBe(["release", "layers"]);
        json.GetProperty("release").GetString().ShouldBe("T.1");
        var layers = json.GetProperty("layers");
        Names(layers).ShouldBe(["useCases", "appTypes", "productParts", "testTypes"]);
        ShouldHaveFields(layers.GetProperty("useCases"), "id", "name", "hint", "excludedBy");
        ShouldHaveFields(layers.GetProperty("appTypes"), "id", "name", "hint", "excludedBy");
        ShouldHaveFields(layers.GetProperty("productParts"), "id", "name", "explanation", "alwaysSelected", "excludedBy");
        ShouldHaveFields(layers.GetProperty("testTypes"), "id", "name", "url", "excludedBy");

        // The rules of T.1 whose source is selected; every other option is available.
        ExcludedBy(layers).ShouldBe(
            new Dictionary<string, string>
            {
                ["appTypes/hardware"] = "useCase:hr:HR-Anwendung",
                ["productParts/video"] = "useCase:hr:HR-Anwendung",
                ["testTypes/appTest"] = "appType:web:Website oder Webanwendung",
                ["testTypes/softwareTest"] = "productPart:audio:Audio",
            },
            ignoreOrder: true);
        var excluder = layers.GetProperty("appTypes").EnumerateArray().Single(a => a.GetProperty("id").GetString() == "hardware")
            .GetProperty("excludedBy")[0];
        Names(excluder).ShouldBe(["layer", "id", "name"]);
        var basePart = layers.GetProperty("productParts").EnumerateArray().First();
        (basePart.GetProperty("id").GetString(), basePart.GetProperty("alwaysSelected").GetBoolean()).ShouldBe(("base", true));
    }

    [Fact(DisplayName = "INV:CAT-12 " + nameof(FilterOptions_WithoutSelection_ListsEverySeededValueAvailable))]
    [Trait("Invariant", "CAT-12")]
    [Trait("Kind", "Contract")]
    public async Task FilterOptions_WithoutSelection_ListsEverySeededValueAvailable()
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();

        using var response = await client.GetAsync(Path, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var layers = (await ReadJsonAsync(response)).GetProperty("layers");
        ExcludedBy(layers).ShouldBeEmpty();
        Ids(layers, "appTypes").ShouldBe(
            ["web", "softwareOpen", "softwareClosed", "mobileOpen", "mobileClosed", "document", "hardware", "relayEmergencyService"]);
        Ids(layers, "productParts").Count.ShouldBe(7);
        Ids(layers, "testTypes").ShouldBe(["softwareTest", "appTest", "pdfTest", "bikWeb", "bikApp"]);
    }

    [Fact(DisplayName = "INV:CAT-18 " + nameof(FilterOptions_InactiveUseCase_IsNotOfferedButAccepted))]
    [Trait("Invariant", "CAT-18")]
    public async Task FilterOptions_InactiveUseCase_IsNotOfferedButAccepted()
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();

        using var response = await client.GetAsync($"{Path}?useCase=databaseApplication", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var layers = (await ReadJsonAsync(response)).GetProperty("layers");
        // The six active seeded use cases in German order; „Datenbankgestützte Fachanwendung“ is inactive.
        Ids(layers, "useCases").ShouldBe(
            ["travelRequests", "trainingManagement", "payroll", "hr", "internalCommunication", "reporting"]);
        ExcludedBy(layers).ShouldBe(new Dictionary<string, string>
        {
            ["testTypes/appTest"] = "useCase:databaseApplication:Datenbankgestützte Fachanwendung",
        });
    }

    [Fact(DisplayName = "INV:CAT-19 " + nameof(FilterOptions_NamedRelease_UsesTheRulesOfThatRelease))]
    [Trait("Invariant", "CAT-19")]
    public async Task FilterOptions_NamedRelease_UsesTheRulesOfThatRelease()
    {
        // T.2 is active and has replaced HR → Video by HR → Audio; T.1 keeps its own rule.
        await ImportAndActivateAsync(MiniCatalog.T2Version);
        using var client = CreateClient();

        using var t1 = await client.GetAsync($"{Path}?release=T.1&useCase=hr", Cancellation);
        using var t2 = await client.GetAsync($"{Path}?release=T.2&useCase=hr", Cancellation);
        using var active = await client.GetAsync($"{Path}?useCase=hr", Cancellation);

        var t1Json = await ReadJsonAsync(t1);
        t1Json.GetProperty("release").GetString().ShouldBe("T.1");
        ExcludedBy(t1Json.GetProperty("layers")).Keys.ShouldBe(["appTypes/hardware", "productParts/video"], ignoreOrder: true);
        var t2Json = await ReadJsonAsync(t2);
        t2Json.GetProperty("release").GetString().ShouldBe("T.2");
        ExcludedBy(t2Json.GetProperty("layers")).Keys.ShouldBe(["appTypes/hardware", "productParts/audio"], ignoreOrder: true);
        var activeJson = await ReadJsonAsync(active);
        activeJson.GetProperty("release").GetString().ShouldBe("T.2");
        ExcludedBy(activeJson.GetProperty("layers")).Keys.ShouldBe(["appTypes/hardware", "productParts/audio"], ignoreOrder: true);
    }

    [Fact(DisplayName = "INV:CAT-09 " + nameof(FilterOptions_RequestAfterActivation_ReturnsTheNewRelease))]
    [Trait("Invariant", "CAT-09")]
    public async Task FilterOptions_RequestAfterActivation_ReturnsTheNewRelease()
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();
        using var before = await client.GetAsync($"{Path}?useCase=hr", Cancellation);
        var etagBefore = before.Headers.ETag.ShouldNotBeNull();

        await ActivateAsync(MiniCatalog.T2Version);
        // A browser revalidates with the ETag it has; the answer is the new release, not a 304.
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Path}?useCase=hr");
        request.Headers.IfNoneMatch.Add(etagBefore);
        using var after = await client.SendAsync(request, Cancellation);

        (await ReadJsonAsync(before)).GetProperty("release").GetString().ShouldBe("T.1");
        after.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadJsonAsync(after)).GetProperty("release").GetString().ShouldBe("T.2");
        after.Headers.ETag.ShouldNotBe(etagBefore);
    }

    [Fact]
    public async Task FilterOptions_NamedRelease_MayBeKeptForAnHourAndRevalidatesTo304()
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();
        using var first = await client.GetAsync($"{Path}?release=T.1&useCase=hr", Cancellation);
        var etag = first.Headers.ETag.ShouldNotBeNull();

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{Path}?release=T.1&useCase=hr");
        request.Headers.IfNoneMatch.Add(etag);
        using var second = await client.SendAsync(request, Cancellation);
        using var otherSelection = await client.GetAsync($"{Path}?release=T.1&useCase=reporting", Cancellation);

        first.Headers.CacheControl.ShouldNotBeNull().ToString().ShouldBe("public, must-revalidate, max-age=3600");
        second.StatusCode.ShouldBe(HttpStatusCode.NotModified);
        (await second.Content.ReadAsByteArrayAsync(Cancellation)).ShouldBeEmpty();
        second.Headers.ETag.ShouldBe(etag);
        second.Headers.CacheControl.ShouldNotBeNull().MaxAge.ShouldBe(TimeSpan.FromHours(1));
        otherSelection.Headers.ETag.ShouldNotBe(etag);
    }

    [Fact]
    public async Task FilterOptions_ActiveRelease_MustBeRevalidatedOnEveryUse()
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();

        using var response = await client.GetAsync(Path, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var cacheControl = response.Headers.CacheControl.ShouldNotBeNull();
        (cacheControl.Public, cacheControl.NoCache, cacheControl.MaxAge).ShouldBe((true, true, (TimeSpan?)null));
        response.Headers.ETag.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("?release=2026.1")]
    [InlineData("")]
    public async Task FilterOptions_UnknownOrNoActiveRelease_Returns404ProblemDetails(string query)
    {
        // Nothing is imported: no release is stored and none is active.
        using var client = CreateClient();

        using var response = await client.GetAsync(Path + query, Cancellation);

        var problem = await ProblemResponse.ReadAsync(response, Cancellation);
        problem.Deviations(HttpStatusCode.NotFound, TypeUri(ProblemTypes.NotFound), requireErrors: false).ShouldBeEmpty();
        problem.MediaTypeOfResponse.ShouldBe(ProblemResponse.MediaType);
    }

    [Fact]
    public async Task FilterOptions_UnknownKeys_Returns400ValidationProblemKeyedAsSent()
    {
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();

        using var response = await client.GetAsync(
            $"{Path}?useCase=nope&appTypeIds[]=web&appTypeIds[]=nope&productPartIds[]=nope", Cancellation);

        var problem = await ProblemResponse.ReadAsync(response, Cancellation);
        problem.Deviations(HttpStatusCode.BadRequest, TypeUri(ProblemTypes.Validation), requireErrors: true).ShouldBeEmpty();
        problem.MediaTypeOfResponse.ShouldBe(ProblemResponse.MediaType);
        problem.ErrorKeys.ShouldBe(["useCase", "appTypeIds[1]", "productPartIds[0]"], ignoreOrder: true);
    }

    [Fact]
    public async Task FilterOptions_UnversionedPath_IsNotMapped()
    {
        // Only /api/v1 exists for module endpoints (owner, S3-19, #153).
        await ImportAndActivateAsync(MiniCatalog.T1Version);
        using var client = CreateClient();

        using var response = await client.GetAsync("/api/catalog/filter-options", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private string TypeUri(string name) =>
        $"{Api.Factory.Services.GetRequiredService<IConfiguration>()["ProblemTypes:BaseUri"]}/problems/{name}";

    private async Task ImportAndActivateAsync(string activeVersion)
    {
        foreach (var version in new[] { MiniCatalog.T1Version, MiniCatalog.T2Version })
        {
            await using var scope = Api.Factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var result = await new CatalogReleaseImporter(db, Clock).ImportAsync(TestReleasesPath, version, Cancellation);
            result.Errors.ShouldBeEmpty();
            Clock.Advance(TimeSpan.FromMinutes(1));
        }

        await ActivateAsync(activeVersion);
    }

    /// <summary>Activates through the command, which changes only the database, as from its own process.</summary>
    private async Task ActivateAsync(string version)
    {
        var error = new StringWriter();
        var exitCode = await CatalogModule.RunCommandAsync(
            Api.Factory.Services, ["catalog", "activate", version], TextWriter.Null, error, Cancellation);
        exitCode.ShouldBe(0, error.ToString());
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return document.RootElement.Clone();
    }

    private static List<string> Names(JsonElement element) => element.EnumerateObject().Select(p => p.Name).ToList();

    private static List<string?> Ids(JsonElement layers, string layer) =>
        layers.GetProperty(layer).EnumerateArray().Select(o => o.GetProperty("id").GetString()).ToList();

    /// <summary>Every item of <paramref name="list"/> has exactly <paramref name="fields"/>, in this order.</summary>
    private static void ShouldHaveFields(JsonElement list, params string[] fields)
    {
        list.GetArrayLength().ShouldBeGreaterThan(0);
        foreach (var item in list.EnumerateArray())
        {
            Names(item).ShouldBe(fields);
        }
    }

    /// <summary>The non-empty <c>excludedBy</c> lists as <c>layer/id</c> → <c>layer:id:name</c>, joined by <c>|</c>.</summary>
    private static Dictionary<string, string> ExcludedBy(JsonElement layers) =>
        layers.EnumerateObject()
            .SelectMany(layer => layer.Value.EnumerateArray().Select(option => (
                Key: $"{layer.Name}/{option.GetProperty("id").GetString()}",
                Sources: option.GetProperty("excludedBy").EnumerateArray()
                    .Select(e => $"{e.GetProperty("layer").GetString()}:{e.GetProperty("id").GetString()}:{e.GetProperty("name").GetString()}")
                    .ToList())))
            .Where(option => option.Sources.Count > 0)
            .ToDictionary(option => option.Key, option => string.Join('|', option.Sources));
}
