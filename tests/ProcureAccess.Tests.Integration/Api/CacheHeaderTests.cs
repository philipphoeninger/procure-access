// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Configurator.Drafts;
using ProcureAccess.Modules.Configurator.Settings;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;

namespace ProcureAccess.Tests.Integration.Api;

/// <summary>
/// TEN-04 as ARCHITECTURE §9.6 says (catalog, export and, since S5-22, workspace parts; the
/// directory part follows in step 6): both catalog endpoints answer with an <c>ETag</c>,
/// <c>public, max-age=3600, must-revalidate</c> for a named release and <c>public, no-cache</c>
/// for the active one; every answer of the export carries <c>no-store</c>, and so does every other
/// answer below <c>/api/v1</c> (the platform default). The workspace endpoints with every user and
/// status are in <c>AuthorizationMatrixTests</c>.
/// </summary>
public sealed partial class CacheHeaderTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
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

    [Fact(DisplayName = "INV:TEN-04 " + nameof(Settings_Anonymous_AnswersTheConfiguredWeightRangeWithNoStore))]
    [Trait("Invariant", "TEN-04")]
    public async Task Settings_Anonymous_AnswersTheConfiguredWeightRangeWithNoStore()
    {
        using var client = CreateClient();
        var configured = Api.Factory.Services.GetRequiredService<PriorityWeightRange>();

        using var response = await client.GetAsync("/api/v1/configurator/settings", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
        var settings = await response.Content.ReadFromJsonAsync<ConfiguratorSettingsResponse>(Cancellation);
        settings.ShouldNotBeNull().PriorityWeight.ShouldBe(new WeightRangeResponse(configured.Min, configured.Max));
    }

    /// <summary>
    /// The default covers every GET below <c>/api/v1</c> except the catalog, signed in and anonymous,
    /// whatever the status, with a random id in each route parameter. State-changing endpoints are
    /// not swept blindly (one would sign the client out); the matrix covers the workspace ones.
    /// </summary>
    [Theory(DisplayName = "INV:TEN-04 " + nameof(EveryGet_SignedInOrAnonymous_CarriesNoStore))]
    [Trait("Invariant", "TEN-04")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EveryGet_SignedInOrAnonymous_CarriesNoStore(bool signedIn)
    {
        var paths = Api.Factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains(HttpMethods.Get) == true)
            .Select(e => e.RoutePattern.RawText ?? string.Empty)
            .Where(p => p.TrimStart('/').StartsWith("api/v{version", StringComparison.Ordinal))
            .Where(p => !p.Contains("/catalog/", StringComparison.Ordinal))
            .Select(ToPath)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        paths.ShouldContain("/api/v1/configurations");
        paths.ShouldContain("/api/v1/auth/session");
        using var client = CreateClient(signedIn ? TestUsers.OwnerA : null);

        foreach (var path in paths)
        {
            using var response = await client.GetAsync(path, Cancellation);

            ((int)response.StatusCode).ShouldBeLessThan(500, path);
            response.Headers.CacheControl.ShouldNotBeNull(path).NoStore.ShouldBeTrue($"{path} {(int)response.StatusCode} without no-store");
        }
    }

    [Fact(DisplayName = "INV:TEN-04 " + nameof(UnknownPath_BelowTheVersionedApi_CarriesNoStore))]
    [Trait("Invariant", "TEN-04")]
    public async Task UnknownPath_BelowTheVersionedApi_CarriesNoStore()
    {
        using var client = CreateClient(TestUsers.OwnerA);

        using var response = await client.GetAsync("/api/v1/gibt-es-nicht", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
    }

    /// <summary>The route pattern with version 1 and a random id for every other parameter.</summary>
    private static string ToPath(string pattern) =>
        "/" + RouteParameter().Replace(pattern.Trim('/'), m =>
            m.Value.StartsWith("{version", StringComparison.Ordinal) ? "1" : Guid.NewGuid().ToString());

    [GeneratedRegex(@"\{[^}]+\}")]
    private static partial Regex RouteParameter();

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;
}
