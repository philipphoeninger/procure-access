// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Export.Documents;
using ProcureAccess.Modules.Export.Requests;
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace ProcureAccess.Modules.Export.Sources;

/// <summary>
/// The content of the anonymous export (ARCHITECTURE §4.4 Flow A1, R-LST-12): the client's draft,
/// checked and re-resolved on the server. Depends on <c>Catalog.Contracts</c> only (ARC-03); it
/// reads and writes nothing else, so the export persists nothing (EXP-05).
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><b>Server resolves (EXP-07).</b> The recommended level, weight and rationale come from
/// <see cref="ICatalogSelectionResolver"/>; the request has no member for them (unknown members are
/// refused). A chosen level equal to the recommendation counts as no choice.</item>
/// <item><b>Active release only (owner decision O-S4-60 (a)).</b> An unknown or incomplete release
/// is 404 <c>not-found</c>; a stored release that is no longer active is 400
/// <c>release-mismatch</c> („inzwischen gilt eine neue Katalogversion“, <c>criteria.export.outdated</c>).</item>
/// <item><b>Selection (CAT-11, CAT-12).</b> Checked as by the criteria endpoint: 400
/// <c>validation</c> or <c>selection-excluded</c>, keyed below <c>selection</c>.</item>
/// <item><b>Every criterion once (owner decision O-S4-57 (a)).</b> The array names each criterion of
/// the selection exactly once, by its revision in the release; a missing, repeated or foreign one
/// is a field error, never a silently dropped or added row.</item>
/// <item><b>Limits (SPEC §7.2).</b> No more entries than the release has criteria; a note of at most
/// <see cref="NoteMaxLength"/> UTF-16 code units; a weight within <see cref="PriorityWeightRange"/>
/// (O-S4-58 (a)) and only while B applies. Error messages never repeat a note.</item>
/// </list>
/// </remarks>
internal sealed class AdHocSelectionSource(ICatalogSelectionResolver catalog, PriorityWeightRange weights)
    : IExportSource<ExportRequest>
{
    /// <summary>SPEC §7.2 „Criterion note“, as the client counts it (<c>string.Length</c>).</summary>
    public const int NoteMaxLength = 1000;

    private const string SelectionField = "selection";
    private const string ReleaseField = "selection.releaseVersion";
    private const string CriteriaField = "criteria";

    public async Task<ExportSourceResult> LoadAsync(ExportRequest input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var selection = input.Selection;
        if (selection is null)
        {
            errors[SelectionField] = ["The selection is required."];
        }
        else if (string.IsNullOrEmpty(selection.ReleaseVersion))
        {
            errors[ReleaseField] = ["The catalog release is required."];
        }

        if (input.Criteria is null)
        {
            errors[CriteriaField] = ["The criteria are required, one entry per criterion of the selection."];
        }

        var decisions = ReadDecisions(input.Criteria ?? [], errors);
        if (selection?.ReleaseVersion is not { Length: > 0 } version)
        {
            return Refused(ExportProblem.Validation(errors));
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
            return Refused(new ExportProblem(404, ProblemTypes.NotFound, $"Catalog release '{version}' is not stored or not complete.", null));
        }

        if (!resolved.Release.IsActive)
        {
            return Refused(new ExportProblem(
                400,
                ProblemTypes.ReleaseMismatch,
                $"Catalog release '{version}' is no longer the active release.",
                new Dictionary<string, string[]>(StringComparer.Ordinal)
                {
                    [ReleaseField] = [$"Catalog release '{version}' is no longer the active release; load the criteria again."],
                }));
        }

        var selectionErrors = resolved.Errors.ToDictionary(e => SelectionKey(e.Key), e => e.Value, StringComparer.Ordinal);
        if (resolved.Outcome == CatalogSelectionOutcome.ExcludedSelection && errors.Count == 0)
        {
            return Refused(new ExportProblem(400, ProblemTypes.SelectionExcluded, "The selection contains an excluded value.", selectionErrors));
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
            return Refused(ExportProblem.Validation(errors));
        }

        var entryCount = input.Criteria?.Count ?? 0;
        if (entryCount > resolved.Release.CriterionCount)
        {
            errors[CriteriaField] = [$"Release '{version}' has {resolved.Release.CriterionCount} criteria; the request lists {entryCount}."];
            return Refused(ExportProblem.Validation(errors));
        }

        var rows = Match(valid.Criteria, decisions, errors);
        return errors.Count > 0
            ? Refused(ExportProblem.Validation(errors))
            : ExportSourceResult.Of(new ExportContent(
                resolved.Release.Version, valid.UseCase, valid.AppTypes, valid.ProductParts, valid.TestTypes, rows));
    }

    /// <summary>Reads each entry and checks what needs no catalog: presence, level names, weight range, note length.</summary>
    private List<Decision> ReadDecisions(IReadOnlyList<ExportCriterionRequest?> entries, Dictionary<string, string[]> errors)
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
    /// Pairs each criterion of the selection with its decision and builds the rows of the included
    /// ones in list order; records every missing, repeated or foreign revision.
    /// </summary>
    private static List<ExportRow> Match(
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

        return [.. criteria
            .Select(c => (Criterion: c, Decision: byRevision[c.RevisionId]))
            .Where(p => p.Decision.IsIncluded)
            .Select(p => RowOf(p.Criterion, p.Decision))];
    }

    private static ExportRow RowOf(ResolvedCatalogCriterion criterion, Decision decision)
    {
        var recommended = criterion.RecommendedPriority;
        var chosen = decision.OverrideLevel == recommended.Level ? null : decision.OverrideLevel;
        var effective = chosen ?? recommended.Level;
        int? weight = effective != PriorityLevel.B
            ? null
            : decision.OverrideWeight ?? (recommended.Level == PriorityLevel.B ? recommended.Weight : null);

        return new ExportRow(
            criterion.Key,
            criterion.Kind,
            criterion.Number,
            criterion.Title,
            criterion.PlainLanguageTitle,
            criterion.ShortDescription,
            criterion.OriginalTitle,
            criterion.ProductPart,
            recommended.Level,
            chosen,
            weight,
            recommended.Rationale,
            string.IsNullOrEmpty(decision.Note) ? null : decision.Note,
            criterion.TestStepUrl);
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
        catalogKey == "useCase" ? "selection.useCaseTemplateId" : $"{SelectionField}.{catalogKey}";

    private static ExportSourceResult Refused(ExportProblem problem) => ExportSourceResult.Refused(problem);

    private sealed record Decision(int Index, int RevisionId, bool IsIncluded, PriorityLevel? OverrideLevel, int? OverrideWeight, string? Note);
}
