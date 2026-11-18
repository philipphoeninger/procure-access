// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Workspace.Domain;

/// <summary>
/// What a test evidence names as „Art der Prüfung“ (ARCHITECTURE §5.4, C27; UX copy <c>testType.*</c>).
/// Stored by name.
/// </summary>
internal enum TestKind
{
    /// <summary>One of the catalog's test procedures, named by its key (<see cref="TestEvidence.TestTypeKey"/>).</summary>
    Procedure,

    /// <summary>„Andere Prüfung“: a procedure outside the catalog, with its name (<see cref="TestEvidence.OtherTestTypeName"/>).</summary>
    Other,

    /// <summary>„Eigene Prüfung ohne festes Verfahren“.</summary>
    Own,
}
