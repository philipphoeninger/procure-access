// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.Catalog.Releases;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// Catalog part of the E2E seeder (TESTING §6.3; S4-08, #241): it gives a stack the active test
/// release <c>T.1</c> and changes only what is missing, so it can run on every start. Where it runs
/// (Development and E2E only) is tested in <c>TestCatalogSeedGuardTests</c>.
/// </summary>
public sealed class TestReleaseSeederTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private static readonly string TestReleasesPath = Path.GetDirectoryName(TestReleaseFolders.PathOf(MiniCatalog.T1Version))!;

    public async ValueTask InitializeAsync() => await Api.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Seed_EmptyCatalog_ImportsAndActivatesTheTestRelease()
    {
        await SeedAsync(TestReleasesPath, MiniCatalog.T1Version);

        (await ReleasesAsync()).ShouldBe([(MiniCatalog.T1Version, true)]);
    }

    [Fact]
    public async Task Seed_SecondTime_ChangesNothing()
    {
        await SeedAsync(TestReleasesPath, MiniCatalog.T1Version);
        var criteria = await CriterionCountAsync();

        await SeedAsync(TestReleasesPath, MiniCatalog.T1Version);

        (await ReleasesAsync()).ShouldBe([(MiniCatalog.T1Version, true)]);
        (await CriterionCountAsync()).ShouldBe(criteria);
    }

    [Fact]
    public async Task Seed_AnotherReleaseIsActive_ImportsTheTestReleaseButLeavesTheOtherActive()
    {
        // A release activated by hand stays the active one. T.0 is T.1 under another version: T.2
        // cannot be imported before T.1, whose criteria it replaces.
        var releases = Directory.CreateTempSubdirectory("releases-").FullName;
        try
        {
            var files = TestReleaseFolders.FilesOf(MiniCatalog.T1Version);
            files["release.json"] = files["release.json"].Replace("\"version\": \"T.1\"", "\"version\": \"T.0\"", StringComparison.Ordinal);
            foreach (var (path, content) in files)
            {
                var target = Path.Combine(releases, "T.0", path);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                await File.WriteAllTextAsync(target, content, TestContext.Current.CancellationToken);
            }

            await ImportAsync(releases, "T.0");
        }
        finally
        {
            Directory.Delete(releases, recursive: true);
        }

        (await RunAsync("activate", "T.0")).ShouldBe(0);

        await SeedAsync(TestReleasesPath, MiniCatalog.T1Version);

        (await ReleasesAsync()).ShouldBe([("T.0", true), (MiniCatalog.T1Version, false)]);
    }

    [Fact]
    public async Task Seed_TestReleaseStoredButInactive_ActivatesItWhenNoReleaseIsActive()
    {
        await ImportAsync(TestReleasesPath, MiniCatalog.T1Version);

        await SeedAsync(TestReleasesPath, MiniCatalog.T1Version);

        (await ReleasesAsync()).ShouldBe([(MiniCatalog.T1Version, true)]);
    }

    [Fact]
    public async Task Seed_FolderMissing_WritesNothing()
    {
        var empty = Directory.CreateTempSubdirectory("releases-").FullName;
        try
        {
            await SeedAsync(empty, MiniCatalog.T1Version);
        }
        finally
        {
            Directory.Delete(empty, recursive: true);
        }

        (await ReleasesAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task Seed_FolderFailsTheImport_ThrowsAndWritesNothing()
    {
        // The stack must not start without its test data: an invalid folder stops the start.
        var releases = Directory.CreateTempSubdirectory("releases-").FullName;
        try
        {
            var folder = Directory.CreateDirectory(Path.Combine(releases, "T.9")).FullName;
            await File.WriteAllTextAsync(Path.Combine(folder, "release.json"), """{ "version": "T.8" }""", TestContext.Current.CancellationToken);

            var error = await Should.ThrowAsync<InvalidOperationException>(() => SeedAsync(releases, "T.9"));

            error.Message.ShouldStartWith("Test release T.9 could not be imported:");
        }
        finally
        {
            Directory.Delete(releases, recursive: true);
        }

        (await ReleasesAsync()).ShouldBeEmpty();
    }

    private Task SeedAsync(string releasesPath, string version) =>
        CatalogModule.SeedTestReleaseAsync(Api.Factory.Services, releasesPath, version, TestContext.Current.CancellationToken);

    private async Task ImportAsync(string releasesPath, string version)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        (await new CatalogReleaseImporter(db, Clock).ImportAsync(releasesPath, version, TestContext.Current.CancellationToken))
            .Errors.ShouldBeEmpty();
    }

    private async Task<int> RunAsync(params string[] command) =>
        await CatalogModule.RunCommandAsync(
            Api.Factory.Services, ["catalog", .. command], TextWriter.Null, TextWriter.Null, TestContext.Current.CancellationToken);

    private async Task<List<(string Version, bool IsActive)>> ReleasesAsync()
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var releases = await db.Releases.OrderBy(r => r.Version)
            .Select(r => new { r.Version, r.IsActive })
            .ToListAsync(TestContext.Current.CancellationToken);
        return releases.Select(r => (r.Version, r.IsActive)).ToList();
    }

    private async Task<int> CriterionCountAsync()
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<CatalogDbContext>().Criteria
            .CountAsync(TestContext.Current.CancellationToken);
    }
}
