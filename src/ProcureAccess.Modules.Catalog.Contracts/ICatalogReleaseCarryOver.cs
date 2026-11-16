// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Contracts;

/// <summary>
/// Carries a selection of a stored catalog release over to the active release (ARCHITECTURE D6,
/// C58, §16.3.1; SPEC R-LST-14; CAT-22): what „Neue Liste mit derselben Auswahl“ needs from the
/// catalog. The caller keeps the user's decisions; the catalog says which values survive and which
/// criterion of the active release continues each criterion of the old selection.
/// </summary>
public interface ICatalogReleaseCarryOver
{
    /// <summary>
    /// Resolves <paramref name="selection"/> under <paramref name="fromReleaseVersion"/> and again
    /// under the active release. A use case the catalog no longer offers (CAT-18) and a value the
    /// exclusion rules of the active release exclude are dropped and named, top layer first, instead
    /// of refusing the selection. A criterion continues under its own key, else under the key that
    /// supersedes it (<c>Criterion.SupersededByKey</c>, followed across releases).
    /// </summary>
    Task<CatalogCarryOverResult> CarryOverAsync(
        string fromReleaseVersion,
        CatalogSelectionQuery selection,
        CancellationToken cancellationToken);
}

/// <summary>What became of a carry-over.</summary>
public enum CatalogCarryOverOutcome
{
    /// <summary>The selection resolves under the active release; the result holds both selections and the successors.</summary>
    CarriedOver = 1,

    /// <summary>The source release is not stored or not complete.</summary>
    SourceReleaseNotFound = 2,

    /// <summary>No release is active.</summary>
    NoActiveRelease = 3,

    /// <summary>
    /// What is left after dropping is no complete selection under the active release (e.g. every app
    /// type excluded); <see cref="CatalogCarryOverResult.Errors"/> says why, keyed as on
    /// <see cref="CatalogSelectionQuery"/>.
    /// </summary>
    NoSelectionLeft = 4,
}

/// <summary>
/// The outcome; the active release; the selection under the source release
/// (<paramref name="Previous"/>) and under the active release (<paramref name="Selection"/>, only
/// for <see cref="CatalogCarryOverOutcome.CarriedOver"/>); the dropped values by German name in
/// layer order; and for each criterion of <paramref name="Previous"/> that continues in
/// <paramref name="Selection"/> its revision there.
/// </summary>
/// <param name="Successors">Revision id under the source release → revision id under the active release.</param>
public sealed record CatalogCarryOverResult(
    CatalogCarryOverOutcome Outcome,
    CatalogReleaseSummary? Release,
    ResolvedCatalogSelection? Previous,
    ResolvedCatalogSelection? Selection,
    IReadOnlyList<CatalogValueName> DroppedValues,
    IReadOnlyDictionary<int, int> Successors,
    IReadOnlyDictionary<string, string[]> Errors);
