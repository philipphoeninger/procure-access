// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;

namespace ProcureAccess.Tests.Integration.Api;

/// <summary>
/// TEN-04 as ARCHITECTURE §9.6 says (catalog and export parts; the directory part follows in
/// step 6): both catalog endpoints answer with an <c>ETag</c>, <c>public, max-age=3600,
/// must-revalidate</c> for a named release and <c>public, no-cache</c> for the active one; every
/// answer of the export carries <c>no-store</c>.
/// </summary>
public sealed class CacheHeaderTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Selection = "useCase=hr&appTypeIds[]=web&productPartIds[]=base";

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Theory(DisplayName = "INV:TEN-04 " + nameof(Catalog_NamedOrActiveRelease_CarriesETagAndItsCacheControl))]
    [Trait("Invariant", "TEN-04")]
    [InlineData("/api/v1/catalog/filter-options")]
    [InlineData("/api/v1/catalog/criteria")]
    public async Task Catalog_NamedOrActiveRelease_CarriesETagAndItsCacheControl(string path)
    {
        using var client = CreateClient();

        using var named = await client.GetAsync($"{path}?release=T.1&{Selection}", Cancellation);
        using var active = await client.GetAsync($"{path}?{Selection}", Cancellation);

        named.StatusCode.ShouldBe(HttpStatusCode.OK);
        named.Headers.ETag.ShouldNotBeNull();
        named.Headers.CacheControl.ShouldNotBeNull().ShouldSatisfyAllConditions(
            c => c.Public.ShouldBeTrue(),
            c => c.MaxAge.ShouldBe(TimeSpan.FromHours(1)),
            c => c.MustRevalidate.ShouldBeTrue(),
            c => c.NoStore.ShouldBeFalse());
        active.StatusCode.ShouldBe(HttpStatusCode.OK);
        active.Headers.ETag.ShouldNotBeNull();
        active.Headers.CacheControl.ShouldNotBeNull().ShouldSatisfyAllConditions(
            c => c.Public.ShouldBeTrue(),
            c => c.NoCache.ShouldBeTrue(),
            c => c.MaxAge.ShouldBeNull());
    }

    [Fact(DisplayName = "INV:TEN-04 " + nameof(Export_EveryAnswer_CarriesNoStore))]
    [Trait("Invariant", "TEN-04")]
    public async Task Export_EveryAnswer_CarriesNoStore()
    {
        using var client = CreateClient();
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);

        using var file = await client.PostAsJsonAsync(ExportPath, Body("csv", MiniCatalog.T1Version, WebSelection, criteria), Cancellation);
        using var refused = await client.PostAsJsonAsync(ExportPath, Body("csv", MiniCatalog.T1Version, WebSelection, []), Cancellation);
        using var notFound = await client.PostAsJsonAsync(ExportPath, Body("csv", "9.9", WebSelection, []), Cancellation);
        using var unreadable = await client.PostAsync(
            ExportPath, new StringContent("{", Encoding.UTF8, "application/json"), Cancellation);

        file.StatusCode.ShouldBe(HttpStatusCode.OK);
        refused.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        notFound.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        unreadable.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        foreach (var response in new[] { file, refused, notFound, unreadable })
        {
            response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue($"{(int)response.StatusCode} without no-store");
        }
    }

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;
}
