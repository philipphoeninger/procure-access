// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Workspace.Domain;

/// <summary>
/// The status of a procurement project (SPEC R-PRJ-05): „Laufend“, „Abgeschlossen“, „Archiviert“
/// (<c>projects.status.open</c> / <c>.done</c> / <c>.archived</c>). Stored by name.
/// </summary>
internal enum ProjectStatus
{
    /// <summary>„Laufend“, the status of a new project.</summary>
    Open,

    /// <summary>„Abgeschlossen“. Still editable.</summary>
    Done,

    /// <summary>„Archiviert“: read-only for everyone until the status changes again.</summary>
    Archived,
}
