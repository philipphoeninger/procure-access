// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Text.RegularExpressions;
using Npgsql;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.Platform.Analytics;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Tests.Integration.Configurator;
using ProcureAccess.Tests.Integration.Export;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// The indicator queries of <c>docs/TECHNICAL.md</c> §8 (SPEC R-MET-02, §9.2; S6-30), read from the
/// document itself: every <c>sql</c> block whose first line is <c>-- metric: &lt;name&gt;</c>. Each
/// runs in a read-only transaction against rows whose aggregates are known — two organisations'
/// saved lists with decisions, a project's copy that must not count, candidates in two projects and
/// usage events in two months — and returns exactly those aggregates, with no id column. A renamed
/// table or column therefore fails here, not in the next report.
/// </summary>
public sealed partial class MetricQueryTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private static readonly string DocumentPath = Path.Combine(AppContext.BaseDirectory, "Docs", "TECHNICAL.md");

    private ResolvedCatalogSelection? _selection;

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private ResolvedCatalogSelection Selection => _selection ?? throw new InvalidOperationException("Not seeded.");

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        var tenants = Tenants.Build(Clock.GetUtcNow());
        // The accounts first: organisations and memberships reference them (foreign keys, S6-10b).
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => TestAccounts.Of(u, confirmed: true))]);
        await StoreAsync(tenants.Entities);
        await ExportTestCatalog.ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        _selection = await SavedLists.ResolveAsync(Api.Factory.Services, MiniCatalog.T1Version, Cancellation);
        Selection.Criteria.Count.ShouldBeGreaterThan(2);

        // Two saved lists of two organisations with the same decisions, and organisation A's
        // project with its own copy of A's list: the copy has the decisions too, but is no saved list.
        var now = Clock.GetUtcNow();
        var listA = SavedLists.Build(tenants.A.Id, TestUsers.EditorA.Id, "Liste A", now, MiniCatalog.T1Version, Selection, Decide);
        var listB = SavedLists.Build(tenants.B.Id, TestUsers.EditorB.Id, "Liste B", now, MiniCatalog.T1Version, Selection, Decide);
        var projectA = ProcurementProject.Create(tenants.A.Id, "Projekt A", TestUsers.OwnerA.Id, now);
        var projectB = ProcurementProject.Create(tenants.B.Id, "Projekt B", TestUsers.OwnerB.Id, now);
        var copy = listA.CopyForProject(projectA.Id, TestUsers.OwnerA.Id, now);
        var candidates = new[] { Candidate(projectA, "Produkt 1"), Candidate(projectA, "Produkt 2"), Candidate(projectB, "Produkt 3") };
        await StoreAsync(listA, listB, projectA, projectB, copy);
        await StoreAsync([.. candidates, .. candidates.Select(c => CandidatePrivateSection.Of(c, null, null))]);

        await StoreAsync(
            Event(UsageEventType.CriteriaListShown, "2026-10-03", "anonymous"),
            Event(UsageEventType.CriteriaListShown, "2026-10-31", "anonymous"),
            Event(UsageEventType.CriteriaListShown, "2026-11-01", "signedIn"),
            Event(UsageEventType.ExportCreated, "2026-10-15", "xlsx", "anonymous"),
            Event(UsageEventType.ExportCreated, "2026-10-16", "xlsx", "anonymous"),
            Event(UsageEventType.ExportCreated, "2026-11-20", "csv", "signedIn"));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public void Document_MetricBlocks_AreExactlyTheQueriesThisTestKnows()
    {
        Queries().Keys.ShouldBe(
            ["completed-configurations", "exports", "saved-lists", "workspace-entries", "priority-overrides"],
            ignoreOrder: true);
    }

    [Theory]
    [InlineData("completed-configurations")]
    [InlineData("exports")]
    [InlineData("saved-lists")]
    [InlineData("workspace-entries")]
    [InlineData("priority-overrides")]
    public async Task Query_OnKnownRows_ReturnsOnlyItsAggregates(string name)
    {
        var (columns, rows) = await RunReadOnlyAsync(Queries()[name]);

        columns.ShouldNotContain(c => c.Type == typeof(Guid), "an indicator never names a row");
        // Compared without order: how the database collation sorts criterion keys is not the point.
        rows.ShouldBe(Expected(name), ignoreOrder: true);
    }

    /// <summary>The rows each query must return for the seeded data, as <c>value|value|…</c>.</summary>
    private List<string> Expected(string name) => name switch
    {
        "completed-configurations" => ["2026-10-01|anonymous|2", "2026-11-01|signedIn|1"],
        "exports" => ["2026-10-01|xlsx|anonymous|2", "2026-11-01|csv|signedIn|1"],
        "saved-lists" => ["2"],
        "workspace-entries" => ["3"],
        "priority-overrides" => [.. Selection.Criteria
            .Select((criterion, index) => (criterion.Key, Row: index switch
            {
                0 => "2|0|0|2",
                1 => criterion.RecommendedPriority.Level == PriorityLevel.B ? "2|2|0|0" : "2|2|2|0",
                _ => "2|0|0|0",
            }))
            .Select(r => $"{r.Key}|{r.Row}")],
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "No expectation for this query."),
    };

    /// <summary>
    /// The decisions of both lists: the first criterion left out, the second with another level (and
    /// a weight when that level is B), the last with a „choice“ equal to its recommendation, which the
    /// domain stores as no choice, so it does not count as a changed level.
    /// </summary>
    private ConfigurationCriterionDraft Decide(ResolvedCatalogCriterion criterion)
    {
        var index = Selection.Criteria.ToList().IndexOf(criterion);
        var recommended = criterion.RecommendedPriority.Level;
        return index switch
        {
            0 => SavedLists.Draft(criterion, isIncluded: false),
            1 when recommended == PriorityLevel.B => SavedLists.Draft(criterion, overrideLevel: PriorityLevel.A),
            1 => SavedLists.Draft(criterion, overrideLevel: PriorityLevel.B, overrideWeight: 9),
            _ when index == Selection.Criteria.Count - 1 => SavedLists.Draft(criterion, overrideLevel: recommended),
            _ => SavedLists.Draft(criterion),
        };
    }

    private CandidateProduct Candidate(ProcurementProject project, string name) =>
        CandidateProduct.Create(project, new CandidateFacts(name, null, null, null), Clock.GetUtcNow());

    private static UsageEvent Event(UsageEventType type, string day, string dimension1, string? dimension2 = null) => new()
    {
        EventType = type,
        OccurredOn = DateOnly.Parse(day, CultureInfo.InvariantCulture),
        Dimension1 = dimension1,
        Dimension2 = dimension2,
    };

    /// <summary>The <c>-- metric:</c> blocks of the document, by name; a name used twice fails.</summary>
    private static Dictionary<string, string> Queries()
    {
        var document = File.ReadAllText(DocumentPath);
        var blocks = MetricBlock().Matches(document);
        blocks.ShouldNotBeEmpty($"no -- metric: block in {DocumentPath}");
        return blocks.ToDictionary(m => m.Groups["name"].Value, m => m.Groups["sql"].Value, StringComparer.Ordinal);
    }

    /// <summary>
    /// Runs <paramref name="sql"/> in a read-only transaction, as the document tells the operator to,
    /// and returns its columns and its rows with dates as <c>yyyy-MM-dd</c>.
    /// </summary>
    private async Task<(List<(string Name, Type Type)> Columns, List<string> Rows)> RunReadOnlyAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(Api.ConnectionString);
        await connection.OpenAsync(Cancellation);
        await using var transaction = await connection.BeginTransactionAsync(Cancellation);
        await using (var readOnly = new NpgsqlCommand("SET TRANSACTION READ ONLY", connection, transaction))
        {
            await readOnly.ExecuteNonQueryAsync(Cancellation);
        }

        await using var command = new NpgsqlCommand(sql, connection, transaction);
        await using var reader = await command.ExecuteReaderAsync(Cancellation);
        var columns = Enumerable.Range(0, reader.FieldCount).Select(i => (reader.GetName(i), reader.GetFieldType(i))).ToList();
        var rows = new List<string>();
        while (await reader.ReadAsync(Cancellation))
        {
            rows.Add(string.Join('|', Enumerable.Range(0, reader.FieldCount).Select(i => Format(reader.GetValue(i)))));
        }

        return (columns, rows);
    }

    private static string Format(object value) => value switch
    {
        DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        IFormattable number => number.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    // ```sql, then the line "-- metric: <name>", then everything up to the closing fence.
    [GeneratedRegex(@"^```sql\r?\n(?<sql>-- metric: (?<name>[a-z-]+)\r?\n.*?)^```", RegexOptions.Multiline | RegexOptions.Singleline)]
    private static partial Regex MetricBlock();
}
