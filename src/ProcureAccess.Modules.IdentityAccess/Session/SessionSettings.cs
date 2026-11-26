// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.Platform.Contracts.Configuration;

namespace ProcureAccess.Modules.IdentityAccess.Session;

/// <summary>
/// The lifetimes of the cookie session (ARCHITECTURE §9.1, C52, C60; SPEC R-AUTH-05). 🔧 Confirmed
/// defaults, read from the section <see cref="SectionName"/> of the configuration: 30 minutes
/// idle, 8 hours absolute from sign-in, a warning 5 minutes before the earlier of the two, and
/// the security stamp checked on every request. Never hard-code them (CLAUDE §6.6).
/// </summary>
internal sealed class SessionSettings
{
    public const string SectionName = "Session";

    /// <summary>The idle limit: a session ends this long after its last request.</summary>
    public TimeSpan IdleTimeout { get; init; }

    /// <summary>The absolute limit: a session ends this long after sign-in, whatever happens.</summary>
    public TimeSpan AbsoluteTimeout { get; init; }

    /// <summary>
    /// How long before the earlier limit the shell warns (SPEC R-AUTH-06). The server only serves
    /// it with the session probe, so the client keeps no copy (owner decision S5-29 2a).
    /// </summary>
    public TimeSpan WarningLead { get; init; }

    /// <summary>
    /// How old a session's last check of the security stamp may be before the next request checks
    /// it again. Zero checks on every request, so a password reset or an account deletion ends the
    /// user's other sessions with their next request (SEC-08; owner decision S5-29 4a).
    /// </summary>
    public TimeSpan SecurityStampValidationInterval { get; init; }

    /// <summary>
    /// Reads and checks the section. A missing or unusable value stops the start, like the other
    /// 🔧 settings of the API (rate limits, problem-type base URI).
    /// </summary>
    public static SessionSettings From(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        var idle = Read(section, nameof(IdleTimeout));
        var absolute = Read(section, nameof(AbsoluteTimeout));
        var warningLead = Read(section, nameof(WarningLead));
        var stampInterval = Read(section, nameof(SecurityStampValidationInterval), zeroAllowed: true);
        if (absolute < idle)
        {
            throw new InvalidOperationException(
                $"{SectionName}:{nameof(AbsoluteTimeout)} must not be shorter than {SectionName}:{nameof(IdleTimeout)}.");
        }

        if (warningLead >= idle)
        {
            throw new InvalidOperationException(
                $"{SectionName}:{nameof(WarningLead)} must be shorter than {SectionName}:{nameof(IdleTimeout)}.");
        }

        return new SessionSettings
        {
            IdleTimeout = idle,
            AbsoluteTimeout = absolute,
            WarningLead = warningLead,
            SecurityStampValidationInterval = stampInterval,
        };
    }

    private static TimeSpan Read(IConfigurationSection section, string name, bool zeroAllowed = false) =>
        DurationSetting.Read(section[name], $"{SectionName}:{name}", zeroAllowed);
}
