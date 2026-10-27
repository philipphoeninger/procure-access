// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Persistence;

/// <summary>
/// OPS-03: the migrations build the schema from nothing, and the model matches the last migration.
/// Each test uses its own database on the shared container, not the fixture's database, which is
/// already migrated.
/// </summary>
public sealed class MigrationTests(ApiFixture api) : IAsyncDisposable
{
    private readonly ApplicationDBContext _context = ApiFixture.CreateDbContext(api.ConnectionStringForNewDatabase());

    [Fact(DisplayName = "INV:OPS-03 " + nameof(Migrate_EmptyDatabase_AppliesAllMigrations))]
    [Trait("Invariant", "OPS-03")]
    public async Task Migrate_EmptyDatabase_AppliesAllMigrations()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var creator = _context.GetService<IRelationalDatabaseCreator>();
        (await creator.ExistsAsync(cancellationToken)).ShouldBeFalse("the test must start without a database");

        await _context.Database.MigrateAsync(cancellationToken);

        var applied = await _context.Database.GetAppliedMigrationsAsync(cancellationToken);
        var pending = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
        _context.Database.GetMigrations().ShouldNotBeEmpty();
        applied.ShouldBe(_context.Database.GetMigrations());
        pending.ShouldBeEmpty();
    }

    [Fact(DisplayName = "INV:OPS-03 " + nameof(Model_ComparedWithLastMigration_HasNoPendingChanges))]
    [Trait("Invariant", "OPS-03")]
    public void Model_ComparedWithLastMigration_HasNoPendingChanges()
    {
        // Same check as `dotnet ef migrations has-pending-model-changes`.
        _context.Database.HasPendingModelChanges().ShouldBeFalse(
            "the model differs from the last migration's snapshot; add a migration");
    }

    public async ValueTask DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }
}
