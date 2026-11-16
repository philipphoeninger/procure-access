// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Contracts.Drafts;
using ProcureAccess.Modules.Configurator.Domain;

namespace ProcureAccess.Modules.Configurator.Drafts;

/// <inheritdoc cref="IConfigurationDraftCheck"/>
/// <remarks>
/// <para>Moved from the ad-hoc export source in S5-20a (C153) without a change of its rules or
/// messages, so the export answers as before and saving a list answers the same:</para>
/// <list type="bullet">
/// <item><b>Server resolves (EXP-07).</b> The recommended level, weight and rationale come from
/// <see cref="ICatalogSelectionResolver"/>; the request has no member for them.</item>
/// <item><b>Active release only (owner decision O-S4-60 (a)).</b> An unknown or incomplete release
/// is <see cref="DraftCheckOutcome.ReleaseNotFound"/>; a stored release that is no longer active is
/// <see cref="DraftCheckOutcome.ReleaseNotActive"/>.</item>
/// <item><b>Selection (CAT-11, CAT-12).</b> Checked as by the criteria endpoint, keyed below
/// <c>selection</c>; field errors of the request come before an exclusion, so the two problem
/// types never mix. For <see cref="DraftPurpose.Save"/> an inactive use case is a field error
/// (CAT-18).</item>
/// <item><b>Every criterion once (owner decision O-S4-57 (a)).</b> The array names each criterion of
/// the selection exactly once, by its revision in the release; a missing, repeated or foreign one
/// is a field error, never a silently dropped or added row.</item>
/// <item><b>Limits (SPEC §7.2).</b> No more entries than the release has criteria; a note of at most
/// <see cref="NoteMaxLength"/> UTF-16 code units; a weight within <see cref="PriorityWeightRange"/>
/// (O-S4-58 (a)) and only while B applies. Error messages never repeat a note.</item>
/// </list>
/// </remarks>
internal sealed class ConfigurationDraftCheck(ICatalogSelectionResolver catalog, PriorityWeightRange weights)
    : IConfigurationDraftCheck
{
    /// <summary>SPEC §7.2 „Criterion note“, as the client counts it (<c>string.Length</c>); the limit a stored list keeps.</summary>
    public const int NoteMaxLength = ConfigurationCriterion.NoteMaxLength;

    private const string SelectionField = "selection";
    private const string ReleaseField = "selection.releaseVersion";
    private const string UseCaseField = "selection.useCaseTemplateId";
    private const string CriteriaField = "criteria";
    private const string InvalidDetail = "The request is not valid.";

    public async Task<DraftCheckResult> CheckAsync(CriteriaDraft draft, DraftPurpose purpose, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var selection = draft.Selection;
        if (selection is null)
        {
            errors[SelectionField] = ["The selection is required."];
        }
        else if (string.IsNullOrEmpty(selection.ReleaseVersion))
        {
            errors[ReleaseField] = ["The catalog release is required."];
        }

        if (draft.Criteria is null)
        {
            errors[CriteriaField] = ["The criteria are required, one entry per criterion of the selection."];
        }

        var decisions = ReadDecisions(draft.Criteria ?? [], errors);
        if (selection?.ReleaseVersion is not { Length: > 0 } version)
        {
            return Invalid(errors);
        }

        var resolved = await catalog.ResolveAsync(
            version,
            new CatalogSelectionQuery(
                selection.UseCaseTemplateId,
                selection.AppTypeIds ?? [],
                selection.ProductPartIds ?? [],
                selection.TestTypeIds ?? []),
            cancellationToken);

        if (resolved.Outcome == CatalogSelectionOutcome.ReleaseNotFound || resolved.Release is null)
        {
            return Refused(DraftCheckOutcome.ReleaseNotFound, $"Catalog release '{version}' is not stored or not complete.", NoErrors());
        }

        if (!resolved.Release.IsActive)
        {
            return Refused(
                DraftCheckOutcome.ReleaseNotActive,
                $"Catalog release '{version}' is no longer the active release.",
                new Dictionary<string, string[]>(StringComparer.Ordinal)
                {
                    [ReleaseField] = [$"Catalog release '{version}' is no longer the active release; load the criteria again."],
                });
        }

        var selectionErrors = resolved.Errors.ToDictionary(e => SelectionKey(e.Key), e => e.Value, StringComparer.Ordinal);
        if (resolved.Outcome == CatalogSelectionOutcome.ExcludedSelection && errors.Count == 0)
        {
            return Refused(DraftCheckOutcome.SelectionExcluded, "The selection contains an excluded value.", selectionErrors);
        }

        if (resolved.Outcome == CatalogSelectionOutcome.InvalidSelection)
        {
            foreach (var (key, messages) in selectionErrors)
            {
                errors[key] = messages;
            }
        }

        if (resolved.Selection is not { } valid)
        {
            return Invalid(errors);
        }

        if (purpose == DraftPurpose.Save && !valid.UseCaseIsActive)
        {
            errors[UseCaseField] = [$"The use case '{valid.UseCase?.Key}' is no longer offered; choose another one."];
        }

        var entryCount = draft.Criteria?.Count ?? 0;
        if (entryCount > resolved.Release.CriterionCount)
        {
            errors[CriteriaField] = [$"Release '{version}' has {resolved.Release.CriterionCount} criteria; the request lists {entryCount}."];
            return Invalid(errors);
        }

        var criteria = Match(valid.Criteria, decisions, errors);
        return errors.Count > 0
            ? Invalid(errors)
            : new DraftCheckResult(DraftCheckOutcome.Checked, "The draft is valid.", NoErrors(), new CheckedDraft(resolved.Release.Version, valid, criteria));
    }

    /// <summary>Reads each entry and checks what needs no catalog: presence, level names, weight range, note length.</summary>
    private List<Decision> ReadDecisions(IReadOnlyList<DraftCriterion?> entries, Dictionary<string, string[]> errors)
    {
        var decisions = new List<Decision>();
        for (var i = 0; i < entries.Count; i++)
        {
            var field = $"{CriteriaField}[{i}]";
            if (entries[i] is not { } entry)
            {
                errors[field] = ["The entry is empty."];
                continue;
            }

            if (entry.IsIncluded is null)
            {
                errors[$"{field}.isIncluded"] = ["Say whether the criterion is included."];
            }

            PriorityLevel? level = null;
            if (entry.OverrideLevel is not null)
            {
                level = LevelOf(entry.OverrideLevel);
                if (level is null)
                {
                    errors[$"{field}.overrideLevel"] = ["The level must be A, B or Info."];
                }
            }

            if (entry.OverrideWeight is { } weight && !weights.Contains(weight))
            {
                errors[$"{field}.overrideWeight"] = [$"The weight must be between {weights.Min} and {weights.Max}."];
            }

            if (entry.Note is { Length: > NoteMaxLength } note)
            {
                errors[$"{field}.note"] = [$"The note has {note.Length} characters; at most {NoteMaxLength} are allowed."];
            }

            if (entry.CriterionRevisionId is not { } revisionId)
            {
                errors[$"{field}.criterionRevisionId"] = ["The criterion revision is required."];
                continue;
            }

            decisions.Add(new Decision(i, revisionId, entry.IsIncluded ?? false, level, entry.OverrideWeight, entry.Note));
        }

        return decisions;
    }

    /// <summary>
    /// Pairs each criterion of the selection with its decision, in list order; records every
    /// missing, repeated or foreign revision.
    /// </summary>
    private static List<CheckedCriterion> Match(
        IReadOnlyList<ResolvedCatalogCriterion> criteria,
        List<Decision> decisions,
        Dictionary<string, string[]> errors)
    {
        var known = criteria.ToDictionary(c => c.RevisionId);
        var byRevision = new Dictionary<int, Decision>();
        foreach (var decision in decisions)
        {
            var field = $"{CriteriaField}[{decision.Index}].criterionRevisionId";
            if (!known.TryGetValue(decision.RevisionId, out var criterion))
            {
                errors[field] = [$"Revision {decision.RevisionId} is not a criterion of this selection in this release."];
            }
            else if (!byRevision.TryAdd(decision.RevisionId, decision))
            {
                errors[field] = [$"Criterion '{criterion.Key}' (revision {decision.RevisionId}) is listed more than once."];
            }
            else if (decision.OverrideWeight is not null && EffectiveLevel(criterion, decision) != PriorityLevel.B)
            {
                errors[$"{CriteriaField}[{decision.Index}].overrideWeight"] = ["A weight applies only while the level is B."];
            }
        }

        var missing = criteria.Where(c => !byRevision.ContainsKey(c.RevisionId)).ToList();
        if (missing.Count > 0)
        {
            errors[CriteriaField] = [
                "Every criterion of the selection must be listed once; missing: " +
                string.Join(", ", missing.Select(c => $"'{c.Key}' (revision {c.RevisionId})")) + "."];
        }

        if (errors.Count > 0)
        {
            return [];
        }

        return [.. criteria.Select(c =>
        {
            var decision = byRevision[c.RevisionId];
            return new CheckedCriterion(c, decision.IsIncluded, decision.OverrideLevel, decision.OverrideWeight, decision.Note);
        })];
    }

    private static PriorityLevel EffectiveLevel(ResolvedCatalogCriterion criterion, Decision decision) =>
        decision.OverrideLevel ?? criterion.RecommendedPriority.Level;

    /// <summary>The level names of SPEC §8.2 and the UX keys: <c>A</c>, <c>B</c>, <c>Info</c>, exactly.</summary>
    private static PriorityLevel? LevelOf(string name) => name switch
    {
        "A" => PriorityLevel.A,
        "B" => PriorityLevel.B,
        "Info" => PriorityLevel.Info,
        _ => null,
    };

    /// <summary>The catalog keys its errors as the query of the criteria endpoint; the body nests them below <c>selection</c>.</summary>
    private static string SelectionKey(string catalogKey) =>
        catalogKey == "useCase" ? UseCaseField : $"{SelectionField}.{catalogKey}";

    private static DraftCheckResult Invalid(Dictionary<string, string[]> errors) =>
        Refused(DraftCheckOutcome.Invalid, InvalidDetail, errors);

    private static DraftCheckResult Refused(DraftCheckOutcome outcome, string detail, IReadOnlyDictionary<string, string[]> errors) =>
        new(outcome, detail, errors, null);

    private static Dictionary<string, string[]> NoErrors() => new(StringComparer.Ordinal);

    private sealed record Decision(int Index, int RevisionId, bool IsIncluded, PriorityLevel? OverrideLevel, int? OverrideWeight, string? Note);
}
