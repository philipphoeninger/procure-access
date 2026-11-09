// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;

namespace ProcureAccess.Modules.Catalog.Releases;

/// <summary>What completeness needs to know of one criterion of a release.</summary>
internal sealed record CriterionCompletenessFacts(string Key, CriterionKind Kind, string? TestTypeKey, bool HasCatalogDefault);

/// <summary>
/// The completeness rules of a release (ARCHITECTURE §5.1 „Activation“, C106): every criterion,
/// procedure criteria included, has a catalog-default priority, and every seeded test type has its
/// procedure criterion. <c>catalog activate</c> refuses an incomplete release; the criteria endpoint
/// answers 404 for one (owner, S3-20 decision 7). One function, so both mean the same.
/// </summary>
internal static class ReleaseCompleteness
{
    /// <summary>Every gap in a stable order: criteria by key, then test types in the given order.</summary>
    public static List<string> FindGaps(IEnumerable<CriterionCompletenessFacts> criteria, IEnumerable<string> testTypeKeys)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        ArgumentNullException.ThrowIfNull(testTypeKeys);
        var list = criteria.ToList();

        var gaps = list
            .Where(c => !c.HasCatalogDefault)
            .Select(c => c.Key)
            .Order(StringComparer.Ordinal)
            .Select(key => $"Criterion '{key}' has no catalog-default priority (a rule without app type and use case).")
            .ToList();
        var procedureTestTypes = list
            .Where(c => c.Kind == CriterionKind.TestProcedure && c.TestTypeKey is not null)
            .Select(c => c.TestTypeKey!)
            .ToHashSet(StringComparer.Ordinal);
        gaps.AddRange(testTypeKeys
            .Where(key => !procedureTestTypes.Contains(key))
            .Select(key => $"Test type '{key}' has no procedure criterion (PROC-{key})."));
        return gaps;
    }
}
