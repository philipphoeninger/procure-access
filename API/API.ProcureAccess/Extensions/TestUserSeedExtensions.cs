// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Extensions;

/// <summary>
/// User part of the E2E seeder (TESTING §6.3; S5-14): creates the E2E users with known test
/// credentials, so E2E tests sign in for real instead of faking the session. It runs only in
/// <c>Development</c> and <c>E2E</c>, whatever the configuration says, and only when
/// <see cref="PasswordKey"/> is set, which only <c>docker-compose.e2e.yml</c> does (owner decision
/// S5-14 1 (a)): the dev stack, <c>dotnet run</c>, Staging and Production seed nothing. The addresses
/// are reserved (<c>example.invalid</c>), the password is test data in the repository, never a secret
/// (owner decision S5-14 3 (a)). A user that exists already is left as it is, so a second start
/// changes nothing; the E2E stack's <c>RebuildDatabase</c> deletes them on every start anyway.
/// The users move into IdentityAccess with the users (M3.1).
/// </summary>
public static class TestUserSeedExtensions
{
    /// <summary>Password of every E2E user; the switch of the seeder (compose <c>TestUsers__Password</c>).</summary>
    public const string PasswordKey = "TestUsers:Password";

    /// <summary>E2E user with a confirmed address (sign-in, sign-out, gated actions).</summary>
    public const string ConfirmedEmail = "e2e-confirmed@example.invalid";

    /// <summary>E2E user whose address is not confirmed (the confirmation gate, SEC-10 E2E part with S5-16).</summary>
    public const string UnconfirmedEmail = "e2e-unconfirmed@example.invalid";

    /// <summary>The E2E users and whether their address is confirmed (owner decision S5-14 2 (a)).</summary>
    public static IReadOnlyList<(string Email, bool EmailConfirmed)> Users { get; } =
    [
        (ConfirmedEmail, true),
        (UnconfirmedEmail, false),
    ];

    /// <summary>True only in <c>Development</c> and <c>E2E</c>, like the test catalog release.</summary>
    public static bool IsTestUserSeedAllowed(IHostEnvironment environment) =>
        TestCatalogSeedExtensions.IsTestCatalogSeedAllowed(environment);

    public static async Task SeedTestUsersIfAllowedAsync(this WebApplication app)
    {
        var password = app.Configuration[PasswordKey];
        if (string.IsNullOrEmpty(password))
        {
            return;
        }

        if (!IsTestUserSeedAllowed(app.Environment))
        {
            app.Logger.LogWarning(
                "E2E test users are configured, but the environment is {EnvironmentName}: no test user is seeded",
                app.Environment.EnvironmentName);
            return;
        }

        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        foreach (var (email, emailConfirmed) in Users)
        {
            if (await userManager.FindByEmailAsync(email) is not null)
            {
                continue;
            }

            var created = await userManager.CreateAsync(
                new User { UserName = email, Email = email, EmailConfirmed = emailConfirmed }, password);
            if (!created.Succeeded)
            {
                // Error codes only: the descriptions may repeat the password rules, never the password.
                throw new InvalidOperationException(
                    "Creating an E2E test user failed: " +
                    string.Join(", ", created.Errors.Select(e => e.Code)) +
                    ". The test user password must meet the password rule.");
            }
        }

        app.Logger.LogInformation("E2E test users are seeded");
    }
}
