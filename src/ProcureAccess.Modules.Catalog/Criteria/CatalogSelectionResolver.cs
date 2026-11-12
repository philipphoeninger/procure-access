// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Reading;

namespace ProcureAccess.Modules.Catalog.Criteria;

/// <summary>
/// <see cref="ICatalogSelectionResolver"/> on the code of <c>GET /api/v1/catalog/criteria</c>: the
/// same release rule (a stored release that <c>catalog activate</c> would accept), the same
/// selection check (<see cref="CriteriaSelectionReader"/>) and the same resolution
/// (<see cref="CriteriaResolver.ResolveCriteria"/>), so the export and the criteria list S11 can
/// never disagree about which criteria a selection has and what the catalog recommends (S4-19).
/// </summary>
internal sealed class CatalogSelectionResolver(CatalogReader reader) : ICatalogSelectionResolver
{
    public async Task<CatalogSelectionResult> ResolveAsync(
        string releaseVersion,
        CatalogSelectionQuery selection,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(releaseVersion);
        ArgumentNullException.ThrowIfNull(selection);
        var noErrors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        var snapshot = await reader.GetReleaseByVersionAsync(releaseVersion, cancellationToken);
        if (snapshot is not { IsComplete: true })
        {
            return new CatalogSelectionResult(CatalogSelectionOutcome.ReleaseNotFound, null, null, noErrors);
        }

        var activeRelease = await reader.GetActiveReleaseAsync(cancellationToken);
        var release = new CatalogReleaseSummary(snapshot.Version, activeRelease?.Id == snapshot.Id, snapshot.Criteria.Count);

        var values = await reader.GetFilterValuesAsync(cancellationToken);
        var checkedSelection = CriteriaSelectionReader.Read(
            values,
            snapshot.Exclusions,
            selection.UseCase is null ? [] : [selection.UseCase],
            selection.AppTypeIds,
            selection.ProductPartIds,
            selection.TestTypeIds);
        if (checkedSelection.Selection is not { } valid)
        {
            var outcome = checkedSelection.Problem == SelectionProblem.Excluded
                ? CatalogSelectionOutcome.ExcludedSelection
                : CatalogSelectionOutcome.InvalidSelection;
            return new CatalogSelectionResult(outcome, release, null, checkedSelection.Errors);
        }

        var appTypes = values.AppTypes.ToDictionary(v => v.Key, v => v.NameDe, StringComparer.Ordinal);
        var productParts = values.ProductParts.ToDictionary(v => v.Key, v => v.NameDe, StringComparer.Ordinal);
        var testTypes = values.TestTypes.ToDictionary(v => v.Key, v => v, StringComparer.Ordinal);
        var useCase = valid.UseCase is null
            ? null
            : new CatalogValueName(valid.UseCase, values.UseCases.Single(u => u.Key == valid.UseCase).NameDe);

        var criteria = CriteriaResolver.ResolveCriteria(values, snapshot, valid)
            .Select(r => new ResolvedCatalogCriterion(
                r.Criterion.RevisionId,
                r.Criterion.Rules.Key,
                r.Criterion.Rules.Kind,
                r.Number,
                r.Criterion.TitleDe,
                r.Criterion.PlainLanguageDe,
                r.Texts.Count > 0 ? r.Texts[0].OriginalTitleDe : null,
                r.ProductPart is null ? null : productParts[r.ProductPart],
                new CatalogPriority(r.Priority.Level, r.Priority.Weight, r.Priority.RationaleDe),
                TestStepUrl(r, testTypes)))
            .ToList();

        return new CatalogSelectionResult(
            CatalogSelectionOutcome.Resolved,
            release,
            new ResolvedCatalogSelection(
                useCase,
                InOrder(valid.AppTypes, values.AppTypes, a => a.SortOrder),
                InOrder(valid.ProductParts, values.ProductParts, p => p.SortOrder),
                InOrder(valid.TestTypes, values.TestTypes, t => t.SortOrder),
                criteria),
            noErrors);
    }

    private static string? TestStepUrl(ResolvedCriterion resolved, Dictionary<string, TestType> testTypes)
    {
        if (resolved.Texts.Count > 0)
        {
            return resolved.Texts[0].SourceUrl;
        }

        return resolved.IsProcedure && resolved.Criterion.TestType is { } testType && testTypes.TryGetValue(testType, out var value)
            ? value.Url
            : null;
    }

    /// <summary>The selected values in the layer's sort order, by key and German name.</summary>
    private static List<CatalogValueName> InOrder<T>(IReadOnlyList<string> selected, IReadOnlyList<T> values, Func<T, int> sortOrder)
        where T : FilterValue =>
        [.. values
            .Where(v => selected.Contains(v.Key))
            .OrderBy(sortOrder).ThenBy(v => v.Key, StringComparer.Ordinal)
            .Select(v => new CatalogValueName(v.Key, v.NameDe))];
}
