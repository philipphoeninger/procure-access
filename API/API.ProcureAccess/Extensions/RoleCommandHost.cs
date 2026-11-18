// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Extensions;

/// <summary>
/// Runs a <c>roles</c> command (SPEC R-ADM-03, S4-25) with the host's configuration instead of
/// starting the API, like <see cref="CatalogCommandHost"/>: same image, settings and secrets, but
/// only Platform (logging) and IdentityAccess (the users and roles with ASP.NET Core Identity, and
/// the Data Protection keys its token providers need). No web server, no migration, no background job.
/// </summary>
internal static class RoleCommandHost
{
    public static async Task<int> RunAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddPlatformModule(builder.Configuration)
                        .AddIdentityAccessModule(builder.Configuration, builder.Environment);

        await using var app = builder.Build();
        try
        {
            return await IdentityAccessModule.RunRolesCommandAsync(app.Services, args, Console.Out, Console.Error);
        }
        catch (Exception ex)
        {
            app.Logger.LogCritical(ex, "The roles command {Command} failed.", string.Join(' ', args.Take(2)));
            await Console.Error.WriteLineAsync($"The roles command failed: {ex.Message}");
            return IdentityAccessModule.RolesCommandFailure;
        }
    }
}
