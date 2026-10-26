// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Extensions;

/// <summary>
/// The <c>RebuildDatabase</c> switch deletes every user and reseeds the sample data. It runs only
/// in the <c>Development</c> environment, whatever the configuration says (ARCHITECTURE §9.7,
/// invariant SEC-06): a misconfigured production deployment must not wipe data.
/// </summary>
public static class DatabaseRebuildExtensions
{
    public const string RebuildDatabaseKey = "RebuildDatabase";

    /// <summary>
    /// Password of the sample user (environment variable <c>SampleData__UserPassword</c>,
    /// <c>SAMPLE_USER_PASSWORD</c> in <c>.env</c>). Without it the sample user is not created.
    /// </summary>
    public const string SampleUserPasswordKey = "SampleData:UserPassword";

    /// <summary>True only in <c>Development</c> with <c>RebuildDatabase</c> set to <c>true</c>.</summary>
    public static bool IsDatabaseRebuildAllowed(IHostEnvironment environment, IConfiguration configuration) =>
        environment.IsDevelopment() && configuration.GetValue<bool>(RebuildDatabaseKey);

    public static async Task RebuildDatabaseIfAllowedAsync(this WebApplication app)
    {
        if (!IsDatabaseRebuildAllowed(app.Environment, app.Configuration))
        {
            if (app.Configuration.GetValue<bool>(RebuildDatabaseKey))
            {
                app.Logger.LogWarning(
                    "RebuildDatabase is set, but the environment is {EnvironmentName}: the database is left unchanged",
                    app.Environment.EnvironmentName);
            }

            return;
        }

        app.Logger.LogWarning("RebuildDatabase is set in Development: deleting all users and reseeding sample data");

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        await SampleDataInitializer.ClearDatabase(dbContext, userManager);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await app.AddRolesAndPermissions(roleManager);

        var seeded = await SampleDataInitializer.InitializeData(
            dbContext, userManager, app.Configuration[SampleUserPasswordKey]);
        if (!seeded)
        {
            app.Logger.LogWarning(
                "{SettingKey} is not set: no sample user was created", SampleUserPasswordKey);
        }
    }
}
