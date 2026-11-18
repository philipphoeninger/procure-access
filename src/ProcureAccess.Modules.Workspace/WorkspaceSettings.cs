// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;

namespace ProcureAccess.Modules.Workspace;

/// <summary>
/// 🔧 <c>Workspace:*</c>: the time zone whose calendar day is „today“ for the date of a test
/// evidence, which may not lie after it (owner decision S6-15 2 (a); Europe/Berlin). Read once at
/// the start; a missing value or a time zone the system does not know stops it.
/// </summary>
internal sealed record WorkspaceSettings(TimeZoneInfo TimeZone)
{
    public const string SectionName = "Workspace";

    public static WorkspaceSettings From(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var zoneId = configuration.GetSection(SectionName).GetValue<string>(nameof(TimeZone));
        if (string.IsNullOrWhiteSpace(zoneId))
        {
            throw new InvalidOperationException($"{SectionName}:{nameof(TimeZone)} is required (Europe/Berlin).");
        }

        try
        {
            return new WorkspaceSettings(TimeZoneInfo.FindSystemTimeZoneById(zoneId));
        }
        catch (TimeZoneNotFoundException ex)
        {
            throw new InvalidOperationException(
                $"{SectionName}:{nameof(TimeZone)} '{zoneId}' is not a time zone of this system (is tzdata installed?).", ex);
        }
    }

    /// <summary>Today in <see cref="TimeZone"/> at <paramref name="now"/>.</summary>
    public DateOnly Today(DateTimeOffset now) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, TimeZone).DateTime);
}
