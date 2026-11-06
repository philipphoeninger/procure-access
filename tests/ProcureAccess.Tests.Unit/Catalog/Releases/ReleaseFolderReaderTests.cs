// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using ProcureAccess.Modules.Catalog.Domain;
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
        Json(result.Release!).ShouldBe(Json(DefinitionOf(release, previous)));
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

    private static string Json(ReleaseDefinition definition) =>
        JsonSerializer.Serialize(definition, new JsonSerializerOptions { WriteIndented = true });

    /// <summary>
    /// The definition a folder of <paramref name="release"/> must give. A criterion of the release
    /// supersedes the criteria of <paramref name="previous"/> whose <c>SupersededByKey</c> names it.
    /// </summary>
    private static ReleaseDefinition DefinitionOf(CatalogRelease release, CatalogRelease? previous)
    {
        var predecessors = (previous?.Criteria ?? []).Select(c => c.Revision.Criterion)
            .Where(c => c.SupersededByKey is not null)
            .ToLookup(c => c.SupersededByKey!, c => c.Key);
        var criteria = release.Criteria.Select(c => c.Revision)
            .OrderBy(r => r.Criterion.Key, StringComparer.Ordinal)
            .Select(r => new CriterionDefinition(
                r.Criterion.Key,
                r.Criterion.Kind,
                r.Criterion.TestType?.Key,
                r.Criterion.Category,
                [.. predecessors[r.Criterion.Key].Order(StringComparer.Ordinal)],
                r.TitleDe,
                r.PlainLanguageDe,
                r.RationaleDe,
                r.SortOrder,
                [.. r.StandardReferences.Select(s => new StandardReferenceDefinition(s.Standard, s.Clause, s.Url))],
                [.. r.Applicability.Select(a => new ApplicabilityDefinition(a.AppType.Key, a.ProductPart?.Key))],
                [.. r.Priorities.Select(p => new PriorityDefinition(p.AppType?.Key, p.UseCase?.Key, p.Level, p.Weight, p.RationaleDe))],
                [.. r.ProcedureTexts.Select(t => new ProcedureTextDefinition(t.TestType.Key, new ProcedureTextContent(
                    t.OriginalNumber, t.OriginalTitleDe, t.AllowedRatings, t.SourceUrl, t.ApplicabilityDe, t.RequirementDe,
                    t.RationaleWhyDe, t.PositiveExamplesDe, t.NegativeExamplesDe)))]))
            .ToList();
        var exclusions = release.Exclusions.Select(e => e.Exclusion)
            .Select(e => new ExclusionDefinition(Reference(e.Source!), Reference(e.Target!), e.ReasonDe))
            .ToList();
        var procedures = release.Procedures.Select(p => new ProcedureMappingDefinition(p.AppType.Key, p.TestType.Key)).ToList();
        return new ReleaseDefinition(release.Version, release.ReleaseNotes, criteria, exclusions, procedures);

        static FilterValueReference Reference(FilterValue value) => new(value.Layer, value.Key);
    }
}
