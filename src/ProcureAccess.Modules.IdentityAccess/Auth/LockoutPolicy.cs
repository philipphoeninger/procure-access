// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace ProcureAccess.Modules.IdentityAccess.Auth;

/// <summary>
/// The sign-in lockout of SPEC R-AUTH-03 and ARCHITECTURE §9.1 (C72; S5-13a): after
/// 🔧 <c>Account:Lockout:MaxFailedAttempts</c> (5) failed sign-ins the account is locked for
/// 🔧 <c>Account:Lockout:Duration</c> (15 minutes). Set as Identity's lockout options; the lock runs
/// on the API's <see cref="TimeProvider"/> through <see cref="ClockedUserManager"/>.
/// </summary>
internal sealed record LockoutPolicy(int MaxFailedAttempts, TimeSpan Duration)
{
    public const string MaxFailedAttemptsSetting = "Account:Lockout:MaxFailedAttempts";
    public const string DurationSetting = "Account:Lockout:Duration";

    /// <summary>Reads both 🔧 values; a missing or impossible value stops the start.</summary>
    public static LockoutPolicy From(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var attempts = configuration.GetValue<int?>(MaxFailedAttemptsSetting);
        if (attempts is null or < 1)
        {
            throw new InvalidOperationException(
                $"{MaxFailedAttemptsSetting} must be a whole number of at least 1 (SPEC R-AUTH-03: 5).");
        }

        var durationValue = configuration[DurationSetting];
        if (!TimeSpan.TryParse(durationValue, CultureInfo.InvariantCulture, out var duration) || duration <= TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                $"{DurationSetting} must be a positive duration such as 00:15:00 (SPEC R-AUTH-03); it is '{durationValue}'.");
        }

        return new LockoutPolicy(attempts.Value, duration);
    }
}
