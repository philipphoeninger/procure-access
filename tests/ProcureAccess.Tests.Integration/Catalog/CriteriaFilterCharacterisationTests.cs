// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// CAT-10, API part: characterisation of the existing criteria filter
/// (<c>GET /api/Criteria/byCriteriaFilterIds</c>). For the seed of migration <c>AddSeedData</c>
/// and each recorded selection, the endpoint returns exactly the recorded criteria.
/// </summary>
/// <remarks>
/// The expected sets live in <c>Catalog/Fixtures/cat-10-criteria-filter.json</c>. They were
/// recorded from the endpoint before migration step 2; step 2 must keep them true for the
/// migrated catalog (TESTING §4.2, status 0→2). Only the fields listed in
/// <c>comparedFields</c> are compared, and the order is ignored.
/// </remarks>
public sealed class CriteriaFilterCharacterisationTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Endpoint = "/api/Criteria/byCriteriaFilterIds";

    private static readonly Lazy<CriteriaFilterFixture> Fixture = new(CriteriaFilterFixture.Load);

    public static TheoryData<string> Cases => new(Fixture.Value.Cases.Select(c => c.Name));

    /// <summary>
    /// Every test starts from the same seed. The seed rows carry fixed ids, so they are stored
    /// once per test after a reset rather than once per class.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        await StoreAsync(ExistingCatalogSeed.All());
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Theory(DisplayName = "INV:CAT-10 " + nameof(CriteriaByFilterIds_FixedSeedAndSelection_ReturnsRecordedCriteria))]
    [MemberData(nameof(Cases))]
    [Trait("Invariant", "CAT-10")]
    public async Task CriteriaByFilterIds_FixedSeedAndSelection_ReturnsRecordedCriteria(string caseName)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var fixture = Fixture.Value;
        var testCase = fixture.Cases.Single(c => c.Name == caseName);
        // Anonymous, as in the configurator without account (J1).
        using var client = CreateClient();
        var query = string.Join("&", testCase.CriteriaFilterIds.Select(id => $"criteriaFilterIds={id}"));

        using var response = await client.GetAsync($"{Endpoint}?{query}", cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var actual = json.RootElement.EnumerateArray().Select(fixture.KeyOf).ToList();
        var expected = testCase.Expected.Select(fixture.KeyOf).ToList();

        actual.Except(expected).ShouldBeEmpty("criteria returned that the recording does not contain");
        expected.Except(actual).ShouldBeEmpty("recorded criteria missing from the response");
        actual.Count.ShouldBe(expected.Count, "the response contains a criterion more than once");
    }
}

/// <summary>The fixture file of CAT-10: seed description, compared fields and recorded cases.</summary>
internal sealed record CriteriaFilterFixture(
    string Invariant,
    string Description,
    IReadOnlyList<string> ComparedFields,
    IReadOnlyList<CriteriaFilterCase> Cases)
{
    private const string FileName = "Catalog/Fixtures/cat-10-criteria-filter.json";

    public static CriteriaFilterFixture Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, FileName);
        var fixture = JsonSerializer.Deserialize<CriteriaFilterFixture>(
            File.ReadAllText(path),
            new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException($"{FileName} is empty.");

        if (fixture.ComparedFields.Count == 0 || fixture.Cases.Count == 0)
        {
            throw new InvalidOperationException($"{FileName} needs comparedFields and cases.");
        }

        return fixture;
    }

    /// <summary>
    /// The compared fields of one criterion as a single comparable string, e.g.
    /// <c>id=1 | name=5.2 … | criteriaFilterId=1</c>. Used for the response and the recording alike.
    /// </summary>
    public string KeyOf(JsonElement criterion) => string.Join(" | ", ComparedFields.Select(field =>
        criterion.TryGetProperty(field, out var value)
            ? $"{field}={value}"
            : throw new ShouldAssertException($"Criterion {criterion} has no field '{field}'.")));
}

/// <summary>One recorded selection and the criteria the endpoint returned for it.</summary>
internal sealed record CriteriaFilterCase(
    string Name,
    string Description,
    IReadOnlyList<int> CriteriaFilterIds,
    IReadOnlyList<JsonElement> Expected);
