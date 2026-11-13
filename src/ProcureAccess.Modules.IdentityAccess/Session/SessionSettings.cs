// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;

namespace ProcureAccess.Modules.IdentityAccess.Session;

/// <summary>
/// The lifetimes of the cookie session (ARCHITECTURE §9.1, C52, C60; SPEC R-AUTH-05). 🔧 Confirmed
/// defaults, read from the section <see cref="SectionName"/> of the configuration: 30 minutes
/// idle and 8 hours absolute from sign-in. Never hard-code them (CLAUDE §6.6).
/// </summary>
/// <remarks>
/// The warning lead time (<c>Session:WarningLead</c>) belongs to the expiry warning of the shell
/// and arrives with it (S5-29/S5-30, step 4).
/// </remarks>
internal sealed class SessionSettings
{
    public const string SectionName = "Session";

    /// <summary>The idle limit: a session ends this long after its last request.</summary>
    public TimeSpan IdleTimeout { get; init; }

    /// <summary>The absolute limit: a session ends this long after sign-in, whatever happens.</summary>
    public TimeSpan AbsoluteTimeout { get; init; }

    /// <summary>
    /// Reads and checks the section. A missing or unusable value stops the start, like the other
    /// 🔧 settings of the API (rate limits, problem-type base URI).
    /// </summary>
    public static SessionSettings From(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        var idle = Read(section, nameof(IdleTimeout));
        var absolute = Read(section, nameof(AbsoluteTimeout));
        if (absolute < idle)
        {
            throw new InvalidOperationException(
                $"{SectionName}:{nameof(AbsoluteTimeout)} must not be shorter than {SectionName}:{nameof(IdleTimeout)}.");
        }

        return new SessionSettings { IdleTimeout = idle, AbsoluteTimeout = absolute };
    }

    private static TimeSpan Read(IConfigurationSection section, string name)
    {
        var value = section[name];
        if (!TimeSpan.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, out var timeout) || timeout <= TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                $"{SectionName}:{name} must be a positive duration such as 00:30:00 (ARCHITECTURE §9.1); it is '{value}'.");
        }

        return timeout;
    }
}
