// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;

namespace ProcureAccess.Modules.Configurator.Domain;

/// <summary>
/// The user's decision on one criterion of a saved list (ARCHITECTURE §5.3): included or not, the
/// level and weight chosen instead of the recommendation, and a note. A row of its list, stored and
/// read only with it.
/// </summary>
/// <remarks>
/// <see cref="ResolvedLevel"/> is what the tool recommended when the list was saved. It is set once
/// and no decision changes it (CAT-08): the list keeps „recommended“ next to „decided“.
/// </remarks>
internal sealed class ConfigurationCriterion
{
    /// <summary>SPEC §7.2 „Criterion note“, counted in UTF-16 code units like <see cref="string.Length"/>.</summary>
    public const int NoteMaxLength = 1000;

    private ConfigurationCriterion()
    {
    }

    /// <summary>The criterion by its revision in the release the list is pinned to (ADR-002).</summary>
    public int CriterionRevisionId { get; private set; }

    public bool IsIncluded { get; private set; }

    /// <summary>The recommended level at the time of saving; never changed afterwards (CAT-08).</summary>
    public PriorityLevel ResolvedLevel { get; private set; }

    /// <summary>The user's level; <c>null</c> while the recommendation applies.</summary>
    public PriorityLevel? OverrideLevel { get; private set; }

    /// <summary>The user's weight; only while <see cref="EffectiveLevel"/> is B.</summary>
    public int? OverrideWeight { get; private set; }

    public string? NoteDe { get; private set; }

    /// <summary>The level that applies: the user's choice, else the recommendation.</summary>
    public PriorityLevel EffectiveLevel => OverrideLevel ?? ResolvedLevel;

    /// <summary>Creates the row from what the catalog resolved and what the user decided.</summary>
    internal static ConfigurationCriterion Create(ConfigurationCriterionDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        if (!Enum.IsDefined(draft.ResolvedLevel))
        {
            throw new ArgumentOutOfRangeException(nameof(draft), draft.ResolvedLevel, "Unknown recommended level.");
        }

        var criterion = new ConfigurationCriterion
        {
            CriterionRevisionId = draft.CriterionRevisionId,
            ResolvedLevel = draft.ResolvedLevel,
        };
        criterion.Decide(draft.IsIncluded, draft.OverrideLevel, draft.OverrideWeight, draft.Note);
        return criterion;
    }

    /// <summary>
    /// Stores the user's decision. A level equal to the recommendation counts as no choice, an
    /// empty note as none. Throws for an unknown level, a weight below 1 or without B, and a note
    /// longer than <see cref="NoteMaxLength"/>; the configured weight range is the caller's check.
    /// </summary>
    internal void Decide(bool isIncluded, PriorityLevel? overrideLevel, int? overrideWeight, string? note)
    {
        if (overrideLevel is { } level && !Enum.IsDefined(level))
        {
            throw new ArgumentOutOfRangeException(nameof(overrideLevel), overrideLevel, "Unknown level.");
        }

        var chosen = overrideLevel == ResolvedLevel ? null : overrideLevel;
        if (overrideWeight is { } weight)
        {
            if (weight < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(overrideWeight), overrideWeight, "A weight is at least 1.");
            }

            if ((chosen ?? ResolvedLevel) != PriorityLevel.B)
            {
                throw new ArgumentException("A weight applies only while the level is B.", nameof(overrideWeight));
            }
        }

        if (note is { Length: > NoteMaxLength })
        {
            throw new ArgumentException($"A note has at most {NoteMaxLength} characters.", nameof(note));
        }

        IsIncluded = isIncluded;
        OverrideLevel = chosen;
        OverrideWeight = overrideWeight;
        NoteDe = string.IsNullOrEmpty(note) ? null : note;
    }
}

/// <summary>What <see cref="CriteriaConfiguration.Create"/> needs per criterion of the selection.</summary>
/// <param name="ResolvedLevel">The level the catalog recommends for the selection, resolved on the server.</param>
internal sealed record ConfigurationCriterionDraft(
    int CriterionRevisionId,
    PriorityLevel ResolvedLevel,
    bool IsIncluded = true,
    PriorityLevel? OverrideLevel = null,
    int? OverrideWeight = null,
    string? Note = null);
