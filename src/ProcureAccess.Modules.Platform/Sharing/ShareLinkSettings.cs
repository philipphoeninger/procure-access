// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;

namespace ProcureAccess.Modules.Platform.Sharing;

/// <summary>
/// 🔧 <c>ShareLinks:*</c> (SPEC SD15, SD23; ARCHITECTURE C81): how far ahead an expiry date may lie,
/// and the time zone whose calendar days the dates name (owner decision S5-23 2a). Read once at the
/// start; a missing or impossible value, or a time zone the system does not know, stops it.
/// </summary>
/// <param name="MaxExpiryMonths">The latest expiry date is today plus this many months (SD15: 12, one year).</param>
/// <param name="TimeZone">The zone of „today“ and of the end of the chosen day.</param>
internal sealed record ShareLinkSettings(int MaxExpiryMonths, TimeZoneInfo TimeZone)
{
    public const string SectionName = "ShareLinks";

    public static ShareLinkSettings From(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var section = configuration.GetSection(SectionName);
        var months = section.GetValue<int?>(nameof(MaxExpiryMonths));
        if (months is not > 0)
        {
            throw new InvalidOperationException(
                $"{SectionName}:{nameof(MaxExpiryMonths)} must be greater than 0 (SPEC SD15: 12).");
        }

        var zoneId = section.GetValue<string>(nameof(TimeZone));
        if (string.IsNullOrWhiteSpace(zoneId))
        {
            throw new InvalidOperationException($"{SectionName}:{nameof(TimeZone)} is required (Europe/Berlin).");
        }

        TimeZoneInfo zone;
        try
        {
            zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
        }
        catch (TimeZoneNotFoundException ex)
        {
            throw new InvalidOperationException(
                $"{SectionName}:{nameof(TimeZone)} '{zoneId}' is not a time zone of this system (is tzdata installed?).", ex);
        }

        return new ShareLinkSettings(months.Value, zone);
    }
}
