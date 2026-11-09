// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.Platform.Persistence;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Persistence;

/// <summary>
/// OPS-03 for the IdentityAccess module's own context (ARCHITECTURE ADR-014): its migrations apply
/// to an empty database after the legacy and Platform ones (as at startup), record themselves in
/// <c>identity."__EFMigrationsHistory"</c>, and its model matches the last migration.
/// Each test uses databases of its own on the shared container.
/// </summary>
public sealed class IdentityAccessMigrationTests(ApiFixture api) : IAsyncDisposable
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
        await using var identity = IdentityAccess(connectionString);
        await legacy.Database.MigrateAsync(cancellationToken);
        await platform.Database.MigrateAsync(cancellationToken);
        await identity.Database.MigrateAsync(cancellationToken);

        identity.Database.GetMigrations().ShouldNotBeEmpty();
        (await identity.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldBe(identity.Database.GetMigrations());

        // Each context only knows its own migrations.
        (await legacy.Database.GetAppliedMigrationsAsync(cancellationToken))
            .ShouldNotContain(id => identity.Database.GetMigrations().Contains(id));
        (await platform.Database.GetAppliedMigrationsAsync(cancellationToken))
            .ShouldNotContain(id => identity.Database.GetMigrations().Contains(id));
        (await ScalarAsync(connectionString, """SELECT count(*) FROM identity."__EFMigrationsHistory" """))
            .ShouldBe(identity.Database.GetMigrations().Count());
    }

    [Fact(DisplayName = "INV:OPS-03 " + nameof(IdentityAccessModel_ComparedWithLastMigration_HasNoPendingChanges))]
    [Trait("Invariant", "OPS-03")]
    public async Task IdentityAccessModel_ComparedWithLastMigration_HasNoPendingChanges()
    {
        // Same check as `dotnet ef migrations has-pending-model-changes --context IdentityAccessDbContext`.
        await using var identity = IdentityAccess(NewDatabase());

        identity.Database.HasPendingModelChanges().ShouldBeFalse(
            "the IdentityAccess model differs from the last migration's snapshot; add a migration");
    }

    [Fact]
    public async Task Startup_EmptyDatabase_AppliesTheIdentityAccessMigrations()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();

        // Program.cs migrates the legacy context and then runs every IModuleDatabaseMigrator.
        await using (var factory = new ProcureAccessApiFactory(connectionString, api.Clock))
        {
            _ = factory.Services;
        }

        await using var identity = IdentityAccess(connectionString);
        (await identity.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldBe(identity.Database.GetMigrations());
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        foreach (var connectionString in _databases)
        {
            await using var context = IdentityAccess(connectionString);
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

    private static IdentityAccessDbContext IdentityAccess(string connectionString) =>
        new(new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(connectionString).Options);

    private static async Task<long> ScalarAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }
}
