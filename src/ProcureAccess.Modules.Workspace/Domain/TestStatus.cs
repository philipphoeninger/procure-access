// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Workspace.Domain;

/// <summary>
/// Whether a candidate product was tested for accessibility (ARCHITECTURE §5.4, C46; SPEC R-CAN-05;
/// SIG-03 workspace part): „Ja“, „Nein“, „Nicht bekannt“ (<c>candidate.evidence.tested.*</c>).
/// Stored by name, not null, default <see cref="Unknown"/>. <see cref="Unknown"/> is a working state
/// that never leaves the workspace: publishing refuses it (SIG-10, step 6).
/// </summary>
internal enum TestStatus
{
    /// <summary>„Nicht bekannt“, the status of a new candidate.</summary>
    Unknown,

    /// <summary>„Ja“: the only status under which test evidences exist (WSP-01).</summary>
    Tested,

    /// <summary>„Nein“.</summary>
    NotTested,
}
