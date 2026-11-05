// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.Platform.Persistence;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Persistence;

/// <summary>
/// OPS-03 for the Catalog module's own context (ARCHITECTURE ADR-014 rule 6): its migrations apply
/// to an empty database after the legacy and Platform ones (as at startup) and on their own, record
/// themselves in <c>catalog."__EFMigrationsHistory"</c>, and its model matches the last migration.
/// Each test uses databases of its own on the shared container.
/// </summary>
public sealed class CatalogMigrationTests(ApiFixture api) : IAsyncDisposable
{
    private readonly List<string> _databases = [];

    [Fact(DisplayName = "INV:OPS-03 " + nameof(Migrate_EmptyDatabaseAfterLegacyAndPlatform_AppliesAllMigrationsWithOwnHistory))]
    [Trait("Invariant", "OPS-03")]
    public async Task Migrate_EmptyDatabaseAfterLegacyAndPlatform_AppliesAllMigrationsWithOwnHistory()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();

        await using var legacy = Legacy(connectionString);
        await using var platform = Platform(connectionString);
        await using var catalog = Catalog(connectionString);
        await legacy.Database.MigrateAsync(cancellationToken);
        await platform.Database.MigrateAsync(cancellationToken);
        await catalog.Database.MigrateAsync(cancellationToken);

        catalog.Database.GetMigrations().ShouldNotBeEmpty();
        (await catalog.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldBe(catalog.Database.GetMigrations());
        (await catalog.Database.GetPendingMigrationsAsync(cancellationToken)).ShouldBeEmpty();

        // Each context only knows its own migrations.
        (await legacy.Database.GetAppliedMigrationsAsync(cancellationToken))
            .ShouldNotContain(id => catalog.Database.GetMigrations().Contains(id));
        (await platform.Database.GetAppliedMigrationsAsync(cancellationToken))
            .ShouldNotContain(id => catalog.Database.GetMigrations().Contains(id));
        (await ScalarAsync(connectionString, """SELECT count(*) FROM catalog."__EFMigrationsHistory" """))
            .ShouldBe(catalog.Database.GetMigrations().Count());
    }

    [Fact(DisplayName = "INV:OPS-03 " + nameof(CatalogMigrations_EmptyDatabaseWithoutOtherContexts_ApplyAll))]
    [Trait("Invariant", "OPS-03")]
    public async Task CatalogMigrations_EmptyDatabaseWithoutOtherContexts_ApplyAll()
    {
        // Nothing moves from the legacy context (O-S3-5 C), so the catalog schema does not depend on it.
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var catalog = Catalog(NewDatabase());

        await catalog.Database.MigrateAsync(cancellationToken);

        (await catalog.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldBe(catalog.Database.GetMigrations());
    }

    [Fact(DisplayName = "INV:OPS-03 " + nameof(CatalogModel_ComparedWithLastMigration_HasNoPendingChanges))]
    [Trait("Invariant", "OPS-03")]
    public async Task CatalogModel_ComparedWithLastMigration_HasNoPendingChanges()
    {
        // Same check as `dotnet ef migrations has-pending-model-changes --context CatalogDbContext`.
        await using var catalog = Catalog(NewDatabase());

        catalog.Database.HasPendingModelChanges().ShouldBeFalse(
            "the Catalog model differs from the last migration's snapshot; add a migration");
    }

    [Fact]
    public async Task Startup_EmptyDatabase_AppliesTheCatalogMigrations()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();

        // Program.cs migrates the legacy context and then runs every IModuleDatabaseMigrator.
        await using (var factory = new ProcureAccessApiFactory(connectionString, api.Clock))
        {
            _ = factory.Services;
        }

        await using var catalog = Catalog(connectionString);
        (await catalog.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldBe(catalog.Database.GetMigrations());
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        foreach (var connectionString in _databases)
        {
            await using var context = Catalog(connectionString);
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

    private CatalogDbContext Catalog(string connectionString) =>
        new(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(connectionString).Options, api.Clock);

    private static async Task<long> ScalarAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }
}
