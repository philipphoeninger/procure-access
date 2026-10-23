// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Shared.Builders.TestData;

namespace ProcureAccess.Tests.Integration.FixtureTests;

/// <summary>
/// Sample integration test (S1-13): an existing endpoint reads from the real PostgreSQL 17
/// database through the full API pipeline.
/// </summary>
public sealed class DatabaseFixtureTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact]
    public async Task FilterTypes_GetAll_ReturnsOnlyFilterTypeStoredByTheTest()
    {
        await StoreAsync(AFilterType().WithName("Screen reader support").Build());
        using var client = CreateClient();

        using var response = await client.GetAsync("/api/FilterTypes", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        // The existing CRUD endpoints wrap their result as { succeeded, value }.
        // Only the stored row: the reset also removed the reference data of migration AddSeedData.
        json.RootElement.GetProperty("value").EnumerateArray()
            .Select(filterType => filterType.GetProperty("name").GetString())
            .ShouldBe(["Screen reader support"]);
    }
}
