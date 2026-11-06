// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ProcureAccess.Modules.Catalog;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.Catalog.Reading;
using ProcureAccess.Modules.Catalog.Releases;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using Keys = ProcureAccess.Tests.Shared.Fixtures.MiniCatalog.Keys;

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// <c>catalog activate &lt;version&gt;</c> (S3-17, SPEC R-ADM-01) on the real <c>catalog</c> schema:
/// it switches the one active release and refuses an incomplete or unknown one (CAT-23, activate
/// part); the catalog reads the active release from the database on every call and caches release
/// content by its id (CAT-09).
/// </summary>
/// <remarks>
/// Each test starts on an empty catalog. The API's memory cache lives as long as the fixture, but
/// Respawn deletes rows without resetting the id sequences, so a release of one test never has the
/// id of a release cached by another.
/// </remarks>
public sealed class CatalogActivationTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private static readonly string TestReleasesPath = Path.GetDirectoryName(TestReleaseFolders.PathOf(MiniCatalog.T1Version))!;

    private readonly string _tempReleases = Directory.CreateTempSubdirectory("releases-").FullName;

    public async ValueTask InitializeAsync() => await Api.ResetDatabaseAsync();

    public ValueTask DisposeAsync()
    {
        Directory.Delete(_tempReleases, recursive: true);
        return ValueTask.CompletedTask;
    }

    [Fact(DisplayName = "INV:CAT-23 " + nameof(Command_ActivateTwoReleasesInTurn_SwitchesTheOneActiveRelease))]
    [Trait("Invariant", "CAT-23")]
    public async Task Command_ActivateTwoReleasesInTurn_SwitchesTheOneActiveRelease()
    {
        await ImportTestReleasesAsync();

        var first = await RunAsync("activate", MiniCatalog.T1Version);
        var second = await RunAsync("activate", MiniCatalog.T2Version);
        var rollback = await RunAsync("activate", MiniCatalog.T1Version);

        first.ShouldBe((0, "Activated release T.1; active before: none.", ""));
        second.ShouldBe((0, "Activated release T.2; active before: T.1.", ""));
        rollback.ShouldBe((0, "Activated release T.1; active before: T.2.", ""));
        (await ActiveVersionsAsync()).ShouldBe([MiniCatalog.T1Version]);
    }

    [Fact(DisplayName = "INV:CAT-23 " + nameof(Command_ActivateTheActiveRelease_ChangesNothing))]
    [Trait("Invariant", "CAT-23")]
    public async Task Command_ActivateTheActiveRelease_ChangesNothing()
    {
        await ImportTestReleasesAsync();
        (await RunAsync("activate", MiniCatalog.T2Version)).ExitCode.ShouldBe(0);

        var again = await RunAsync("activate", MiniCatalog.T2Version);

        again.ShouldBe((0, "Release T.2 is already active; nothing changed.", ""));
        (await ActiveVersionsAsync()).ShouldBe([MiniCatalog.T2Version]);
    }

    [Fact(DisplayName = "INV:CAT-23 " + nameof(Command_ActivateIncompleteRelease_NamesEveryGapAndChangesNothing))]
    [Trait("Invariant", "CAT-23")]
    public async Task Command_ActivateIncompleteRelease_NamesEveryGapAndChangesNothing()
    {
        // T.3 = T.1 without the catalog default of 99.3 and without the procedure criterion of
        // bikApp. Import accepts it (O-S3-5 Q2 (iii)); activation refuses it.
        await ImportTestReleasesAsync();
        (await RunAsync("activate", MiniCatalog.T1Version)).ExitCode.ShouldBe(0);
        var files = TestReleaseFolders.FilesOf(MiniCatalog.T1Version);
        files["release.json"] = files["release.json"].Replace("\"version\": \"T.1\"", "\"version\": \"T.3\"", StringComparison.Ordinal);
        var defaultOnly = JsonNode.Parse(files[$"criteria/{Keys.DefaultOnly}.json"])!.AsObject();
        defaultOnly.Remove("priorities");
        files[$"criteria/{Keys.DefaultOnly}.json"] = defaultOnly.ToJsonString();
        files.Remove($"criteria/{Keys.ProcedureBikApp}.json");
        WriteRelease("T.3", files);
        (await ImportAsync(_tempReleases, "T.3")).Errors.ShouldBeEmpty();

        var result = await RunAsync("activate", "T.3");

        result.ExitCode.ShouldBe(1);
        result.Output.ShouldBeEmpty();
        result.Error.ShouldBe(string.Join(
            Environment.NewLine,
            "Release T.3 was not activated; the database is unchanged. 2 error(s):",
            "  Criterion '99.3' has no catalog-default priority (a rule without app type and use case).",
            "  Test type 'bikApp' has no procedure criterion (PROC-bikApp).",
            "A stored release never changes: fix its folder and import it as a new version."));
        (await ActiveVersionsAsync()).ShouldBe([MiniCatalog.T1Version]);
    }

    [Fact(DisplayName = "INV:CAT-23 " + nameof(Command_ActivateUnknownVersion_IsRefused))]
    [Trait("Invariant", "CAT-23")]
    public async Task Command_ActivateUnknownVersion_IsRefused()
    {
        var result = await RunAsync("activate", "2026.1");

        result.ShouldBe((1, "", string.Join(
            Environment.NewLine,
            "Release 2026.1 was not activated; the database is unchanged. 1 error(s):",
            "  Release 2026.1 is not imported; run catalog import 2026.1 first.")));
    }

    [Fact(DisplayName = "INV:CAT-23 " + nameof(Save_SecondActiveRelease_IsRefusedByTheDatabase))]
    [Trait("Invariant", "CAT-23")]
    public async Task Save_SecondActiveRelease_IsRefusedByTheDatabase()
    {
        // Around the command: the partial unique index keeps a second release from becoming active.
        var cancellationToken = TestContext.Current.CancellationToken;
        await ImportTestReleasesAsync();
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        foreach (var release in await db.Releases.ToListAsync(cancellationToken))
        {
            release.Activate();
        }

        var error = await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync(cancellationToken));

        error.InnerException.ShouldBeOfType<PostgresException>().ConstraintName.ShouldBe("IX_CatalogReleases_SingleActive");
        (await ActiveVersionsAsync()).ShouldBeEmpty();
    }

    [Fact(DisplayName = "INV:CAT-09 " + nameof(Read_AfterEachActivation_TheNextReadReturnsTheNewActiveRelease))]
    [Trait("Invariant", "CAT-09")]
    public async Task Read_AfterEachActivation_TheNextReadReturnsTheNewActiveRelease()
    {
        // The command changes only the database, as it does from its own process: the reader learns
        // of the switch from there, not from a signal.
        await ImportTestReleasesAsync();
        (await ReadActiveAsync()).ShouldBeNull();

        (await RunAsync("activate", MiniCatalog.T1Version)).ExitCode.ShouldBe(0);
        var t1 = await ReadActiveAsync();
        (await RunAsync("activate", MiniCatalog.T2Version)).ExitCode.ShouldBe(0);
        var t2 = await ReadActiveAsync();

        var expected = MiniCatalog.Build(MiniCatalogValues.InMemory());
        t1.ShouldNotBeNull().Version.ShouldBe(MiniCatalog.T1Version);
        t1.CriterionKeys.ShouldBe(KeysInListOrder(expected.T1));
        t2.ShouldNotBeNull().Version.ShouldBe(MiniCatalog.T2Version);
        t2.CriterionKeys.ShouldBe(KeysInListOrder(expected.T2));
        t2.Id.ShouldNotBe(t1.Id);
    }

    [Fact(DisplayName = "INV:CAT-09 " + nameof(Read_SameReleaseAgain_ComesFromTheCacheKeyedByReleaseId))]
    [Trait("Invariant", "CAT-09")]
    public async Task Read_SameReleaseAgain_ComesFromTheCacheKeyedByReleaseId()
    {
        await ImportTestReleasesAsync();
        (await RunAsync("activate", MiniCatalog.T1Version)).ExitCode.ShouldBe(0);

        var first = await ReadActiveAsync();
        var second = await ReadActiveAsync();
        (await RunAsync("activate", MiniCatalog.T2Version)).ExitCode.ShouldBe(0);
        var other = await ReadActiveAsync();
        (await RunAsync("activate", MiniCatalog.T1Version)).ExitCode.ShouldBe(0);
        var back = await ReadActiveAsync();

        // One entry per release id: the same object for T.1 each time, another one for T.2.
        second.ShouldBeSameAs(first);
        back.ShouldBeSameAs(first);
        other.ShouldNotBeSameAs(first);
        other.ShouldNotBeNull().Version.ShouldBe(MiniCatalog.T2Version);
    }

    private static List<string> KeysInListOrder(CatalogRelease release) =>
        release.Criteria.Select(c => c.Revision)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Criterion.Key, StringComparer.Ordinal)
            .Select(r => r.Criterion.Key)
            .ToList();

    /// <summary>Reads as a request would: a new scope, the API's singleton cache.</summary>
    private async Task<CatalogReleaseSnapshot?> ReadActiveAsync()
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<CatalogReader>()
            .GetActiveReleaseAsync(TestContext.Current.CancellationToken);
    }

    private async Task<(int ExitCode, string Output, string Error)> RunAsync(params string[] command)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var exitCode = await CatalogModule.RunCommandAsync(
            Api.Factory.Services, ["catalog", .. command], output, error, TestContext.Current.CancellationToken);
        return (exitCode, output.ToString().TrimEnd(), error.ToString().TrimEnd());
    }

    private async Task ImportTestReleasesAsync()
    {
        (await ImportAsync(TestReleasesPath, MiniCatalog.T1Version)).Errors.ShouldBeEmpty();
        Clock.Advance(TimeSpan.FromMinutes(1));
        (await ImportAsync(TestReleasesPath, MiniCatalog.T2Version)).Errors.ShouldBeEmpty();
    }

    private async Task<CatalogImportResult> ImportAsync(string releasesPath, string version)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        return await new CatalogReleaseImporter(db, Clock).ImportAsync(releasesPath, version, TestContext.Current.CancellationToken);
    }

    private async Task<List<string>> ActiveVersionsAsync()
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        return await db.Releases.Where(r => r.IsActive).Select(r => r.Version).ToListAsync(TestContext.Current.CancellationToken);
    }

    private void WriteRelease(string version, Dictionary<string, string> files)
    {
        foreach (var (path, content) in files)
        {
            var target = Path.Combine(_tempReleases, version, path);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.WriteAllText(target, content);
        }
    }
}
