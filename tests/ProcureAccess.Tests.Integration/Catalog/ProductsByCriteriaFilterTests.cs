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
/// Regression test for #37: <c>GET /api/Products/byCriteriaFilterIds</c> returned 500 because the
/// controller did not await the service. Also records today's behaviour of the legacy endpoint:
/// the <c>Result</c> wrapper the client reads, and that a product must carry every selected
/// criteria filter. The endpoint is replaced by the directory in a later migration step.
/// </summary>
public sealed class ProductsByCriteriaFilterTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Endpoint = "/api/Products/byCriteriaFilterIds";

    /// <summary>Ids of two criteria filters of the <c>AddSeedData</c> seed.</summary>
    private const int FirstFilterId = 1;
    private const int SecondFilterId = 2;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        await StoreAsync([
            .. ExistingCatalogSeed.FilterTypes(),
            .. ExistingCatalogSeed.CriteriaFilters(),
            AProduct("Product with both filters", FirstFilterId, SecondFilterId),
            AProduct("Product with the first filter", FirstFilterId),
            AProduct("Product without filters"),
        ]);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ProductsByCriteriaFilterIds_OneFilter_ReturnsProductsWithThatFilter()
    {
        var names = await GetProductNamesAsync($"criteriaFilterIds={FirstFilterId}");

        names.ShouldBe(["Product with both filters", "Product with the first filter"], ignoreOrder: true);
    }

    [Fact]
    public async Task ProductsByCriteriaFilterIds_TwoFilters_ReturnsOnlyProductsWithBothFilters()
    {
        var names = await GetProductNamesAsync(
            $"criteriaFilterIds={FirstFilterId}&criteriaFilterIds={SecondFilterId}");

        names.ShouldBe(["Product with both filters"]);
    }

    /// <summary>Calls the endpoint anonymously and returns the product names of the result.</summary>
    private async Task<List<string?>> GetProductNamesAsync(string query)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = CreateClient();

        using var response = await client.GetAsync($"{Endpoint}?{query}", cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        json.RootElement.GetProperty("succeeded").GetBoolean().ShouldBeTrue();
        return json.RootElement.GetProperty("value").EnumerateArray()
            .Select(product => product.GetProperty("name").GetString())
            .ToList();
    }

    private static Product AProduct(string name, params int[] criteriaFilterIds)
    {
        var product = new Product(name) { CreatedAt = TestData.DefaultCreatedAt };
        foreach (var criteriaFilterId in criteriaFilterIds)
        {
            product.ProductCriteriaFilters.Add(new ProductCriteriaFilter { CriteriaFilterId = criteriaFilterId });
        }

        return product;
    }
}
