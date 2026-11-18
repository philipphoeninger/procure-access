// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProcureAccess.Modules.Workspace.Persistence;
using ProcureAccess.Modules.Platform.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Persistence;

/// <summary>
/// OPS-03 for the Workspace module's own context (ARCHITECTURE ADR-014): its migrations apply
/// to an empty database after the legacy and Platform ones (as at startup), record themselves in
/// <c>workspace."__EFMigrationsHistory"</c>, and its model matches the last migration.
/// Each test uses databases of its own on the shared container.
/// </summary>
public sealed class WorkspaceMigrationTests(ApiFixture api) : IAsyncDisposable
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
        await using var workspace = Workspace(connectionString);
        await legacy.Database.MigrateAsync(cancellationToken);
        await platform.Database.MigrateAsync(cancellationToken);
        await workspace.Database.MigrateAsync(cancellationToken);

        workspace.Database.GetMigrations().ShouldNotBeEmpty();
        (await workspace.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldBe(workspace.Database.GetMigrations());

        // Each context only knows its own migrations.
        (await legacy.Database.GetAppliedMigrationsAsync(cancellationToken))
            .ShouldNotContain(id => workspace.Database.GetMigrations().Contains(id));
        (await platform.Database.GetAppliedMigrationsAsync(cancellationToken))
            .ShouldNotContain(id => workspace.Database.GetMigrations().Contains(id));
        (await ScalarAsync(connectionString, """SELECT count(*) FROM workspace."__EFMigrationsHistory" """))
            .ShouldBe(workspace.Database.GetMigrations().Count());
    }

    [Fact(DisplayName = "INV:OPS-03 " + nameof(WorkspaceModel_ComparedWithLastMigration_HasNoPendingChanges))]
    [Trait("Invariant", "OPS-03")]
    public async Task WorkspaceModel_ComparedWithLastMigration_HasNoPendingChanges()
    {
        // Same check as `dotnet ef migrations has-pending-model-changes --context WorkspaceDbContext`.
        await using var workspace = Workspace(NewDatabase());

        workspace.Database.HasPendingModelChanges().ShouldBeFalse(
            "the Workspace model differs from the last migration's snapshot; add a migration");
    }

    /// <summary>
    /// SIG-03 workspace part (S6-15): the test status of a candidate is never null and is
    /// „Nicht bekannt“ (<c>Unknown</c>) when a row is written without one.
    /// </summary>
    [Fact(DisplayName = "INV:SIG-03 " + nameof(CandidateTestStatus_Schema_NotNullWithDefaultUnknown))]
    [Trait("Invariant", "SIG-03")]
    public async Task CandidateTestStatus_Schema_NotNullWithDefaultUnknown()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();
        await using (var legacy = Legacy(connectionString))
        {
            await legacy.Database.MigrateAsync(cancellationToken);
        }

        await using var workspace = Workspace(connectionString);
        await workspace.Database.MigrateAsync(cancellationToken);

        (await ScalarAsync(connectionString, """
            SELECT count(*) FROM information_schema.columns
            WHERE table_schema = 'workspace' AND table_name = 'CandidateProducts' AND column_name = 'TestStatus'
              AND is_nullable = 'NO' AND column_default LIKE '%Unknown%'
            """)).ShouldBe(1);
        var project = Guid.NewGuid();
        var organization = Guid.NewGuid();
        await ExecuteAsync(connectionString, $"""
            INSERT INTO workspace."ProcurementProjects" ("Id", "OrganizationId", "Name", "Status", "CreatedByUserId", "CreatedAt", "ModifiedAt")
            VALUES ('{project}', '{organization}', 'Projekt', 'Open', 'user', now(), now());
            INSERT INTO workspace."CandidateProducts" ("Id", "OrganizationId", "ProcurementProjectId", "Name", "CreatedAt", "ModifiedAt")
            VALUES ('{Guid.NewGuid()}', '{organization}', '{project}', 'Produkt', now(), now());
            """);
        (await ScalarAsync(connectionString, """SELECT count(*) FROM workspace."CandidateProducts" WHERE "TestStatus" = 'Unknown'""")).ShouldBe(1);
        await Should.ThrowAsync<PostgresException>(() => ExecuteAsync(
            connectionString, """UPDATE workspace."CandidateProducts" SET "TestStatus" = NULL"""));
    }

    [Fact]
    public async Task Startup_EmptyDatabase_AppliesTheWorkspaceMigrations()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();

        // Program.cs migrates the legacy context and then runs every IModuleDatabaseMigrator.
        await using (var factory = new ProcureAccessApiFactory(connectionString, api.Clock))
        {
            _ = factory.Services;
        }

        await using var workspace = Workspace(connectionString);
        (await workspace.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldBe(workspace.Database.GetMigrations());
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        foreach (var connectionString in _databases)
        {
            await using var context = Workspace(connectionString);
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
        new(new DbContextOptionsBuilder<PlatformDbContext>().UseNpgsql(connectionString).Options, FixedOrganizationContext.None);

    private static WorkspaceDbContext Workspace(string connectionString) =>
        new(new DbContextOptionsBuilder<WorkspaceDbContext>().UseNpgsql(connectionString).Options, FixedOrganizationContext.None);

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<long> ScalarAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }
}
