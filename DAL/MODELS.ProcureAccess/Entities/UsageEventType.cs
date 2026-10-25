// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities;

/// <summary>
/// The usage events of v1 (SPEC §9.1). The list is final (ARCHITECTURE §5.7, C82): adding an
/// event needs a SPEC change first. Stored as text, so the indicator queries (R-MET-02) stay
/// readable.
/// </summary>
public enum UsageEventType
{
    /// <summary>S11 shown for a complete selection. Dimension 1: <c>anonymous</c> / <c>signedIn</c>.</summary>
    CriteriaListShown,

    /// <summary>Export file generated. Dimension 1: <c>xlsx</c> / <c>csv</c>; dimension 2: <c>anonymous</c> / <c>signedIn</c>.</summary>
    ExportCreated,
}
