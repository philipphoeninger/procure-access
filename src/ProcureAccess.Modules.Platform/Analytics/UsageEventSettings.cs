// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Analytics;

/// <summary>
/// Configuration section <c>UsageEvents</c> (appsettings.json, overridable through environment
/// variables such as <c>UsageEvents__QueueCapacity</c>). Defaults set by the owner in S3-23 (D3).
/// </summary>
internal sealed class UsageEventSettings
{
    public const string SectionName = "UsageEvents";

    /// <summary>
    /// Most events kept in memory before they are stored. While the database is unreachable the
    /// queue fills up; beyond it new events are dropped (and logged), so memory stays bounded.
    /// </summary>
    public int QueueCapacity { get; set; }

    /// <summary>Wait before the next attempt when the database could not be reached.</summary>
    public TimeSpan RetryDelay { get; set; }
}
