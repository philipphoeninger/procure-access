// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcureAccess.Modules.Catalog.Persistence;

namespace ProcureAccess.Modules.Catalog.Releases;

/// <summary>
/// Catalog part of the E2E seeder (TESTING §6.3; S4-08, #241): gives a development or E2E stack an
/// active test release (the MiniCatalog folder <c>T.1</c>), so the catalog endpoints answer there
/// before a real release can be activated (O-1). The host decides where it may run (owner decision
/// O-S4-3 (b): Development and E2E, never Staging or Production).
/// </summary>
/// <remarks>
/// Runs on every start and changes only what is missing: it imports the release only if its version
/// is not stored yet, and activates it only if no release is active, so it never replaces a release
/// someone activated by hand. A missing folder is skipped with a warning; a folder that fails the
/// import or an incomplete release throws, so the stack does not start without its test data.
/// </remarks>
internal sealed class TestReleaseSeeder(CatalogDbContext db, TimeProvider timeProvider, ILogger<TestReleaseSeeder> logger)
{
    public async Task SeedAsync(string releasesPath, string version, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(releasesPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        if (!await db.Releases.AnyAsync(r => r.Version == version, cancellationToken))
        {
            if (!Directory.Exists(Path.Combine(releasesPath, version)))
            {
                logger.LogWarning(
                    "Test release {Version} not seeded: its folder is missing in {ReleasesPath}", version, releasesPath);
                return;
            }

            var import = await new CatalogReleaseImporter(db, timeProvider).ImportAsync(releasesPath, version, cancellationToken);
            if (!import.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Test release {version} could not be imported: {string.Join("; ", import.Errors)}");
            }

            logger.LogInformation("Test release {Version} imported", version);
        }

        if (await db.Releases.AnyAsync(r => r.IsActive, cancellationToken))
        {
            return;
        }

        var activation = await new CatalogReleaseActivator(db).ActivateAsync(version, cancellationToken);
        if (!activation.Succeeded)
        {
            throw new InvalidOperationException(
                $"Test release {version} could not be activated: {string.Join("; ", activation.Errors)}");
        }

        logger.LogInformation("Test release {Version} activated", version);
    }
}
