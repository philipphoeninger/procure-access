// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Cryptography;
using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using MODELS.ProcureAccess.Entities.Configuration;
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
    private const string InsufficientPrivilege = "42501";

    private readonly string _adminConnectionString = api.ConnectionStringForNewDatabase();
    private readonly string _role = $"migration_test_{Guid.NewGuid():N}";
    private readonly string _rolePassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

    [Fact]
    public async Task Startup_MigrationFails_StopsAndStoresTheError()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        // A migration error with the database reachable, whatever the migrations contain: the API
        // connects as a role that may read the migration history and write the log table, but not
        // change the schema. One migration is left pending, so Migrate() has work to do.
        string lastMigration;
        await using (var context = CreateDbContext(_adminConnectionString))
        {
            await context.Database.MigrateAsync(cancellationToken);
            lastMigration = context.Database.GetMigrations().Last();
            await context.Database.ExecuteSqlAsync(
                $"""DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" = {lastMigration}""",
                cancellationToken);

            // Identifiers cannot be parameters; role name and password are generated above.
            var logTable = $"{SeriLogEntryConfiguration.Schema}.\"{SeriLogEntryConfiguration.Table}\"";
#pragma warning disable EF1002 // no user input in this SQL
            await context.Database.ExecuteSqlRawAsync(
                $"""
                CREATE ROLE {_role} LOGIN PASSWORD '{_rolePassword}';
                GRANT SELECT ON "__EFMigrationsHistory" TO {_role};
                GRANT USAGE ON SCHEMA {SeriLogEntryConfiguration.Schema} TO {_role};
                GRANT INSERT ON {logTable} TO {_role};
                GRANT USAGE ON ALL SEQUENCES IN SCHEMA {SeriLogEntryConfiguration.Schema} TO {_role};
                """,
                cancellationToken);
#pragma warning restore EF1002
        }

        var apiConnectionString = new NpgsqlConnectionStringBuilder(_adminConnectionString)
        {
            Username = _role,
            Password = _rolePassword,
        }.ConnectionString;
        await using var factory = new ProcureAccessApiFactory(apiConnectionString, api.Clock);

        var exception = Should.Throw<Exception>(() => factory.Services);

        exception.ShouldBeOfType<PostgresException>().SqlState.ShouldBe(InsufficientPrivilege);

        // No waiting: the entry must be there as soon as the startup has failed, i.e. the batched
        // sink was flushed before the exception left Program.cs.
        await using (var context = CreateDbContext(_adminConnectionString))
        {
            var entry = await context.SeriLogEntries.AsNoTracking()
                .SingleOrDefaultAsync(e => e.MessageTemplate == Program.StartupFailedMessage, cancellationToken);

            entry.ShouldNotBeNull("the startup failure was not stored in logging.\"SeriLogs\"");
            entry.Level.ShouldBe("Fatal");
            entry.Exception.ShouldNotBeNull().ShouldContain(nameof(PostgresException));
            (await context.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldNotContain(lastMigration);
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Pooled connections of the failed API would keep the database in use.
        NpgsqlConnection.ClearAllPools();
        await using (var context = CreateDbContext(_adminConnectionString))
        {
            await context.Database.EnsureDeletedAsync();
        }

        // Roles belong to the server, not to the database.
        var serverConnectionString = new NpgsqlConnectionStringBuilder(_adminConnectionString)
        {
            Database = "postgres",
        }.ConnectionString;
        await using var connection = new NpgsqlConnection(serverConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP ROLE IF EXISTS {_role}", connection);
        await command.ExecuteNonQueryAsync();
    }

    private static ApplicationDBContext CreateDbContext(string connectionString) =>
        new(new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseNpgsql(connectionString)
            .Options);
}
