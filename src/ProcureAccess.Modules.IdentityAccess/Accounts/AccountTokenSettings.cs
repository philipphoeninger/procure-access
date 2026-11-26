// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.Platform.Contracts.Configuration;

namespace ProcureAccess.Modules.IdentityAccess.Accounts;

/// <summary>
/// The 🔧 lifetimes of the account tokens (ARCHITECTURE §9.1, C72; SPEC R-AUTH-02), read from the
/// section <see cref="SectionName"/>: <c>EmailConfirmationLifetime</c> 24 h, <c>PasswordResetLifetime</c> 2 h (S5-13a). Never hard-code them
/// (CLAUDE §6.6); a missing or unusable value stops the start. The same section holds
/// <c>CleanupInterval</c>, how often expired tokens are deleted (1 h, S6-05) — an operations
/// setting, not a 🔧 value.
/// </summary>
internal sealed class AccountTokenSettings
{
    public const string SectionName = "Account:Tokens";

    public TimeSpan EmailConfirmationLifetime { get; init; }

    public TimeSpan PasswordResetLifetime { get; init; }

    /// <summary>How often <see cref="AccountTokenCleanupService"/> runs.</summary>
    public TimeSpan CleanupInterval { get; init; }

    public TimeSpan LifetimeOf(AccountTokenPurpose purpose) => purpose switch
    {
        AccountTokenPurpose.EmailConfirmation => EmailConfirmationLifetime,
        AccountTokenPurpose.PasswordReset => PasswordResetLifetime,
        _ => throw new ArgumentOutOfRangeException(nameof(purpose), purpose, "Unknown token purpose."),
    };

    public static AccountTokenSettings From(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        return new AccountTokenSettings
        {
            EmailConfirmationLifetime = Read(section, nameof(EmailConfirmationLifetime)),
            PasswordResetLifetime = Read(section, nameof(PasswordResetLifetime)),
            CleanupInterval = Read(section, nameof(CleanupInterval)),
        };
    }

    private static TimeSpan Read(IConfigurationSection section, string name) =>
        DurationSetting.Read(section[name], $"{SectionName}:{name}");
}
