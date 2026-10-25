// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities;

/// <summary>
/// One usage event in the <c>analytics</c> schema (ARCHITECTURE §5.7, SPEC R-MET-01). The table
/// counts how often something happened per day and nothing else.
/// </summary>
/// <remarks>
/// <b>Privacy (invariant PRV-05):</b> never add a user, organisation, IP address, session or any
/// other identifying field, and keep <see cref="OccurredOn"/> a date, not a timestamp. The
/// architecture test <c>UsageEventPrivacyTests</c> fails on any column beyond the ones below.
/// Rows are written only through <c>IUsageEventWriter</c>.
/// </remarks>
public class UsageEvent
{
    public long Id { get; set; }

    public UsageEventType EventType { get; set; }

    /// <summary>Day of the event (UTC). Deliberately no time of day.</summary>
    public DateOnly OccurredOn { get; set; }

    /// <summary>First dimension of the event, one of the fixed values of SPEC §9.1.</summary>
    public string? Dimension1 { get; set; }

    /// <summary>Second dimension of the event, one of the fixed values of SPEC §9.1.</summary>
    public string? Dimension2 { get; set; }
}
