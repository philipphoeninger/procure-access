// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Platform.Contracts.Tenancy;

namespace API.ProcureAccess.Extensions;

/// <summary>
/// Runs a catalog command (<c>catalog import &lt;version&gt;</c>, <c>catalog activate &lt;version&gt;</c>) with the host's configuration
/// instead of starting the API (owner, S3-16 decision 1): same image, same settings and secrets,
/// but only the Platform and Catalog modules. No web server starts, no migration runs and no
/// background job starts.
/// </summary>
internal static class CatalogCommandHost
{
    public static async Task<int> RunAsync(string[] args)
    {
        // The web builder reads appsettings.{environment}.json and the environment variables the
        // same way as the API. The command's own arguments are not configuration.
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddPlatformModule(builder.Configuration)
                        .AddCatalogModule(builder.Configuration);

        // Platform's context binds its share-link filter to the organisation context, which only
        // the IdentityAccess module registers; this host has no request and no organisation (S5-23).
        builder.Services.AddSingleton<IOrganizationContext>(FixedOrganizationContext.None);

        await using var app = builder.Build();
        // The command needs a migrated database, so the log table exists (#437)
        app.StartDatabaseLogSink();
        try
        {
            return await CatalogModule.RunCommandAsync(app.Services, args, Console.Out, Console.Error);
        }
        catch (Exception ex)
        {
            app.Logger.LogCritical(ex, "The catalog command {Command} failed.", string.Join(' ', args));
            await Console.Error.WriteLineAsync($"The catalog command failed: {ex.Message}");
            return 1;
        }
    }
}
