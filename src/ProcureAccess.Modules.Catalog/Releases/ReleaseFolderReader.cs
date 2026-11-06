// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Releases;

/// <summary>
/// Reads a release folder <c>catalog/releases/&lt;version&gt;/</c> (SPEC R-ADM-01, SD20) and
/// validates all of it (<see cref="ReleaseFolderValidator"/>). The format is described in
/// <c>docs/sprints/sprint_03/S3-15_release_data_format.md</c> §2.1.
/// </summary>
/// <remarks>
/// Nothing is written: the import (S3-16) stores a release only when <see cref="ReleaseFolderResult.IsValid"/>.
/// </remarks>
internal static class ReleaseFolderReader
{
    public const string ReleaseFile = "release.json";
    public const string ExclusionsFile = "exclusions.json";
    public const string ProcedureMappingFile = "procedure-mapping.json";
    public const string CriteriaFolder = "criteria/";

    /// <summary>
    /// Reads every file below <paramref name="directory"/>; its name is the release version.
    /// <paramref name="filterValues"/> are the keys the folder may refer to (the seeded values).
    /// </summary>
    public static ReleaseFolderResult ReadDirectory(string directory, FilterValueKeys filterValues)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentNullException.ThrowIfNull(filterValues);
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        if (!Directory.Exists(root))
        {
            return new ReleaseFolderResult(null, [new ReleaseFolderError(root, "$", "The release folder does not exist.")]);
        }

        var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .ToDictionary(
                path => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'),
                File.ReadAllText,
                StringComparer.Ordinal);
        return Read(Path.GetFileName(root), files, filterValues);
    }

    /// <summary>
    /// Validates the files of a folder named <paramref name="folderName"/>: paths relative to the
    /// folder with <c>/</c> (<c>criteria/5.2.json</c>), contents as text. Used by
    /// <see cref="ReadDirectory"/> and by tests that change one file of a test release.
    /// </summary>
    public static ReleaseFolderResult Read(
        string folderName,
        IReadOnlyDictionary<string, string> files,
        FilterValueKeys filterValues)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folderName);
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(filterValues);
        return new ReleaseFolderValidator(folderName, files, filterValues).Validate();
    }
}
