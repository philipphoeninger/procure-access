// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using API.ProcureAccess.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// The test catalog release (S4-08, #241) is seeded at startup only in <c>Development</c> and
/// <c>E2E</c> (owner decision O-S4-3 (b)), never in Staging or Production, even when the settings
/// name a folder. Each test starts the API on a database of its own.
/// </summary>
public sealed class TestCatalogSeedGuardTests(ApiFixture api) : IAsyncDisposable
{
    private static readonly string TestReleasesPath = Path.GetDirectoryName(TestReleaseFolders.PathOf(MiniCatalog.T1Version))!;

    private readonly string _connectionString = api.ConnectionStringForNewDatabase();

    [Theory]
    [InlineData("Testing")]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task Startup_TestReleaseConfiguredOutsideDevelopmentAndE2E_SeedsNothing(string environment)
    {
        var releases = await StartApiAndReadReleasesAsync(environment);

        releases.ShouldBeEmpty();
    }

    /// <summary>
    /// Control for the theory above: the same settings do seed in Development and E2E, so the
    /// theory cannot pass merely because the settings never reached the API.
    /// </summary>
    [Theory]
    [InlineData("Development")]
    [InlineData(TestCatalogSeedExtensions.E2EEnvironmentName)]
    public async Task Startup_TestReleaseConfiguredInDevelopmentOrE2E_ActivatesIt(string environment)
    {
        var releases = await StartApiAndReadReleasesAsync(environment);

        releases.ShouldBe([(MiniCatalog.T1Version, true)]);
    }

    /// <summary>Starts the API (Program.cs runs up to <c>app.Run()</c>), reads the stored releases and stops it.</summary>
    private async Task<List<(string Version, bool IsActive)>> StartApiAndReadReleasesAsync(string environment)
    {
        await using var factory = new ProcureAccessApiFactory(
            _connectionString,
            api.Clock,
            environment,
            new Dictionary<string, string?>
            {
                [TestCatalogSeedExtensions.PathKey] = TestReleasesPath,
                [TestCatalogSeedExtensions.VersionKey] = MiniCatalog.T1Version,
            });

        await using var scope = factory.Services.CreateAsyncScope();
        var releases = await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Releases
            .Select(r => new { r.Version, r.IsActive })
            .ToListAsync(TestContext.Current.CancellationToken);
        return releases.Select(r => (r.Version, r.IsActive)).ToList();
    }

    public async ValueTask DisposeAsync()
    {
        await using var db = new CatalogDbContext(
            new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(_connectionString).Options,
            api.Clock);
        await db.Database.EnsureDeletedAsync();
    }
}
