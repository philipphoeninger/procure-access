// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProcureAccess.Modules.Configurator.Persistence;
using ProcureAccess.Modules.Platform.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Persistence;

/// <summary>
/// OPS-03 for the Configurator module's own context (ARCHITECTURE ADR-014): its migrations apply
/// to an empty database after the legacy and Platform ones (as at startup), record themselves in
/// <c>configurator."__EFMigrationsHistory"</c>, and its model matches the last migration.
/// Each test uses databases of its own on the shared container.
/// </summary>
public sealed class ConfiguratorMigrationTests(ApiFixture api) : IAsyncDisposable
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
        await using var configurator = Configurator(connectionString);
        await legacy.Database.MigrateAsync(cancellationToken);
        await platform.Database.MigrateAsync(cancellationToken);
        await configurator.Database.MigrateAsync(cancellationToken);

        configurator.Database.GetMigrations().ShouldNotBeEmpty();
        (await configurator.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldBe(configurator.Database.GetMigrations());

        // Each context only knows its own migrations.
        (await legacy.Database.GetAppliedMigrationsAsync(cancellationToken))
            .ShouldNotContain(id => configurator.Database.GetMigrations().Contains(id));
        (await platform.Database.GetAppliedMigrationsAsync(cancellationToken))
            .ShouldNotContain(id => configurator.Database.GetMigrations().Contains(id));
        (await ScalarAsync(connectionString, """SELECT count(*) FROM configurator."__EFMigrationsHistory" """))
            .ShouldBe(configurator.Database.GetMigrations().Count());
    }

    [Fact(DisplayName = "INV:OPS-03 " + nameof(ConfiguratorModel_ComparedWithLastMigration_HasNoPendingChanges))]
    [Trait("Invariant", "OPS-03")]
    public async Task ConfiguratorModel_ComparedWithLastMigration_HasNoPendingChanges()
    {
        // Same check as `dotnet ef migrations has-pending-model-changes --context ConfiguratorDbContext`.
        await using var configurator = Configurator(NewDatabase());

        configurator.Database.HasPendingModelChanges().ShouldBeFalse(
            "the Configurator model differs from the last migration's snapshot; add a migration");
    }

    [Fact]
    public async Task Startup_EmptyDatabase_AppliesTheConfiguratorMigrations()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();

        // Program.cs migrates the legacy context and then runs every IModuleDatabaseMigrator.
        await using (var factory = new ProcureAccessApiFactory(connectionString, api.Clock))
        {
            _ = factory.Services;
        }

        await using var configurator = Configurator(connectionString);
        (await configurator.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldBe(configurator.Database.GetMigrations());
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        foreach (var connectionString in _databases)
        {
            await using var context = Configurator(connectionString);
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

    private static ConfiguratorDbContext Configurator(string connectionString) =>
        new(new DbContextOptionsBuilder<ConfiguratorDbContext>().UseNpgsql(connectionString).Options, FixedOrganizationContext.None);

    private static async Task<long> ScalarAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }
}
