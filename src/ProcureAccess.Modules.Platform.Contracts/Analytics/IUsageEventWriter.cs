// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.Analytics;

/// <summary>
/// Writes the usage events of SPEC §9.1 into <c>analytics."UsageEvents"</c> (R-MET-01). One method
/// per event, so only the events and dimension values of the specification can be written.
/// </summary>
/// <remarks>
/// The methods never throw: a failed write is logged as an error and the caller's request goes on
/// (R-MET-01). The event is stored in a database context of its own, so it neither saves nor
/// discards changes the caller has not saved yet.
/// </remarks>
public interface IUsageEventWriter
{
    /// <summary>S11 was shown for a complete selection (once per selection, not on re-render).</summary>
    /// <param name="signedIn">Whether the user is signed in (dimension 1).</param>
    Task CriteriaListShownAsync(bool signedIn);

    /// <summary>An export file was generated successfully.</summary>
    /// <param name="format">File format (dimension 1).</param>
    /// <param name="signedIn">Whether the user is signed in (dimension 2).</param>
    Task ExportCreatedAsync(UsageExportFormat format, bool signedIn);
}

/// <summary>Export formats counted by <see cref="IUsageEventWriter.ExportCreatedAsync"/> (SPEC §9.1).</summary>
public enum UsageExportFormat
{
    Xlsx,
    Csv,
}
