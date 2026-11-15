// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;

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
/// Since S5-16b (owner decision S5-16 7a) it also creates three organisation users and the
/// organisation <see cref="OrganizationName"/> with them as Owner, Editor and Reader, through the
/// IdentityAccess module; E2E tests only read it, a test that changes an organisation builds its own.
/// </summary>
public static class TestUserSeedExtensions
{
    /// <summary>Password of every E2E user; the switch of the seeder (compose <c>TestUsers__Password</c>).</summary>
    public const string PasswordKey = "TestUsers:Password";

    /// <summary>E2E user with a confirmed address (sign-in, sign-out, gated actions).</summary>
    public const string ConfirmedEmail = "e2e-confirmed@example.invalid";

    /// <summary>E2E user whose address is not confirmed (the confirmation gate, SEC-10).</summary>
    public const string UnconfirmedEmail = "e2e-unconfirmed@example.invalid";

    /// <summary>Owner of <see cref="OrganizationName"/> (S5-16b, owner decision S5-16 7a).</summary>
    public const string OrganizationOwnerEmail = "e2e-org-owner@example.invalid";

    /// <summary>Editor of <see cref="OrganizationName"/>.</summary>
    public const string OrganizationEditorEmail = "e2e-org-editor@example.invalid";

    /// <summary>Reader of <see cref="OrganizationName"/>.</summary>
    public const string OrganizationReaderEmail = "e2e-org-reader@example.invalid";

    /// <summary>The E2E organisation; E2E tests only read it (owner decision S5-16 7a).</summary>
    public const string OrganizationName = "E2E-Organisation";

    /// <summary>
    /// The known id of <see cref="OrganizationName"/>, so E2E tests open its members page before an
    /// endpoint lists a user's organisations (S5-18); the same value is in <c>e2e/support/users.ts</c>.
    /// </summary>
    public static readonly Guid OrganizationId = new("0e2e0000-0000-4000-8000-000000000001");

    /// <summary>The E2E users and whether their address is confirmed (owner decision S5-14 2 (a); S5-16 7a).</summary>
    public static IReadOnlyList<(string Email, bool EmailConfirmed)> Users { get; } =
    [
        (ConfirmedEmail, true),
        (UnconfirmedEmail, false),
        (OrganizationOwnerEmail, true),
        (OrganizationEditorEmail, true),
        (OrganizationReaderEmail, true),
    ];

    /// <summary>The members of <see cref="OrganizationName"/> by address.</summary>
    public static IReadOnlyList<(string Email, OrganizationRole Role)> OrganizationMembers { get; } =
    [
        (OrganizationOwnerEmail, OrganizationRole.Owner),
        (OrganizationEditorEmail, OrganizationRole.Editor),
        (OrganizationReaderEmail, OrganizationRole.Reader),
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

        var members = new List<(string UserId, OrganizationRole Role)>();
        foreach (var (email, role) in OrganizationMembers)
        {
            var user = await userManager.FindByEmailAsync(email)
                       ?? throw new InvalidOperationException("An E2E organisation user is missing after seeding.");
            members.Add((user.Id, role));
        }

        await IdentityAccessModule.SeedTestOrganizationAsync(app.Services, OrganizationId, OrganizationName, members);

        app.Logger.LogInformation("E2E test users and the E2E organisation are seeded");
    }
}
