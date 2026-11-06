// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Tests.Shared.Fixtures;

/// <summary>
/// The test release folders <c>Fixtures/Releases/T.1</c> and <c>T.2</c> (S3-15): the
/// <see cref="MiniCatalog"/> in the format of <c>catalog/releases/&lt;version&gt;/</c>, written by
/// hand with the same made-up texts. They live under <c>tests/</c>, so no environment can import
/// them as a real release. <c>ReleaseFolderReaderTests</c> checks that they equal the MiniCatalog,
/// and <c>CatalogImportTests</c> that importing them stores it; a change to one needs the same
/// change to the other.
/// </summary>
internal static class TestReleaseFolders
{
    /// <summary>The folder of <paramref name="version"/> in the test output.</summary>
    public static string PathOf(string version) => Path.Combine(AppContext.BaseDirectory, "Fixtures", "Releases", version);

    /// <summary>
    /// The files of <paramref name="version"/>, relative to its folder with <c>/</c>, as a mutable
    /// copy: tests change one file and validate the result.
    /// </summary>
    public static Dictionary<string, string> FilesOf(string version)
    {
        var root = PathOf(version);
        return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .ToDictionary(
                path => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'),
                File.ReadAllText,
                StringComparer.Ordinal);
    }
}
