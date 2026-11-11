// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Extensions;

/// <summary>
/// Catalog part of the E2E seeder (TESTING §6.3; S4-08, #241): imports and activates a test release
/// folder, so a development or E2E stack has an active catalog release before a real one can be
/// activated (O-1). It runs only in <c>Development</c> and <c>E2E</c> (owner decision O-S4-3 (b)),
/// whatever the configuration says, and only when <see cref="PathKey"/> and <see cref="VersionKey"/>
/// are set. The test folders are not part of the API image: the E2E and dev compose overrides mount
/// them, so Staging and Production have neither the environment nor the folder.
/// </summary>
public static class TestCatalogSeedExtensions
{
    /// <summary>Folder that holds the test release folders; a relative path starts at the working directory.</summary>
    public const string PathKey = "Catalog:TestRelease:Path";

    /// <summary>Version (folder name) of the test release, e.g. <c>T.1</c>.</summary>
    public const string VersionKey = "Catalog:TestRelease:Version";

    /// <summary>Environment of the E2E stack once it has its own (TESTING §3.3); until then it runs as Development.</summary>
    public const string E2EEnvironmentName = "E2E";

    /// <summary>True only in <c>Development</c> and <c>E2E</c>.</summary>
    public static bool IsTestCatalogSeedAllowed(IHostEnvironment environment) =>
        environment.IsDevelopment() || environment.IsEnvironment(E2EEnvironmentName);

    public static async Task SeedTestCatalogIfAllowedAsync(this WebApplication app)
    {
        var path = app.Configuration[PathKey];
        var version = app.Configuration[VersionKey];
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(version))
        {
            return;
        }

        if (!IsTestCatalogSeedAllowed(app.Environment))
        {
            app.Logger.LogWarning(
                "A test catalog release is configured, but the environment is {EnvironmentName}: no test release is seeded",
                app.Environment.EnvironmentName);
            return;
        }

        await CatalogModule.SeedTestReleaseAsync(app.Services, Path.GetFullPath(path), version);
    }
}
