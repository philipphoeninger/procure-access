// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Logging;

/// <summary>
/// One row of the Serilog PostgreSQL sink in the <c>logging</c> schema (ARCHITECTURE §9.5).
/// The sink writes the rows; the application only reads and deletes them (log retention, OPS-02).
/// Table and columns are defined in <see cref="Persistence.Configurations.SeriLogEntryConfiguration"/>,
/// which also holds the names the sink is configured with.
/// </summary>
internal sealed class SeriLogEntry
{
    public long Id { get; set; }

    /// <summary>Time of the log event in UTC.</summary>
    public DateTime TimeStamp { get; set; }

    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public string? Exception { get; set; }

    /// <summary>All structured properties of the event as JSON.</summary>
    public string? Properties { get; set; }

    public string? SourceContext { get; set; }
    public string? ApplicationName { get; set; }
    public string? MachineName { get; set; }
}
