// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Contracts;

/// <summary>
/// Kind of a catalog criterion (ARCHITECTURE §5.1, C30). In <c>Contracts</c> because Configurator,
/// Export, Workspace and Directory need it too (S3-11 §2.2).
/// </summary>
public enum CriterionKind
{
    /// <summary>An accessibility requirement that is assessed per product.</summary>
    Requirement = 1,

    /// <summary>
    /// „Geprüft nach …“: one per test type, added by selecting the test type; never assessed, its
    /// answer is derived from the test evidences (CAT-16, CAT-17).
    /// </summary>
    TestProcedure = 2,
}
