// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProcureAccess.Modules.Platform.Persistence;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Persistence;

/// <summary>
/// OPS-03 for the Platform module's own context (ARCHITECTURE ADR-014): its migrations apply to an
/// empty database after the legacy ones (as at startup) and on their own, record themselves in
/// <c>platform."__EFMigrationsHistory"</c>, and its model matches the last migration. Its baseline
/// adopts the tables the legacy migrations created, so both ways must give the same tables.
/// Each test uses databases of its own on the shared container.
/// </summary>
public sealed class PlatformMigrationTests(ApiFixture api) : IAsyncDisposable
{
    private static readonly string[] PlatformSchemas = ["logging", "analytics"];

    private readonly List<string> _databases = [];

    [Fact(DisplayName = "INV:OPS-03 " + nameof(Migrate_EmptyDatabaseLegacyFirst_AppliesAllMigrationsWithSeparateHistories))]
    [Trait("Invariant", "OPS-03")]
    public async Task Migrate_EmptyDatabaseLegacyFirst_AppliesAllMigrationsWithSeparateHistories()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();

        await using var legacy = Legacy(connectionString);
        await using var platform = Platform(connectionString);
        await legacy.Database.MigrateAsync(cancellationToken);
        await platform.Database.MigrateAsync(cancellationToken);

        (await platform.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldBe(platform.Database.GetMigrations());
        (await platform.Database.GetPendingMigrationsAsync(cancellationToken)).ShouldBeEmpty();
        (await legacy.Database.GetPendingMigrationsAsync(cancellationToken)).ShouldBeEmpty();

        // Each context only knows its own migrations, so neither sees the other's as unknown.
        (await legacy.Database.GetAppliedMigrationsAsync(cancellationToken))
            .ShouldNotContain(id => platform.Database.GetMigrations().Contains(id));
        (await ScalarAsync(connectionString, """SELECT count(*) FROM platform."__EFMigrationsHistory" """))
            .ShouldBe(platform.Database.GetMigrations().Count());
    }

    [Fact(DisplayName = "INV:OPS-03 " + nameof(PlatformMigrations_EmptyDatabaseWithoutLegacy_CreateTheTablesTheLegacyMigrationsCreated))]
    [Trait("Invariant", "OPS-03")]
    public async Task PlatformMigrations_EmptyDatabaseWithoutLegacy_CreateTheTablesTheLegacyMigrationsCreated()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var withLegacy = NewDatabase();
        var platformOnly = NewDatabase();

        await using (var legacy = Legacy(withLegacy))
        {
            await legacy.Database.MigrateAsync(cancellationToken);
        }

        await using (var platform = Platform(withLegacy))
        {
            await platform.Database.MigrateAsync(cancellationToken);
        }

        await using (var platform = Platform(platformOnly))
        {
            await platform.Database.MigrateAsync(cancellationToken);
            (await platform.Database.GetPendingMigrationsAsync(cancellationToken)).ShouldBeEmpty();
        }

        var expected = await LayoutAsync(withLegacy);
        expected.ShouldNotBeEmpty();
        (await LayoutAsync(platformOnly)).ShouldBe(expected);
    }

    [Fact(DisplayName = "INV:OPS-03 " + nameof(PlatformModel_ComparedWithLastMigration_HasNoPendingChanges))]
    [Trait("Invariant", "OPS-03")]
    public async Task PlatformModel_ComparedWithLastMigration_HasNoPendingChanges()
    {
        // Same check as `dotnet ef migrations has-pending-model-changes --context PlatformDbContext`.
        await using var platform = Platform(NewDatabase());

        platform.Database.HasPendingModelChanges().ShouldBeFalse(
            "the Platform model differs from the last migration's snapshot; add a migration");
    }

    [Fact]
    public async Task Startup_EmptyDatabase_AppliesThePlatformMigrations()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();

        // Program.cs migrates the legacy context and then runs every IModuleDatabaseMigrator.
        await using (var factory = new ProcureAccessApiFactory(connectionString, api.Clock))
        {
            _ = factory.Services;
        }

        await using var platform = Platform(connectionString);
        (await platform.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldBe(platform.Database.GetMigrations());
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        foreach (var connectionString in _databases)
        {
            await using var context = Platform(connectionString);
            await context.Database.EnsureDeletedAsync();
        }
    }

    private string NewDatabase()
    {
        var connectionString = api.ConnectionStringForNewDatabase();
        _databases.Add(connectionString);
        return connectionString;
    }

    private static ApplicationDBContext Legacy(string connectionString) =>
        new(new DbContextOptionsBuilder<ApplicationDBContext>().UseNpgsql(connectionString).Options);

    private static PlatformDbContext Platform(string connectionString) =>
        new(new DbContextOptionsBuilder<PlatformDbContext>().UseNpgsql(connectionString).Options);

    /// <summary>
    /// Columns (type, length, nullability, default, identity) and index definitions of the Platform
    /// schemas, one line each, in a fixed order. Column order is left out: the legacy migrations
    /// re-added <c>UsageEvents.Id</c> at the end (#124).
    /// </summary>
    private static async Task<List<string>> LayoutAsync(string connectionString)
    {
        const string sql = """
            SELECT table_schema || '.' || table_name || '.' || column_name || ' ' || data_type
                   || coalesce('(' || character_maximum_length || ')', '')
                   || ' nullable=' || is_nullable || ' default=' || coalesce(column_default, '-')
                   || ' identity=' || is_identity || coalesce(' ' || identity_generation, '')
            FROM information_schema.columns
            WHERE table_schema = ANY(@schemas)
            UNION ALL
            SELECT indexdef FROM pg_indexes WHERE schemaname = ANY(@schemas)
            ORDER BY 1
            """;
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("schemas", PlatformSchemas);
        var lines = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lines.Add(reader.GetString(0));
        }

        return lines;
    }

    private static async Task<long> ScalarAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }
}
