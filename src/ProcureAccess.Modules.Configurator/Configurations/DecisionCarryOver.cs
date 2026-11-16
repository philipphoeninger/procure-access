// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Domain;

namespace ProcureAccess.Modules.Configurator.Configurations;

/// <summary>
/// How a user's decision on a criterion moves to a new selection of the same list („Auswahl
/// ändern“, R-SAV-03) or to a copy on the active release (R-LST-14, CAT-22): included or not, the
/// chosen level, the weight and the note move as they are; the recommendation is the one the catalog
/// resolves for the new selection.
/// </summary>
/// <remarks>
/// <b>A weight without a chosen level (owner decision S5-21 a).</b> A weight applies only while B
/// does. Who weighted a criterion recommended as B without choosing a level accepted B; when the new
/// recommendation is not B, the decision is kept as „B chosen“ with that weight instead of losing
/// the weight. A chosen level equal to the new recommendation counts as no choice
/// (<see cref="ConfigurationCriterion"/>).
/// </remarks>
internal static class DecisionCarryOver
{
    /// <summary>The row for <paramref name="revisionId"/> under its new recommendation, with <paramref name="previous"/>'s decision if there is one.</summary>
    public static ConfigurationCriterionDraft Carry(int revisionId, PriorityLevel recommendation, ConfigurationCriterion? previous)
    {
        if (previous is null)
        {
            return new ConfigurationCriterionDraft(revisionId, recommendation);
        }

        var level = previous.OverrideLevel;
        if (previous.OverrideWeight is not null && level is null && recommendation != PriorityLevel.B)
        {
            level = PriorityLevel.B;
        }

        return new ConfigurationCriterionDraft(
            revisionId, recommendation, previous.IsIncluded, level, previous.OverrideWeight, previous.NoteDe);
    }
}
