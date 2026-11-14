// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

namespace ProcureAccess.Modules.IdentityAccess.Accounts;

/// <summary>
/// The 🔧 lifetimes of the account tokens (ARCHITECTURE §9.1, C72; SPEC R-AUTH-02), read from the
/// section <see cref="SectionName"/>: <c>EmailConfirmationLifetime</c> 24 h. Never hard-code them
/// (CLAUDE §6.6); a missing or unusable value stops the start.
/// </summary>
internal sealed class AccountTokenSettings
{
    public const string SectionName = "Account:Tokens";

    public TimeSpan EmailConfirmationLifetime { get; init; }

    public TimeSpan LifetimeOf(AccountTokenPurpose purpose) => purpose switch
    {
        AccountTokenPurpose.EmailConfirmation => EmailConfirmationLifetime,
        _ => throw new ArgumentOutOfRangeException(nameof(purpose), purpose, "Unknown token purpose."),
    };

    public static AccountTokenSettings From(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        return new AccountTokenSettings
        {
            EmailConfirmationLifetime = Read(section, nameof(EmailConfirmationLifetime)),
        };
    }

    private static TimeSpan Read(IConfigurationSection section, string name)
    {
        var value = section[name];
        if (!TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var lifetime) || lifetime <= TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                $"{SectionName}:{name} must be a positive duration such as 1.00:00:00 (ARCHITECTURE §9.1); it is '{value}'.");
        }

        return lifetime;
    }
}
