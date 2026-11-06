// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Releases;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Catalog.Releases;

/// <summary>
/// The test release folders read without errors and hold exactly the <see cref="MiniCatalog"/>
/// (S3-14 decision 2A): S3-16 can then check that importing them gives the fixture.
/// </summary>
public class ReleaseFolderReaderTests
{
    private static readonly MiniCatalogValues Values = MiniCatalogValues.InMemory();
    private static readonly FilterValueKeys Keys = FilterValueKeys.From(Values.All);

    [Theory]
    [InlineData(MiniCatalog.T1Version)]
    [InlineData(MiniCatalog.T2Version)]
    public void ReadDirectory_TestReleaseFolder_IsValidAndEqualsTheMiniCatalog(string version)
    {
        var catalog = MiniCatalog.Build(Values);
        var release = catalog.Releases.Single(r => r.Version == version);
        var previous = catalog.Releases.TakeWhile(r => r != release).LastOrDefault();

        var result = ReleaseFolderReader.ReadDirectory(TestReleaseFolders.PathOf(version), Keys);

        result.Errors.ShouldBeEmpty();
        ReleaseDefinitions.Json(result.Release!).ShouldBe(ReleaseDefinitions.Json(ReleaseDefinitions.Of(release, previous)));
    }

    [Fact]
    public void ReadDirectory_MissingFolder_NamesIt()
    {
        var missing = Path.Combine(TestReleaseFolders.PathOf(MiniCatalog.T1Version), "..", "T.404");

        var result = ReleaseFolderReader.ReadDirectory(missing, Keys);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldHaveSingleItem().Message.ShouldBe("The release folder does not exist.");
        result.Errors[0].File.ShouldEndWith("T.404");
    }

    [Fact]
    public void ReadDirectory_FolderNameDiffersFromVersion_IsRefused()
    {
        var copy = Directory.CreateTempSubdirectory("release-").FullName;
        try
        {
            var folder = Path.Combine(copy, "T.9");
            foreach (var (path, content) in TestReleaseFolders.FilesOf(MiniCatalog.T1Version))
            {
                var target = Path.Combine(folder, path);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.WriteAllText(target, content);
            }

            var result = ReleaseFolderReader.ReadDirectory(folder, Keys);

            result.Errors.ShouldHaveSingleItem().ShouldBe(
                new ReleaseFolderError("release.json", "$.version", "The version 'T.1' must equal the folder name 'T.9'."));
        }
        finally
        {
            Directory.Delete(copy, recursive: true);
        }
    }
}
