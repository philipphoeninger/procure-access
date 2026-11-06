// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Contracts;

/// <summary>
/// Recommended priority of a criterion (ARCHITECTURE §5.1 <c>PriorityRecommendation.Level</c>).
/// The labels in the UI are the provisional terms of SPEC SD6 (O-14). In <c>Contracts</c> because
/// Configurator and Export need it too (S3-11 §2.2). The numbers order the levels from strict to
/// loose, as priority resolution compares them (A &gt; B &gt; Info, CAT-14).
/// </summary>
public enum PriorityLevel
{
    /// <summary>Mindestanforderung.</summary>
    A = 1,

    /// <summary>Bewertungskriterium.</summary>
    B = 2,

    /// <summary>Hinweis.</summary>
    Info = 3,
}
