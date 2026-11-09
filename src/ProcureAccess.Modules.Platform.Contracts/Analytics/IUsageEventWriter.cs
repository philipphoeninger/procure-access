// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.Analytics;

/// <summary>
/// Records the usage events of SPEC §9.1 for <c>analytics."UsageEvents"</c> (R-MET-01). One method
/// per event, so only the events and dimension values of the specification can be recorded.
/// </summary>
/// <remarks>
/// Fire-and-forget (R-MET-01, #115): a method only queues the event and returns at once; a
/// background service of the Platform module stores it later, in a database context of its own.
/// The methods never throw and never wait for the database. An event that cannot be stored is
/// logged and lost, never counted twice.
/// </remarks>
public interface IUsageEventWriter
{
    /// <summary>S11 was shown for a complete selection (once per selection, not on re-render).</summary>
    /// <param name="signedIn">Whether the user is signed in (dimension 1).</param>
    void CriteriaListShown(bool signedIn);

    /// <summary>An export file was generated successfully.</summary>
    /// <param name="format">File format (dimension 1).</param>
    /// <param name="signedIn">Whether the user is signed in (dimension 2).</param>
    void ExportCreated(UsageExportFormat format, bool signedIn);
}

/// <summary>Export formats counted by <see cref="IUsageEventWriter.ExportCreated"/> (SPEC §9.1).</summary>
public enum UsageExportFormat
{
    Xlsx,
    Csv,
}
