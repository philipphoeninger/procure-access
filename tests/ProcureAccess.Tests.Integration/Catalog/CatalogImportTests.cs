// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.Catalog.Releases;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using Keys = ProcureAccess.Tests.Shared.Fixtures.MiniCatalog.Keys;

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// <c>catalog import &lt;version&gt;</c> (S3-16, SPEC R-ADM-01) on the real <c>catalog</c> schema:
/// the test release folders give exactly the MiniCatalog (S3-14 decision 2A), unchanged criteria
/// share their revision (CAT-07), any error leaves the database unchanged (CAT-23), and stored rows
/// cannot be changed or deleted through EF (CAT-05, import part).
/// </summary>
/// <remarks>Each test starts on an empty catalog: a release can be imported only once.</remarks>
public sealed class CatalogImportTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private static readonly string TestReleasesPath = Path.GetDirectoryName(TestReleaseFolders.PathOf(MiniCatalog.T1Version))!;

    private readonly string _tempReleases = Directory.CreateTempSubdirectory("releases-").FullName;

    public async ValueTask InitializeAsync() => await Api.ResetDatabaseAsync();

    public ValueTask DisposeAsync()
    {
        Directory.Delete(_tempReleases, recursive: true);
        return ValueTask.CompletedTask;
    }

    [Fact(DisplayName = "INV:CAT-23 " + nameof(Import_TestReleasesInOrder_StoresExactlyTheMiniCatalog))]
    [Trait("Invariant", "CAT-23")]
    public async Task Import_TestReleasesInOrder_StoresExactlyTheMiniCatalog()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var t1Time = Clock.GetUtcNow();
        (await ImportAsync(TestReleasesPath, MiniCatalog.T1Version)).Errors.ShouldBeEmpty();
        Clock.Advance(TimeSpan.FromMinutes(1));
        (await ImportAsync(TestReleasesPath, MiniCatalog.T2Version)).Errors.ShouldBeEmpty();

        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var stored = await LoadReleasesAsync(db, cancellationToken);
        var expected = MiniCatalog.Build(MiniCatalogValues.InMemory());

        stored.Select(r => r.Version).ShouldBe([MiniCatalog.T1Version, MiniCatalog.T2Version]);
        stored.Select(r => r.PublishedAt).ShouldBe([t1Time, t1Time.AddMinutes(1)]);
        stored.ShouldAllBe(r => !r.IsActive);
        ReleaseDefinitions.SortedJson(ReleaseDefinitions.Of(stored[0], null))
            .ShouldBe(ReleaseDefinitions.SortedJson(ReleaseDefinitions.Of(expected.T1, null)));
        ReleaseDefinitions.SortedJson(ReleaseDefinitions.Of(stored[1], stored[0]))
            .ShouldBe(ReleaseDefinitions.SortedJson(ReleaseDefinitions.Of(expected.T2, expected.T1)));
        stored[0].Criteria.Select(c => c.Revision.Criterion)
            .Where(c => c.SupersededByKey is not null)
            .Select(c => (c.Key, c.SupersededByKey))
            .OrderBy(c => c.Key, StringComparer.Ordinal)
            .ShouldBe([(Keys.MergedA, Keys.Merged), (Keys.MergedB, Keys.Merged), (Keys.RenamedOld, Keys.RenamedNew)]);
    }

    [Fact(DisplayName = "INV:CAT-07 " + nameof(Import_SecondRelease_AddsRevisionsOnlyForChangedCriteria))]
    [Trait("Invariant", "CAT-07")]
    public async Task Import_SecondRelease_AddsRevisionsOnlyForChangedCriteria()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var t1 = await ImportAsync(TestReleasesPath, MiniCatalog.T1Version);
        Clock.Advance(TimeSpan.FromMinutes(1));
        var t2 = await ImportAsync(TestReleasesPath, MiniCatalog.T2Version);

        t1.Summary.ShouldBe(new CatalogImportSummary(MiniCatalog.T1Version, 12, 12, 12, 0, null));
        // 99.6 changed, 99.9 renamed, 99.10 merged, 99.11 added: four new revisions, seven shared.
        t2.Summary.ShouldBe(new CatalogImportSummary(MiniCatalog.T2Version, 11, 3, 4, 7, MiniCatalog.T1Version));

        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var stored = await LoadReleasesAsync(db, cancellationToken);
        (await db.CriterionRevisions.CountAsync(cancellationToken)).ShouldBe(16);
        (await db.Criteria.CountAsync(cancellationToken)).ShouldBe(15);
        var t1Revisions = stored[0].Criteria.Select(c => c.CriterionRevisionId).ToHashSet();
        stored[1].Criteria.Count(c => t1Revisions.Contains(c.CriterionRevisionId)).ShouldBe(7);
        stored.Select(r => MiniCatalog.RevisionOf(r, Keys.Changed))
            .Select(r => (r.CriterionId, r.RevisionNumber))
            .ShouldBe([(MiniCatalog.RevisionOf(stored[0], Keys.Changed).CriterionId, 1),
                (MiniCatalog.RevisionOf(stored[0], Keys.Changed).CriterionId, 2)]);

        // Rows the releases share: the one standard reference and the five unchanged rules.
        (await db.StandardReferences.CountAsync(cancellationToken)).ShouldBe(1);
        (await db.FilterExclusions.CountAsync(cancellationToken)).ShouldBe(7);
    }

    [Fact(DisplayName = "INV:CAT-23 " + nameof(Import_InvalidFolder_ReportsTheErrorsAndWritesNothing))]
    [Trait("Invariant", "CAT-23")]
    public async Task Import_InvalidFolder_ReportsTheErrorsAndWritesNothing()
    {
        var files = TestReleaseFolders.FilesOf(MiniCatalog.T1Version);
        files["criteria/99.4.json"] = files["criteria/99.4.json"].Replace("\"audio\"", "\"podcast\"", StringComparison.Ordinal);
        WriteRelease(MiniCatalog.T1Version, files);
        var before = await CountCatalogRowsAsync();

        var result = await ImportAsync(_tempReleases, MiniCatalog.T1Version);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldAllBe(e => e.File == "criteria/99.4.json");
        (await CountCatalogRowsAsync()).ShouldBe(before);
    }

    [Fact(DisplayName = "INV:CAT-23 " + nameof(Import_SupersededKeyNotInPreviousRelease_WritesNothing))]
    [Trait("Invariant", "CAT-23")]
    public async Task Import_SupersededKeyNotInPreviousRelease_WritesNothing()
    {
        // T.2 is a valid folder, but its successors name criteria of T.1, which is not stored: the
        // import finds out only while it builds the release, after it has created criteria in memory.
        var before = await CountCatalogRowsAsync();

        var result = await ImportAsync(TestReleasesPath, MiniCatalog.T2Version);

        result.Errors.Select(e => e.ToString()).ShouldBe(
        [
            "criteria/99.10.json $.supersedes[0]: '99.10a' cannot be replaced: no release is stored yet.",
            "criteria/99.10.json $.supersedes[1]: '99.10b' cannot be replaced: no release is stored yet.",
            "criteria/99.9.json $.supersedes[0]: '99.8' cannot be replaced: no release is stored yet.",
        ], ignoreOrder: true);
        (await CountCatalogRowsAsync()).ShouldBe(before);
    }

    [Fact(DisplayName = "INV:CAT-05 " + nameof(Import_VersionAlreadyStored_IsRefused))]
    [Trait("Invariant", "CAT-05")]
    public async Task Import_VersionAlreadyStored_IsRefused()
    {
        (await ImportAsync(TestReleasesPath, MiniCatalog.T1Version)).Succeeded.ShouldBeTrue();
        var before = await CountCatalogRowsAsync();

        var again = await ImportAsync(TestReleasesPath, MiniCatalog.T1Version);

        again.Errors.ShouldHaveSingleItem().ShouldBe(new ReleaseFolderError(
            "release.json", "$.version", "Release T.1 is already imported; a release never changes."));
        (await CountCatalogRowsAsync()).ShouldBe(before);
    }

    [Fact(DisplayName = "INV:CAT-23 " + nameof(Import_StoredCriterionWithOtherCategory_IsRefused))]
    [Trait("Invariant", "CAT-23")]
    public async Task Import_StoredCriterionWithOtherCategory_IsRefused()
    {
        (await ImportAsync(TestReleasesPath, MiniCatalog.T1Version)).Succeeded.ShouldBeTrue();
        var files = ReleaseFiles(MiniCatalog.T2Version, "T.3");
        files["criteria/99.3.json"] = files["criteria/99.3.json"].Replace(
            "\"kind\": \"requirement\",", "\"kind\": \"requirement\",\n  \"category\": \"Neu\",", StringComparison.Ordinal);
        WriteRelease("T.3", files);
        var before = await CountCatalogRowsAsync();

        var result = await ImportAsync(_tempReleases, "T.3");

        result.Errors.ShouldHaveSingleItem().ShouldBe(new ReleaseFolderError(
            "criteria/99.3.json",
            "$.category",
            "Criterion '99.3' is stored with category '(none)'; the category of a stored criterion never changes."));
        (await CountCatalogRowsAsync()).ShouldBe(before);
    }

    [Fact(DisplayName = "INV:CAT-07 " + nameof(Import_CorrectedUrl_IsANewReferenceOfANewRevision))]
    [Trait("Invariant", "CAT-07")]
    public async Task Import_CorrectedUrl_IsANewReferenceOfANewRevision()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        (await ImportAsync(TestReleasesPath, MiniCatalog.T1Version)).Succeeded.ShouldBeTrue();
        var files = ReleaseFiles(MiniCatalog.T1Version, "T.3");
        foreach (var file in files.Keys.Where(f => files[f].Contains("https://example.org/en301549#T.9.1.1", StringComparison.Ordinal)).ToList())
        {
            files[file] = files[file].Replace("https://example.org/en301549#T.9.1.1", "https://example.org/en301549/v2#T.9.1.1", StringComparison.Ordinal);
        }

        WriteRelease("T.3", files);
        Clock.Advance(TimeSpan.FromMinutes(1));

        var result = await ImportAsync(_tempReleases, "T.3");

        // 99.3 and 99.6 cite the clause; only their revisions are new.
        result.Summary.ShouldBe(new CatalogImportSummary("T.3", 12, 0, 2, 10, MiniCatalog.T1Version));
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var stored = await LoadReleasesAsync(db, cancellationToken);
        (await db.StandardReferences.CountAsync(cancellationToken)).ShouldBe(2);
        MiniCatalog.RevisionOf(stored[0], Keys.DefaultOnly).StandardReferences.ShouldHaveSingleItem().Url
            .ShouldBe("https://example.org/en301549#T.9.1.1");
        var corrected = MiniCatalog.RevisionOf(stored[1], Keys.DefaultOnly);
        corrected.RevisionNumber.ShouldBe(2);
        corrected.StandardReferences.ShouldHaveSingleItem().Url.ShouldBe("https://example.org/en301549/v2#T.9.1.1");
    }

    [Fact(DisplayName = "INV:CAT-05 " + nameof(Save_ChangeOrDeleteOfImportedRows_IsRefused))]
    [Trait("Invariant", "CAT-05")]
    public async Task Save_ChangeOrDeleteOfImportedRows_IsRefused()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        (await ImportAsync(TestReleasesPath, MiniCatalog.T1Version)).Succeeded.ShouldBeTrue();

        // Around the entities' methods, through the change tracker: each is refused on save.
        await ShouldRefuseAsync(async db =>
        {
            var revision = await db.CriterionRevisions.FirstAsync(cancellationToken);
            db.Entry(revision).Property(r => r.TitleDe).CurrentValue = "Geändert";
        });
        await ShouldRefuseAsync(async db =>
        {
            var release = await db.Releases.SingleAsync(cancellationToken);
            db.Entry(release).Property(r => r.Version).CurrentValue = "T.9";
        });
        await ShouldRefuseAsync(async db => db.Releases.Remove(await db.Releases.SingleAsync(cancellationToken)));
        await ShouldRefuseAsync(async db => db.StandardReferences.Remove(await db.StandardReferences.SingleAsync(cancellationToken)));

        // Allowed: activation, and a successor key once.
        await using (var scope = Api.Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            (await db.Releases.SingleAsync(cancellationToken)).Activate();
            (await db.Criteria.SingleAsync(c => c.Key == Keys.Removed, cancellationToken)).MarkSupersededBy("99.12");
            await db.SaveChangesAsync(cancellationToken);
        }

        await ShouldRefuseAsync(async db =>
        {
            var criterion = await db.Criteria.SingleAsync(c => c.Key == Keys.Removed, cancellationToken);
            db.Entry(criterion).Property(c => c.SupersededByKey).CurrentValue = "99.13";
        });
        await using var readScope = Api.Factory.Services.CreateAsyncScope();
        var readDb = readScope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        (await readDb.Releases.SingleAsync(cancellationToken)).IsActive.ShouldBeTrue();
        (await readDb.Criteria.SingleAsync(c => c.Key == Keys.Removed, cancellationToken)).SupersededByKey.ShouldBe("99.12");

        async Task ShouldRefuseAsync(Func<CatalogDbContext, Task> change)
        {
            await using var scope = Api.Factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            await change(db);
            await Should.ThrowAsync<PublishedCatalogChangeException>(() => db.SaveChangesAsync(cancellationToken));
        }
    }

    [Fact(DisplayName = "INV:CAT-23 " + nameof(Command_ImportAndRepeat_ExitsWithItsCodeAndNamesTheErrors))]
    [Trait("Invariant", "CAT-23")]
    public async Task Command_ImportAndRepeat_ExitsWithItsCodeAndNamesTheErrors()
    {
        var services = new ConfiguredServices(Api.Factory.Services, TestReleasesPath);
        var output = new StringWriter();
        var error = new StringWriter();

        var first = await CatalogModule.RunCommandAsync(services, ["catalog", "import", MiniCatalog.T1Version], output, error, TestContext.Current.CancellationToken);
        var second = await CatalogModule.RunCommandAsync(services, ["catalog", "import", MiniCatalog.T1Version], output, error, TestContext.Current.CancellationToken);

        first.ShouldBe(0);
        output.ToString().ShouldBe(
            "Imported release T.1 (inactive): 12 criteria, 12 new; 12 new revision(s), 0 shared with no previous release." +
            Environment.NewLine);
        second.ShouldBe(1);
        error.ToString().ShouldBe(
            "Release T.1 was not imported; the database is unchanged. 1 error(s):" + Environment.NewLine +
            "  release.json $.version: Release T.1 is already imported; a release never changes." + Environment.NewLine);
    }

    private async Task<CatalogImportResult> ImportAsync(string releasesPath, string version)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        return await new CatalogReleaseImporter(db, Clock).ImportAsync(releasesPath, version, TestContext.Current.CancellationToken);
    }

    /// <summary>Every release with all it holds, oldest first; the filter values are loaded first, so EF links them.</summary>
    private static async Task<List<CatalogRelease>> LoadReleasesAsync(CatalogDbContext db, CancellationToken cancellationToken)
    {
        await MiniCatalogValues.LoadAsync(db, cancellationToken);
        await db.FilterExclusions.LoadAsync(cancellationToken);
        return await db.Releases
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Criterion)
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.StandardReferences)
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Applicability)
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Priorities)
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.ProcedureTexts)
            .Include(r => r.Exclusions)
            .Include(r => r.Procedures)
            .AsSplitQuery()
            .OrderBy(r => r.PublishedAt).ThenBy(r => r.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Row count of every catalog table except the seeded filter values.</summary>
    private async Task<Dictionary<string, int>> CountCatalogRowsAsync()
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        string[] seeded = ["UseCaseTemplates", "AppTypes", "ProductParts", "TestTypes"];
        var tables = db.Model.GetEntityTypes().Select(e => e.GetTableName()!).Distinct().Where(t => !seeded.Contains(t)).Order().ToList();
        var counts = new Dictionary<string, int>();
        foreach (var table in tables)
        {
            // Table names come from the model, not from input.
#pragma warning disable EF1002
            counts[table] = await db.Database
                .SqlQueryRaw<int>($"SELECT count(*)::int AS \"Value\" FROM {CatalogDbContext.Schema}.\"{table}\"")
                .SingleAsync(TestContext.Current.CancellationToken);
#pragma warning restore EF1002
        }

        tables.Count.ShouldBeGreaterThan(10);
        return counts;
    }

    /// <summary>The files of a test release, renamed to <paramref name="version"/>.</summary>
    private static Dictionary<string, string> ReleaseFiles(string from, string version)
    {
        var files = TestReleaseFolders.FilesOf(from);
        files["release.json"] = files["release.json"].Replace($"\"version\": \"{from}\"", $"\"version\": \"{version}\"", StringComparison.Ordinal);
        return files;
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

    /// <summary>The API's services with <c>Catalog:ReleasesPath</c> pointing to <paramref name="releasesPath"/>.</summary>
    private sealed class ConfiguredServices(IServiceProvider inner, string releasesPath) : IServiceProvider
    {
        private readonly IConfiguration _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Catalog:ReleasesPath"] = releasesPath })
            .Build();

        public object? GetService(Type serviceType) =>
            serviceType == typeof(IConfiguration) ? _configuration : inner.GetService(serviceType);
    }
}
