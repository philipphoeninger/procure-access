// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Domain;

namespace ProcureAccess.Modules.IdentityAccess.Accounts;

/// <summary>
/// The users the host seeds outside production (S6-10; formerly the legacy
/// <c>SampleDataInitializer</c> and the user part of the host's <c>TestUserSeedExtensions</c>): the
/// Development rebuild with its sample user (SEC-06) and the E2E users (TESTING §6.3; S5-14). The
/// host decides whether either may run (<c>Development</c>, <c>E2E</c>, the settings); this class
/// checks nothing about the environment.
/// </summary>
internal sealed class SeedUsers(UserManager<User> users)
{
    /// <summary>The sample user of the Development rebuild; its address is confirmed.</summary>
    public const string SampleUserEmail = "sample.user@example.org";

    /// <summary>
    /// Deletes every account (a soft-deleted row stays, as before S6-10) and creates the sample user with
    /// <paramref name="sampleUserPassword"/>. Without a password no user is created: a fixed
    /// password or hash would be public in the repository. Returns true when it created the user.
    /// </summary>
    public async Task<bool> RebuildAsync(string? sampleUserPassword, CancellationToken cancellationToken)
    {
        foreach (var user in await users.Users.ToListAsync(cancellationToken))
        {
            await users.DeleteAsync(user);
        }

        if (string.IsNullOrEmpty(sampleUserPassword))
        {
            return false;
        }

        await CreateAsync(SampleUserEmail, emailConfirmed: true, sampleUserPassword, "the sample user");
        return true;
    }

    /// <summary>
    /// Creates the users of <paramref name="accounts"/> that do not exist yet, each with
    /// <paramref name="password"/>; an existing one is left as it is. Returns the id of every
    /// address.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, string>> EnsureAsync(
        IReadOnlyList<(string Email, bool EmailConfirmed)> accounts, string password)
    {
        var ids = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (email, emailConfirmed) in accounts)
        {
            var user = await users.FindByEmailAsync(email)
                       ?? await CreateAsync(email, emailConfirmed, password, "an E2E test user");
            ids[email] = user.Id;
        }

        return ids;
    }

    private async Task<User> CreateAsync(string email, bool emailConfirmed, string password, string what)
    {
        var user = new User { UserName = email, Email = email, EmailConfirmed = emailConfirmed };
        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            // Error codes only: the descriptions may repeat the password rules, never the password.
            throw new InvalidOperationException(
                $"Creating {what} failed: " +
                string.Join(", ", created.Errors.Select(e => e.Code)) +
                ". Its password must meet the password rule.");
        }

        return user;
    }
}
