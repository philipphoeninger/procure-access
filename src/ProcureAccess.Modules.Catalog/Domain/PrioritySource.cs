// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// Where a <see cref="PriorityRecommendation"/> comes from (ARCHITECTURE §5.1). The only value of
/// v1; the UI shows it as „Empfehlung des Projektteams“ (CAT-02).
/// </summary>
internal enum PrioritySource
{
    ExpertJudgement = 1,
}
