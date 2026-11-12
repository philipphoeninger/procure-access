// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using ProcureAccess.Modules.Platform.Persistence.Configurations;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;

namespace ProcureAccess.Tests.Integration.Export;

/// <summary>
/// EXP-05: the anonymous export succeeds without sign-in and persists nothing — the row count of
/// every table outside <c>analytics</c> stays the same, and no note reaches a log, also when the
/// request is refused or cannot be read. Runs on its own database and API host, so no other host
/// writes rows while it counts, and with <see cref="CapturingLoggerProvider"/> at every level.
/// </summary>
/// <remarks>
/// The <c>logging</c> schema is left out of the counts: the Serilog sink writes the host's own
/// warnings there in the background. It is searched for the sentinel instead, after the host has
/// flushed it. No file store exists yet; its part of EXP-05 follows with the store (TESTING V-row
/// of S4-19).
/// </remarks>
public sealed class AnonymousExportPersistsNothingTests(ApiFixture api) : IntegrationTest(api)
{
    private const string Sentinel = "SENTINEL-EXPORT-NOTE-7f3a";

    private static readonly string[] UncountedSchemas = [UsageEventConfiguration.Schema, SeriLogEntryConfiguration.Schema];

    [Fact(DisplayName = "INV:EXP-05 " + nameof(Export_Anonymous_PersistsNothingAndLogsNoNote))]
    [Trait("Invariant", "EXP-05")]
    public async Task Export_Anonymous_PersistsNothingAndLogsNoNote()
    {
        var connectionString = Api.ConnectionStringForNewDatabase();
        var logs = new CapturingLoggerProvider();
        var factory = new ProcureAccessApiFactory(
            connectionString,
            Clock,
            configureServices: services => services.AddLogging(logging => logging
                .AddProvider(logs)
                .AddFilter<CapturingLoggerProvider>(null, LogLevel.Trace)));
        try
        {
            try
            {
                using var client = factory.CreateDefaultClient(BaseAddress);
                await ImportBothAndActivateAsync(factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
                var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
                var before = await DatabaseRowCounts.TakeAsync(connectionString, UncountedSchemas, Cancellation);

                foreach (var format in new[] { "xlsx", "csv" })
                {
                    using var exported = await client.PostAsJsonAsync(
                        ExportPath,
                        Body(format, MiniCatalog.T1Version, WebSelection, criteria, (key, entry) => entry["note"] = $"{Sentinel} {key}"),
                        Cancellation);
                    exported.StatusCode.ShouldBe(HttpStatusCode.OK);
                }

                using var refused = await client.PostAsJsonAsync(
                    ExportPath,
                    Body("xlsx", MiniCatalog.T1Version, WebSelection, criteria, (_, entry) => entry["note"] = Sentinel + new string('x', 1000)),
                    Cancellation);
                refused.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
                (await refused.Content.ReadAsStringAsync(Cancellation)).ShouldNotContain(Sentinel);

                using var unreadable = await client.PostAsync(ExportPath, UnreadableBody(criteria), Cancellation);
                unreadable.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

                var after = await DatabaseRowCounts.TakeAsync(connectionString, UncountedSchemas, Cancellation);
                after.ShouldBe(before);
                logs.Events.ShouldNotBeEmpty();
                logs.Text.ShouldNotContain(Sentinel);
            }
            finally
            {
                // Disposing the host flushes the batched PostgreSQL log sink.
                await factory.DisposeAsync();
            }

            (await StoredLogRowsWithAsync(connectionString, Sentinel)).ShouldBe(0);
        }
        finally
        {
            await DropAsync(connectionString);
        }
    }

    /// <summary>A body with a note that cannot be bound: the weight is text, so the binding fails before the endpoint.</summary>
    private static StringContent UnreadableBody(JsonArray criteria)
    {
        var body = Body("xlsx", MiniCatalog.T1Version, WebSelection, criteria, (_, entry) =>
        {
            entry["note"] = Sentinel;
            entry["overrideWeight"] = "zehn";
        });
        return new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
    }

    private static async Task<long> StoredLogRowsWithAsync(string connectionString, string text)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(Cancellation);
        await using var command = new NpgsqlCommand(
            $"SELECT count(*) FROM \"{SeriLogEntryConfiguration.Schema}\".\"{SeriLogEntryConfiguration.Table}\" AS row WHERE row::text LIKE @pattern",
            connection);
        command.Parameters.AddWithValue("pattern", $"%{text}%");
        return (long)(await command.ExecuteScalarAsync(Cancellation))!;
    }

    private static async Task DropAsync(string connectionString)
    {
        await using var db = new ApplicationDBContext(new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseNpgsql(connectionString)
            .Options);
        await db.Database.EnsureDeletedAsync(Cancellation);
    }

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;
}
