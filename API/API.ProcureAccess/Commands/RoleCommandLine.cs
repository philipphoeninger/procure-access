// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Commands;

/// <summary>
/// The <c>roles</c> commands of the API host: grant, revoke and list the platform roles of
/// <see cref="Roles.PlatformRoles"/> (SPEC R-ADM-03: no UI; S4-25). Users are found by e-mail
/// address. Messages are for the maintainer, in English like the logs; they are not UI texts.
/// </summary>
/// <remarks>
/// It works on the legacy Identity tables through <see cref="UserManager{TUser}"/>, so it lives in
/// the host until the users move into the IdentityAccess module (roadmap M3.1). A change reaches a
/// signed-in user with the next sign-in (the cookie principal is built at sign-in). Every grant and revoke is logged
/// with the user id and the role, never with the e-mail address.
/// </remarks>
public static class RoleCommandLine
{
    public const string CommandName = "roles";

    public const int Success = 0;
    public const int Failure = 1;
    public const int UsageError = 2;

    private const string List = "list";
    private const string Grant = "grant";
    private const string Revoke = "revoke";

    private static readonly string Usage = $"""
        Usage: roles list [<email>]
                 Lists every user with a platform role, or the platform roles of one user.
               roles grant <email> <role>
                 Gives the user the platform role.
               roles revoke <email> <role>
                 Takes the platform role from the user.
        Platform roles: {string.Join(", ", Roles.PlatformRoles)}.
        """;

    /// <summary>Whether <paramref name="args"/> start a <c>roles</c> command instead of the API.</summary>
    public static bool IsCommand(IReadOnlyList<string> args) => args is [CommandName, ..];

    /// <summary>
    /// Runs the command in <paramref name="args"/> on <paramref name="services"/>, which must hold
    /// the legacy context, ASP.NET Core Identity and logging. Returns the exit code: 0 done (also
    /// when nothing had to change), 1 refused (nothing written), 2 wrong usage.
    /// </summary>
    public static async Task<int> RunAsync(
        IServiceProvider services,
        IReadOnlyList<string> args,
        TextWriter output,
        TextWriter error,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        string? role = null;
        switch (args)
        {
            case [CommandName, List]:
            case [CommandName, List, _]:
                break;
            case [CommandName, Grant or Revoke, _, var name]:
                role = Roles.PlatformRoles.FirstOrDefault(r => string.Equals(r, name, StringComparison.OrdinalIgnoreCase));
                if (role is null)
                {
                    await error.WriteLineAsync($"'{name}' is not a platform role.");
                    await error.WriteLineAsync(Usage);
                    return UsageError;
                }

                break;
            default:
                await error.WriteLineAsync(Usage);
                return UsageError;
        }

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var pending = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count > 0)
        {
            // The command never migrates: the API applies migrations when it starts (CLAUDE §5),
            // and the platform roles come from a migration (RenamePlatformRoles).
            await error.WriteLineAsync(
                $"The database has {pending.Count} pending migration(s) ({string.Join(", ", pending)}). " +
                "Start the API once so it applies them, then run the command again.");
            return Failure;
        }

        if (args is [_, List])
        {
            return await ListAllAsync(db, output, cancellationToken);
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var email = args[2];
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            await error.WriteLineAsync($"There is no user with the e-mail address {email}.");
            return Failure;
        }

        if (role is null)
        {
            var held = (await userManager.GetRolesAsync(user)).Where(Roles.PlatformRoles.Contains).Order(StringComparer.Ordinal).ToList();
            await output.WriteLineAsync(held.Count == 0
                ? $"{user.Email} has no platform role."
                : $"{user.Email}: {string.Join(", ", held)}");
            return Success;
        }

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(RoleCommandLine));
        return args[1] == Grant
            ? await GrantAsync(userManager, user, role, output, error, logger)
            : await RevokeAsync(userManager, user, role, output, error, logger);
    }

    private static async Task<int> ListAllAsync(ApplicationDBContext db, TextWriter output, CancellationToken cancellationToken)
    {
        var rows = await (
            from userRole in db.UserRoles
            join role in db.Roles on userRole.RoleId equals role.Id
            join user in db.Users on userRole.UserId equals user.Id
            where Roles.PlatformRoles.Contains(role.Name!)
            select new { user.Email, role.Name }).ToListAsync(cancellationToken);
        if (rows.Count == 0)
        {
            await output.WriteLineAsync("No user has a platform role.");
            return Success;
        }

        foreach (var user in rows.GroupBy(r => r.Email).OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
        {
            await output.WriteLineAsync($"{user.Key}: {string.Join(", ", user.Select(r => r.Name).Order(StringComparer.Ordinal))}");
        }

        return Success;
    }

    private static async Task<int> GrantAsync(
        UserManager<User> userManager, User user, string role, TextWriter output, TextWriter error, ILogger logger)
    {
        if (await userManager.IsInRoleAsync(user, role))
        {
            await output.WriteLineAsync($"{user.Email} already has the role {role}; nothing changed.");
            return Success;
        }

        var result = await userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            return await RefusedAsync(error, $"The role {role} was not granted", result);
        }

        logger.LogInformation("Granted the platform role {Role} to user {UserId}", role, user.Id);
        await output.WriteLineAsync($"Granted the role {role} to {user.Email}. It applies from the user's next sign-in or token refresh (at most 30 minutes).");
        return Success;
    }

    private static async Task<int> RevokeAsync(
        UserManager<User> userManager, User user, string role, TextWriter output, TextWriter error, ILogger logger)
    {
        if (!await userManager.IsInRoleAsync(user, role))
        {
            await output.WriteLineAsync($"{user.Email} does not have the role {role}; nothing changed.");
            return Success;
        }

        var result = await userManager.RemoveFromRoleAsync(user, role);
        if (!result.Succeeded)
        {
            return await RefusedAsync(error, $"The role {role} was not revoked", result);
        }

        logger.LogInformation("Revoked the platform role {Role} from user {UserId}", role, user.Id);
        await output.WriteLineAsync($"Revoked the role {role} from {user.Email}. A token issued before keeps it until it expires (at most 30 minutes).");
        return Success;
    }

    private static async Task<int> RefusedAsync(TextWriter error, string message, IdentityResult result)
    {
        await error.WriteLineAsync($"{message}: {string.Join(" ", result.Errors.Select(e => e.Description))}");
        return Failure;
    }
}
