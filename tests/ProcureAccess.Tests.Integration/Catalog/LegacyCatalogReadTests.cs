// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json;
using MODELS.ProcureAccess.Entities;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Builders;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// The query projections of the legacy catalog lists on PostgreSQL (S3-07, ADR-015): the
/// hand-written expressions that replaced AutoMapper's <c>ProjectTo</c> must translate to SQL and
/// fill the nested lookups as before. Retired with the legacy endpoints (migration step 2).
/// </summary>
public sealed class LegacyCatalogReadTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const int FirstFilterId = 1;
    private const int SecondFilterId = 2;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        var product = new Product("Product with a filter") { CreatedAt = TestData.DefaultCreatedAt };
        product.ProductCriteriaFilters.Add(new ProductCriteriaFilter { CriteriaFilterId = FirstFilterId });
        await StoreAsync([
            .. ExistingCatalogSeed.FilterTypes(),
            .. ExistingCatalogSeed.CriteriaFilters(),
            new CriteriaFilterExclusion { CriteriaFilterId = FirstFilterId, ExclusionId = SecondFilterId },
            product,
        ]);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task CriteriaFilters_GetAll_ListsExclusionsAsLookups()
    {
        var filters = await GetValuesAsync("/api/CriteriaFilters");

        var first = filters.Single(filter => filter.GetProperty("id").GetInt32() == FirstFilterId);
        var exclusion = first.GetProperty("exclusions").EnumerateArray().ShouldHaveSingleItem();
        exclusion.GetProperty("id").GetInt32().ShouldBe(SecondFilterId);
        exclusion.GetProperty("name").GetString().ShouldBe(NameOfFilter(SecondFilterId));
        first.GetProperty("filterTypeId").GetInt32().ShouldBe(ExistingCatalogSeed.CriteriaFilters().Single(f => f.Id == FirstFilterId).FilterTypeId);
    }

    [Fact]
    public async Task Products_GetAll_ListsCriteriaFiltersAsLookups()
    {
        var products = await GetValuesAsync("/api/Products");

        var product = products.ShouldHaveSingleItem();
        product.GetProperty("name").GetString().ShouldBe("Product with a filter");
        var filter = product.GetProperty("criteriaFilters").EnumerateArray().ShouldHaveSingleItem();
        filter.GetProperty("id").GetInt32().ShouldBe(FirstFilterId);
        filter.GetProperty("name").GetString().ShouldBe(NameOfFilter(FirstFilterId));
    }

    private static string NameOfFilter(int id) => ExistingCatalogSeed.CriteriaFilters().Single(filter => filter.Id == id).Name;

    /// <summary>Calls the endpoint anonymously and returns the elements of the <c>Result</c> wrapper's value.</summary>
    private async Task<List<JsonElement>> GetValuesAsync(string path)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = CreateClient();

        using var response = await client.GetAsync(path, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        json.RootElement.GetProperty("succeeded").GetBoolean().ShouldBeTrue();
        return json.RootElement.GetProperty("value").EnumerateArray().Select(element => element.Clone()).ToList();
    }
}
