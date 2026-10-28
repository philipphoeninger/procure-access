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
/// The builder defaults are valid in the database too (TESTING §6.1): an entity built without
/// overrides can be stored in the empty database without seed data.
/// </summary>
public sealed class BuilderPersistenceTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact]
    public async Task Criterion_StoredWithBuilderDefaults_IsStoredWithItsOwnCriteriaFilter()
    {
        await StoreAsync(ACriterion().Build());
        using var client = CreateClient();

        using var response = await client.GetAsync("/api/Criteria", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        json.RootElement.GetProperty("value").EnumerateArray()
            .Select(criterion => criterion.GetProperty("name").GetString())
            .ShouldBe(["Test criterion"]);
    }
}
