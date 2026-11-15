// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace ProcureAccess.Modules.IdentityAccess.Organizations;

/// <summary>
/// The settings of the invitations (S5-17a), read from the section <see cref="SectionName"/>:
/// <c>Lifetime</c> 🔧 14 days (SPEC R-ORG-02, SD23; ARCHITECTURE C72) and <c>CleanupInterval</c>, how
/// often expired rows are deleted (1 h). Never hard-code them (CLAUDE §6.6); a missing or unusable
/// value stops the start.
/// </summary>
internal sealed class InvitationSettings
{
    public const string SectionName = "Organizations:Invitations";

    public TimeSpan Lifetime { get; init; }

    public TimeSpan CleanupInterval { get; init; }

    public static InvitationSettings From(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        return new InvitationSettings
        {
            Lifetime = Read(section, nameof(Lifetime)),
            CleanupInterval = Read(section, nameof(CleanupInterval)),
        };
    }

    private static TimeSpan Read(IConfigurationSection section, string name)
    {
        var value = section[name];
        if (!TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var duration) || duration <= TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                $"{SectionName}:{name} must be a positive duration such as 14.00:00:00; it is '{value}'.");
        }

        return duration;
    }
}
