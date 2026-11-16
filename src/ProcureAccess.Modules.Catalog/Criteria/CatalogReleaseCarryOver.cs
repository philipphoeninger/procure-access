// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.Catalog.Reading;

namespace ProcureAccess.Modules.Catalog.Criteria;

/// <inheritdoc cref="ICatalogReleaseCarryOver"/>
/// <remarks>
/// <para><b>Dropping (CAT-22).</b> Exclusion rules point from a higher to a lower layer (the
/// importer refuses anything else, CAT-13), so going down the layers and dropping every value that
/// a kept value of a higher layer excludes leaves exactly the values the active release allows.
/// Filter values belong to no release (C104); a key the catalog no longer knows is dropped and named
/// by its key.</para>
/// <para><b>Successors.</b> <c>SupersededByKey</c> is set on the old criterion when a later release
/// is imported, so it is read from the database on every call, not from the release cache (which a
/// later import cannot clear). The importer refuses a split, so a key has at most one successor.</para>
/// </remarks>
internal sealed class CatalogReleaseCarryOver(
    ICatalogSelectionResolver resolver,
    CatalogReader reader,
    CatalogDbContext db) : ICatalogReleaseCarryOver
{
    private static readonly Dictionary<string, string[]> NoErrors = new(StringComparer.Ordinal);

    public async Task<CatalogCarryOverResult> CarryOverAsync(
        string fromReleaseVersion,
        CatalogSelectionQuery selection,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(fromReleaseVersion);
        ArgumentNullException.ThrowIfNull(selection);

        var previous = await resolver.ResolveAsync(fromReleaseVersion, selection, cancellationToken);
        if (previous.Outcome == CatalogSelectionOutcome.ReleaseNotFound)
        {
            return Refused(CatalogCarryOverOutcome.SourceReleaseNotFound, null, null, NoErrors);
        }

        if (previous.Selection is not { } old)
        {
            // Stored releases never change (CAT-05), so a stored selection always resolves.
            throw new InvalidOperationException(
                $"The selection no longer resolves in its release '{fromReleaseVersion}' ({previous.Outcome}).");
        }

        var active = await reader.GetActiveReleaseAsync(cancellationToken);
        if (active is null)
        {
            return Refused(CatalogCarryOverOutcome.NoActiveRelease, null, old, NoErrors);
        }

        var values = await reader.GetFilterValuesAsync(cancellationToken);
        var trimmed = Trim(old, values, active.Exclusions);
        var resolved = await resolver.ResolveAsync(active.Version, trimmed.Query, cancellationToken);
        if (resolved.Selection is not { } current)
        {
            return Refused(CatalogCarryOverOutcome.NoSelectionLeft, resolved.Release, old, resolved.Errors) with
            {
                DroppedValues = trimmed.Dropped,
            };
        }

        var successors = await SuccessorsAsync(old.Criteria, current.Criteria, cancellationToken);
        return new CatalogCarryOverResult(
            CatalogCarryOverOutcome.CarriedOver, resolved.Release, old, current, trimmed.Dropped, successors, NoErrors);
    }

    /// <summary>
    /// The selection without the values the active release does not allow, and those values in
    /// layer order. A use case the catalog no longer offers is dropped first (CAT-18), so its
    /// exclusions no longer apply.
    /// </summary>
    internal static (CatalogSelectionQuery Query, List<CatalogValueName> Dropped) Trim(
        ResolvedCatalogSelection old, CatalogFilterValues values, IReadOnlyList<ExclusionRule> exclusions)
    {
        var dropped = new List<CatalogValueName>();
        var kept = new HashSet<FilterValueRef>();

        string? useCase = null;
        if (old.UseCase is { } oldUseCase)
        {
            if (values.UseCases.Any(u => u.Key == oldUseCase.Key && u.IsActive))
            {
                useCase = oldUseCase.Key;
                kept.Add(new FilterValueRef(FilterLayer.UseCase, useCase));
            }
            else
            {
                dropped.Add(oldUseCase);
            }
        }

        List<string> Keep(IReadOnlyList<CatalogValueName> selected, FilterLayer layer, IEnumerable<string> known)
        {
            var knownKeys = known.ToHashSet(StringComparer.Ordinal);
            var result = new List<string>();
            foreach (var value in selected)
            {
                var target = new FilterValueRef(layer, value.Key);
                if (knownKeys.Contains(value.Key) && !exclusions.Any(rule => rule.Target == target && kept.Contains(rule.Source)))
                {
                    result.Add(value.Key);
                }
                else
                {
                    dropped.Add(value);
                }
            }

            // Added after the whole layer, so a value never decides about another of its own layer.
            kept.UnionWith(result.Select(key => new FilterValueRef(layer, key)));
            return result;
        }

        var appTypes = Keep(old.AppTypes, FilterLayer.AppType, values.AppTypes.Select(v => v.Key));
        var productParts = Keep(old.ProductParts, FilterLayer.ProductPart, values.ProductParts.Select(v => v.Key));
        var testTypes = Keep(old.TestTypes, FilterLayer.TestType, values.TestTypes.Select(v => v.Key));
        return (new CatalogSelectionQuery(useCase, [.. appTypes], [.. productParts], [.. testTypes]), dropped);
    }

    /// <summary>For each old criterion the revision of the active selection that continues it: same key, else its successor's.</summary>
    private async Task<Dictionary<int, int>> SuccessorsAsync(
        IReadOnlyList<ResolvedCatalogCriterion> old,
        IReadOnlyList<ResolvedCatalogCriterion> current,
        CancellationToken cancellationToken)
    {
        var currentByKey = current.ToDictionary(c => c.Key, c => c.RevisionId, StringComparer.Ordinal);
        var supersededBy = await db.Criteria.AsNoTracking()
            .Where(c => c.SupersededByKey != null)
            .Select(c => new { c.Key, c.SupersededByKey })
            .ToDictionaryAsync(c => c.Key, c => c.SupersededByKey!, StringComparer.Ordinal, cancellationToken);

        var successors = new Dictionary<int, int>();
        foreach (var criterion in old)
        {
            var key = criterion.Key;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            while (!currentByKey.ContainsKey(key) && seen.Add(key) && supersededBy.TryGetValue(key, out var next))
            {
                key = next;
            }

            if (currentByKey.TryGetValue(key, out var revisionId))
            {
                successors[criterion.RevisionId] = revisionId;
            }
        }

        return successors;
    }

    private static CatalogCarryOverResult Refused(
        CatalogCarryOverOutcome outcome,
        CatalogReleaseSummary? release,
        ResolvedCatalogSelection? previous,
        IReadOnlyDictionary<string, string[]> errors) =>
        new(outcome, release, previous, null, [], new Dictionary<int, int>(), errors);
}
