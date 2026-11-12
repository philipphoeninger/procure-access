// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Contracts;

/// <summary>
/// Resolves a selection of the four filter layers under a stored catalog release, the way
/// <c>GET /api/v1/catalog/criteria</c> does (ARCHITECTURE §4.4 Flow A; CAT-01, CAT-02, CAT-11,
/// CAT-12): the selection is checked, and every criterion that applies gets the priority the
/// catalog recommends. Other modules use it to re-resolve a selection on the server instead of
/// trusting a client (EXP-07; the ad-hoc export, S4-19).
/// </summary>
public interface ICatalogSelectionResolver
{
    /// <summary>
    /// Resolves <paramref name="selection"/> under the stored release <paramref name="releaseVersion"/>,
    /// active or not. The result says whether that release is the active one; the caller decides
    /// what an inactive release means for it.
    /// </summary>
    Task<CatalogSelectionResult> ResolveAsync(
        string releaseVersion,
        CatalogSelectionQuery selection,
        CancellationToken cancellationToken);
}

/// <summary>
/// A selection by filter-value keys, as the client sent it: empty values count as not given, a
/// repeated key once. Errors are keyed <c>useCase</c>, <c>appTypeIds</c>, <c>appTypeIds[1]</c>,
/// <c>productPartIds</c>, <c>testTypeIds[0]</c> …, with the index of the value in these lists.
/// </summary>
public sealed record CatalogSelectionQuery(
    string? UseCase,
    IReadOnlyList<string?> AppTypeIds,
    IReadOnlyList<string?> ProductPartIds,
    IReadOnlyList<string?> TestTypeIds);

/// <summary>What became of a selection.</summary>
public enum CatalogSelectionOutcome
{
    /// <summary>The selection is complete and allowed; <see cref="CatalogSelectionResult.Selection"/> holds its criteria.</summary>
    Resolved = 1,

    /// <summary>The release is not stored, or <c>catalog activate</c> would refuse it, so no priority could explain itself (CAT-02).</summary>
    ReleaseNotFound = 2,

    /// <summary>Not a complete selection: unknown key, two use cases, no app type, an always-selected part missing (problem type <c>validation</c>, CAT-11).</summary>
    InvalidSelection = 3,

    /// <summary>A value is excluded by a selected higher-layer value under the rules of the release (problem type <c>selection-excluded</c>, CAT-12).</summary>
    ExcludedSelection = 4,
}

/// <summary>
/// The outcome, the release (null for <see cref="CatalogSelectionOutcome.ReleaseNotFound"/>), the
/// resolved selection (only for <see cref="CatalogSelectionOutcome.Resolved"/>) and the field errors
/// of a refused selection, keyed as described on <see cref="CatalogSelectionQuery"/>.
/// </summary>
public sealed record CatalogSelectionResult(
    CatalogSelectionOutcome Outcome,
    CatalogReleaseSummary? Release,
    ResolvedCatalogSelection? Selection,
    IReadOnlyDictionary<string, string[]> Errors);

/// <summary>A stored, complete catalog release.</summary>
/// <param name="CriterionCount">Every criterion of the release, procedure criteria included.</param>
public sealed record CatalogReleaseSummary(string Version, bool IsActive, int CriterionCount);

/// <summary>
/// A checked selection with the German names of its values in layer order (for the „Info“ sheet of
/// the export, SPEC §8.3) and its criteria in list order.
/// </summary>
/// <param name="UseCase">Null for „Keiner davon passt“.</param>
public sealed record ResolvedCatalogSelection(
    CatalogValueName? UseCase,
    IReadOnlyList<CatalogValueName> AppTypes,
    IReadOnlyList<CatalogValueName> ProductParts,
    IReadOnlyList<CatalogValueName> TestTypes,
    IReadOnlyList<ResolvedCatalogCriterion> Criteria);

/// <summary>A filter value by key and German name.</summary>
public sealed record CatalogValueName(string Key, string Name);

/// <summary>
/// A criterion of a resolved selection with what a file about it needs (SPEC §8.3).
/// </summary>
/// <param name="RevisionId">The criterion revision of the release, which clients name in their decisions.</param>
/// <param name="Number">The EN 301 549 number of a requirement criterion; null for a procedure criterion („Geprüft nach …“).</param>
/// <param name="PlainLanguageTitle">Null until the project team writes it (O-1, K-1).</param>
/// <param name="OriginalTitle">The BIT inklusiv title of the first procedure text that applies; null without one.</param>
/// <param name="ProductPart">Name of the product part of the matching applicability rule; null when none names a part.</param>
/// <param name="TestStepUrl">The test step of the first procedure text that applies, or for a procedure criterion the page of its test type.</param>
public sealed record ResolvedCatalogCriterion(
    int RevisionId,
    string Key,
    CriterionKind Kind,
    string? Number,
    string Title,
    string? PlainLanguageTitle,
    string? OriginalTitle,
    string? ProductPart,
    CatalogPriority RecommendedPriority,
    string? TestStepUrl);

/// <summary>
/// The priority the catalog recommends for a criterion under a selection (CAT-02): level, weight
/// (only for B) and the rationale of the rule that decided. The source is always „Empfehlung des
/// Projektteams“ (SPEC §8.2 <c>expertJudgement</c>).
/// </summary>
public sealed record CatalogPriority(PriorityLevel Level, int? Weight, string Rationale);
