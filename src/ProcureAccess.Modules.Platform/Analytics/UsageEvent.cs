// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Analytics;

/// <summary>
/// One usage event in the <c>analytics</c> schema (ARCHITECTURE §5.7, SPEC R-MET-01). The table
/// counts how often something happened per day and nothing else.
/// </summary>
/// <remarks>
/// <b>Privacy (invariant PRV-05):</b> never add a user, organisation, IP address, session or any
/// other identifying field, and keep <see cref="OccurredOn"/> a date, not a timestamp. The
/// architecture test <c>UsageEventPrivacyTests</c> fails on any column beyond the ones below.
/// Rows are written only through <see cref="UsageEventWriter"/>.
/// </remarks>
internal sealed class UsageEvent
{
    /// <summary>
    /// Random key (UUID version 4) set by the database, so that the key does not reveal the order
    /// of events within a day (owner decision 2026-10-29, #124). Never use a sequential or
    /// time-ordered key (identity, UUIDv7) here.
    /// </summary>
    public Guid Id { get; set; }

    public UsageEventType EventType { get; set; }

    /// <summary>Day of the event (UTC). Deliberately no time of day.</summary>
    public DateOnly OccurredOn { get; set; }

    /// <summary>First dimension of the event, one of the fixed values of SPEC §9.1.</summary>
    public string? Dimension1 { get; set; }

    /// <summary>Second dimension of the event, one of the fixed values of SPEC §9.1.</summary>
    public string? Dimension2 { get; set; }
}
