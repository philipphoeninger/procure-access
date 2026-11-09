// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.IdentityAccess.Persistence.Configurations;
using ProcureAccess.Modules.Platform.Persistence;
using Respawn.Graph;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace ProcureAccess.Tests.Integration.Infrastructure;

/// <summary>
/// Assembly fixture (TESTING §5.2): starts one PostgreSQL 17 container, applies the migrations
/// of every context once (legacy first, then the modules, as <c>Program.cs</c> does) and hosts the
/// API once for the whole test run.
/// </summary>
/// <remarks>
/// Test classes derive from <see cref="IntegrationTest"/>, which empties the database before
/// each class. Respawn deletes every row except the migration histories, the four catalog
/// filter-value tables and the Data Protection key ring, including the reference data that migration <c>AddSeedData</c> inserts:
/// each test stores the data it needs. The filter values (use cases, app types, product parts, test
/// types) are static reference data that only the catalog migration writes (O-S3-5 D1), so tests
/// read them instead of storing their own.
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

    /// <summary>The seeded filter-value tables Respawn keeps (O-S3-5 D1, S3-11 P6).</summary>
    private static readonly Table[] CatalogFilterValueTables =
    [
        new(CatalogDbContext.Schema, "UseCaseTemplates"),
        new(CatalogDbContext.Schema, "AppTypes"),
        new(CatalogDbContext.Schema, "ProductParts"),
        new(CatalogDbContext.Schema, "TestTypes"),
    ];

    /// <summary>
    /// The Data Protection key ring (S3-24). The API host caches it, so deleting the rows under a
    /// running host would only make the database and the cache disagree.
    /// </summary>
    private static readonly Table DataProtectionKeysTable =
        new(IdentityAccessDbContext.Schema, DataProtectionKeyConfiguration.Table);

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

        // Migrate here as well as in Program.cs: a broken migration then fails the fixture with
        // the migration error itself, before any API host or test runs.
        await using (var context = CreateDbContext())
        {
            await context.Database.MigrateAsync();
        }

        await using (var context = CreatePlatformDbContext())
        {
            await context.Database.MigrateAsync();
        }

        await using (var context = CreateIdentityAccessDbContext())
        {
            await context.Database.MigrateAsync();
        }

        await using (var context = CreateCatalogDbContext())
        {
            await context.Database.MigrateAsync();
        }

        await using (var connection = await OpenConnectionAsync())
        {
            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore =
                [
                    new Table("public", "__EFMigrationsHistory"),
                    new Table(PlatformDbContext.HistorySchema, PlatformDbContext.HistoryTable),
                    new Table(IdentityAccessDbContext.Schema, IdentityAccessDbContext.HistoryTable),
                    new Table(CatalogDbContext.Schema, CatalogDbContext.HistoryTable),
                    .. CatalogFilterValueTables,
                    DataProtectionKeysTable,
                ],
            });
        }

        _factory = new ProcureAccessApiFactory(_database.GetConnectionString(), Clock);
    }

    /// <summary>Deletes all rows except the migration histories, the catalog filter values and the Data Protection keys.</summary>
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
    /// Stores entities through the application's own context that maps them, never through raw
    /// SQL, so the EF configuration is exercised (TESTING §6.1): the module contexts
    /// (<see cref="PlatformDbContext"/>, <see cref="CatalogDbContext"/>) for their entities, the
    /// legacy <see cref="ApplicationDBContext"/> for all others.
    /// </summary>
    public async Task StoreAsync(params object[] entities)
    {
        using var scope = Factory.Services.CreateScope();
        var legacy = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        DbContext[] modules =
        [
            scope.ServiceProvider.GetRequiredService<PlatformDbContext>(),
            scope.ServiceProvider.GetRequiredService<CatalogDbContext>(),
        ];
        foreach (var entity in entities)
        {
            var context = modules.FirstOrDefault(m => m.Model.FindEntityType(entity.GetType()) is not null) ?? legacy;
            context.Add(entity);
        }

        foreach (var module in modules)
        {
            await module.SaveChangesAsync();
        }

        await legacy.SaveChangesAsync();
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

    private PlatformDbContext CreatePlatformDbContext() =>
        new(new DbContextOptionsBuilder<PlatformDbContext>()
            .UseNpgsql(_database.GetConnectionString())
            .Options);

    private IdentityAccessDbContext CreateIdentityAccessDbContext() =>
        new(new DbContextOptionsBuilder<IdentityAccessDbContext>()
            .UseNpgsql(_database.GetConnectionString())
            .Options);

    private CatalogDbContext CreateCatalogDbContext() =>
        new(new DbContextOptionsBuilder<CatalogDbContext>()
                .UseNpgsql(_database.GetConnectionString())
                .Options,
            Clock);

    private async Task<NpgsqlConnection> OpenConnectionAsync()
    {
        var connection = new NpgsqlConnection(_database.GetConnectionString());
        await connection.OpenAsync();
        return connection;
    }
}
