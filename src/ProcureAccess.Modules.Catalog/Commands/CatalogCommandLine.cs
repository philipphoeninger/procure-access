// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.Catalog.Releases;

namespace ProcureAccess.Modules.Catalog.Commands;

/// <summary>
/// The <c>catalog</c> commands of the API host (SPEC R-ADM-01, SD20; owner, S3-16 decision 1):
/// <c>dotnet API.ProcureAccess.dll catalog import &lt;version&gt;</c>. Release folders are read
/// from <see cref="ReleasesPathKey"/> (S3-16 decision 2). Messages are for the maintainer, in
/// English like the logs; they are not UI texts.
/// </summary>
internal static class CatalogCommandLine
{
    public const string CommandName = "catalog";

    /// <summary>Folder that holds one subfolder per release; relative paths start at the working directory.</summary>
    public const string ReleasesPathKey = "Catalog:ReleasesPath";

    public const int Success = 0;
    public const int Failure = 1;
    public const int UsageError = 2;

    private const string Usage = """
        Usage: catalog import <version>
          Validates catalog/releases/<version>/ and stores it as a published, inactive release.
        """;

    public static async Task<int> RunAsync(
        IServiceProvider services,
        IReadOnlyList<string> args,
        TextWriter output,
        TextWriter error,
        CancellationToken cancellationToken)
    {
        if (args is not [CommandName, "import", var version] || string.IsNullOrWhiteSpace(version))
        {
            await error.WriteLineAsync(Usage);
            return UsageError;
        }

        var releasesPath = services.GetRequiredService<IConfiguration>()[ReleasesPathKey];
        if (string.IsNullOrWhiteSpace(releasesPath))
        {
            await error.WriteLineAsync($"The setting {ReleasesPathKey} is not configured.");
            return Failure;
        }

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count > 0)
        {
            // The command never migrates: the API applies migrations when it starts (CLAUDE §5).
            await error.WriteLineAsync(
                $"The catalog schema has {pending.Count} pending migration(s) ({string.Join(", ", pending)}). " +
                "Start the API once so it applies them, then import again.");
            return Failure;
        }

        var importer = ActivatorUtilities.CreateInstance<CatalogReleaseImporter>(scope.ServiceProvider);
        var result = await importer.ImportAsync(Path.GetFullPath(releasesPath), version, cancellationToken);
        if (!result.Succeeded)
        {
            await error.WriteLineAsync($"Release {version} was not imported; the database is unchanged. {result.Errors.Count} error(s):");
            foreach (var e in result.Errors)
            {
                await error.WriteLineAsync($"  {e}");
            }

            return Failure;
        }

        var s = result.Summary!;
        await output.WriteLineAsync(
            $"Imported release {s.Version} (inactive): {s.Criteria} criteria, {s.NewCriteria} new; " +
            $"{s.NewRevisions} new revision(s), {s.SharedRevisions} shared with {s.PreviousVersion ?? "no previous release"}.");
        return Success;
    }
}
