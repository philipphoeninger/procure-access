// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// The API applies the migrations at startup and stops when that fails, instead of serving
/// requests against a database whose schema does not match the model. The error is stored in
/// <c>logging."SeriLogs"</c> before the process ends.
/// </summary>
public sealed class MigrationStartupTests(ApiFixture api) : IAsyncDisposable
{
    private const string FailureMessage = "Applying the database migrations failed; the API stops.";

    private readonly string _connectionString = api.ConnectionStringForNewDatabase();

    [Fact]
    public async Task Startup_MigrationFails_StopsAndStoresTheError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        // A real migration error with the database reachable: all migrations are applied, then the
        // last one is removed from the history, so the startup runs it again and its SQL fails on
        // the objects it already created. The log table exists, because it is not the last one.
        string lastMigration;
        await using (var context = CreateDbContext())
        {
            await context.Database.MigrateAsync(cancellationToken);
            lastMigration = context.Database.GetMigrations().Last();
            await context.Database.ExecuteSqlAsync(
                $"""DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" = {lastMigration}""",
                cancellationToken);
        }

        await using var factory = new ProcureAccessApiFactory(_connectionString, api.Clock);

        var exception = Should.Throw<Exception>(() => factory.Services);

        exception.ShouldBeAssignableTo<PostgresException>();

        // No waiting: the entry must be there as soon as the startup has failed, i.e. the batched
        // sink was flushed before the exception left Program.cs.
        await using (var context = CreateDbContext())
        {
            var entry = await context.SeriLogEntries.AsNoTracking()
                .SingleOrDefaultAsync(e => e.MessageTemplate == FailureMessage, cancellationToken);

            entry.ShouldNotBeNull("the startup failure was not stored in logging.\"SeriLogs\"");
            entry.Level.ShouldBe("Fatal");
            entry.Exception.ShouldNotBeNull().ShouldContain(nameof(PostgresException));
            (await context.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldNotContain(lastMigration);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await using var context = CreateDbContext();
        await context.Database.EnsureDeletedAsync();
    }

    private ApplicationDBContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseNpgsql(_connectionString)
            .Options);
}
