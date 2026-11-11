// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Releases;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Catalog.Releases;

/// <summary>
/// Every real release folder under <c>catalog/releases/</c> passes the validation of
/// <c>catalog import</c> (CAT-23, S4-05, #239), so a pull request with a new version or with the
/// 2026.1 priorities shows its errors in CI, with file and JSON path. Folders are found, not
/// named: a new version is checked without a new test.
/// </summary>
/// <remarks>
/// <para>The folders are read from the repository, not from a copy in the build output, so a
/// file deleted or renamed locally is never validated from a stale copy.</para>
/// <para>Consistency only, as in the import: priorities and the other completeness rules are
/// checked by <c>catalog activate</c> (O-S3-5 Q2 (iii)), so 2026.1 passes without them. The
/// <c>supersedes</c> check against the previous release needs the database and stays with the
/// import.</para>
/// </remarks>
public class CatalogReleaseFoldersTests
{
    private static readonly FilterValueKeys Keys = SeededFilterValueKeys.Create();

    public static TheoryData<string> Versions => [.. ReleaseFolders()];

    [Theory(DisplayName = "INV:CAT-23 " + nameof(Validate_ReleaseFolder_HasNoErrors))]
    [Trait("Invariant", "CAT-23")]
    [MemberData(nameof(Versions))]
    public void Validate_ReleaseFolder_HasNoErrors(string version)
    {
        var result = ReleaseFolderReader.ReadDirectory(Path.Combine(ReleasesPath(), version), Keys);

        if (result.Errors.Count > 0)
        {
            Assert.Fail(
                $"catalog/releases/{version}/ has {result.Errors.Count} validation error(s), each as <file> <JSON path>: <message>:{Environment.NewLine}"
                + string.Join(Environment.NewLine, result.Errors));
        }
    }

    [Fact(DisplayName = "INV:CAT-23 " + nameof(Find_ReleaseFolders_IncludesRelease2026_1))]
    [Trait("Invariant", "CAT-23")]
    public void Find_ReleaseFolders_IncludesRelease2026_1()
    {
        // Guards the theory above against passing because it found nothing to check.
        ReleaseFolders().ShouldContain("2026.1");
    }

    private static IEnumerable<string> ReleaseFolders() =>
        Directory.EnumerateDirectories(ReleasesPath())
            .Select(Path.GetFileName)
            .OfType<string>()
            .Order(StringComparer.Ordinal);

    /// <summary><c>catalog/releases</c> of the repository the test runs in, found from the test output.</summary>
    private static string ReleasesPath()
    {
        for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder is not null; folder = folder.Parent)
        {
            if (File.Exists(Path.Combine(folder.FullName, "ProcureAccess.sln")))
            {
                return Path.Combine(folder.FullName, "catalog", "releases");
            }
        }

        throw new InvalidOperationException($"No ProcureAccess.sln above {AppContext.BaseDirectory}.");
    }
}
