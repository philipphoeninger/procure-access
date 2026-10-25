// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using Respawn.Graph;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace ProcureAccess.Tests.Integration.Infrastructure;

/// <summary>
/// Assembly fixture (TESTING §5.2): starts one PostgreSQL 17 container, applies the migrations
/// once and hosts the API once for the whole test run.
/// </summary>
/// <remarks>
/// Test classes derive from <see cref="IntegrationTest"/>, which empties the database before
/// each class. Respawn deletes every row except the migration history, including the reference
/// data that migration <c>AddSeedData</c> inserts: each test stores the data it needs.
/// </remarks>
public sealed class ApiFixture : IAsyncLifetime
{
    /// <summary>
    /// Same image as <c>docker-compose.yml</c>, which the deployment uses: PostgreSQL 17 on Alpine
    /// (TESTING §3.1 asks for the production major version).
    /// </summary>
    public const string PostgresImage = "postgres:17-alpine";

    /// <summary>Start time of <see cref="Clock"/>.</summary>
    public static readonly DateTimeOffset ClockStart = new(2026, 3, 2, 9, 0, 0, TimeSpan.Zero);

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder(PostgresImage).Build();

    private Respawner? _respawner;
    private ProcureAccessApiFactory? _factory;

    /// <summary>
    /// The clock the API reads (<see cref="TimeProvider"/>). It only moves when a test calls
    /// <c>Advance</c> or <c>SetUtcNow</c>, and it is not reset between test classes (a fake clock
    /// cannot go back), so tests work with times relative to <c>Clock.GetUtcNow()</c>.
    /// </summary>
    public FakeTimeProvider Clock { get; } = new(ClockStart);

    /// <summary>The API host. One per test run (TESTING §3.1).</summary>
    public ProcureAccessApiFactory Factory =>
        _factory ?? throw new InvalidOperationException("The fixture is not initialised.");

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        // Migrate here rather than relying on Program.cs: Program.cs logs migration errors
        // and starts anyway, which would hide a broken migration behind unrelated test failures.
        await using (var context = CreateDbContext())
        {
            await context.Database.MigrateAsync();
        }

        await using (var connection = await OpenConnectionAsync())
        {
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = [new Table("public", "__EFMigrationsHistory")],
            });
        }

        _factory = new ProcureAccessApiFactory(_database.GetConnectionString(), Clock);
    }

    /// <summary>Deletes all rows except the migration history.</summary>
    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null)
        {
            throw new InvalidOperationException("The fixture is not initialised.");
        }

        await using var connection = await OpenConnectionAsync();
        await _respawner.ResetAsync(connection);
    }

    /// <summary>
    /// Stores entities through the application's own <see cref="ApplicationDBContext"/>, never
    /// through raw SQL, so the EF configuration is exercised (TESTING §6.1).
    /// </summary>
    public async Task StoreAsync(params object[] entities)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        context.AddRange(entities);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Connection string for a database with a new, unique name on the same server. The database
    /// does not exist yet; a test that creates it also drops it. Used where the shared, already
    /// migrated database does not fit, e.g. migrating an empty database (OPS-03).
    /// </summary>
    public string ConnectionStringForNewDatabase() =>
        new NpgsqlConnectionStringBuilder(_database.GetConnectionString())
        {
            Database = $"test_{Guid.NewGuid():N}",
        }.ConnectionString;

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _database.DisposeAsync();
    }

    private ApplicationDBContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseNpgsql(_database.GetConnectionString())
            .Options);

    private async Task<NpgsqlConnection> OpenConnectionAsync()
    {
        var connection = new NpgsqlConnection(_database.GetConnectionString());
        await connection.OpenAsync();
        return connection;
    }
}
