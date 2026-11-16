// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;
using ProcureAccess.Modules.Catalog.Contracts;

namespace ProcureAccess.Modules.Configurator.Contracts.Drafts;

/// <summary>
/// Checks a criteria draft — a selection and one decision per criterion — against the catalog and
/// re-resolves it on the server (ARCHITECTURE §4.4, C153; EXP-07, CAT-11, CAT-12): the one check
/// behind the anonymous export (<c>AdHocSelectionSource</c>) and saving a list
/// (<c>POST /api/v1/configurations</c>), so both accept and refuse exactly the same drafts.
/// Declared here and implemented by Configurator, which owns the decision rules (the 🔧 weight
/// range <c>Configurator:PriorityWeight</c>, the note limit).
/// </summary>
/// <remarks>
/// The check reads no saved list: the ad-hoc export may use it and still never touches a stored
/// configuration (C62, ARC-03). Field errors are keyed by their path in the request body —
/// <c>selection</c>, <c>selection.releaseVersion</c>, <c>selection.appTypeIds[1]</c>,
/// <c>criteria</c>, <c>criteria[0].overrideWeight</c> … — and never repeat a note.
/// </remarks>
public interface IConfigurationDraftCheck
{
    Task<DraftCheckResult> CheckAsync(CriteriaDraft draft, DraftPurpose purpose, CancellationToken cancellationToken);
}

/// <summary>What the draft is checked for; the rules differ only where a purpose says so.</summary>
public enum DraftPurpose
{
    /// <summary>The anonymous export: an inactive use case is accepted (CAT-18; owner, S3-20).</summary>
    Export = 1,

    /// <summary>A new saved list: an inactive use case is refused (CAT-18).</summary>
    Save = 2,
}

/// <summary>A draft as the client sends it. Every member is nullable, so a missing value becomes a field error.</summary>
public sealed record CriteriaDraft(DraftSelection? Selection, IReadOnlyList<DraftCriterion?>? Criteria);

/// <summary>
/// The selection by filter-value keys under the release the client's list was built for. Unknown
/// JSON members are refused, so a client cannot hand in anything the server resolves itself.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record DraftSelection(
    string? ReleaseVersion,
    string? UseCaseTemplateId,
    IReadOnlyList<string?>? AppTypeIds,
    IReadOnlyList<string?>? ProductPartIds,
    IReadOnlyList<string?>? TestTypeIds);

/// <summary>
/// The user's decision on one criterion (R-LST-03 – R-LST-05): included or not, the level
/// (<c>A</c>, <c>B</c>, <c>Info</c>) and weight chosen instead of the recommendation, and a note.
/// Unknown JSON members are refused (a recommended level or rationale is the server's, EXP-07).
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record DraftCriterion(
    int? CriterionRevisionId,
    bool? IsIncluded,
    string? OverrideLevel,
    int? OverrideWeight,
    string? Note);

/// <summary>What became of a draft.</summary>
public enum DraftCheckOutcome
{
    /// <summary>The draft is valid; <see cref="DraftCheckResult.Draft"/> holds it.</summary>
    Checked = 1,

    /// <summary>Field errors (problem type <c>validation</c>).</summary>
    Invalid = 2,

    /// <summary>A value is excluded by a selected higher-layer value (problem type <c>selection-excluded</c>, CAT-12).</summary>
    SelectionExcluded = 3,

    /// <summary>The release is not stored or not complete (404 <c>not-found</c>).</summary>
    ReleaseNotFound = 4,

    /// <summary>The release is stored but no longer active (400 <c>release-mismatch</c>, O-S4-60 (a)).</summary>
    ReleaseNotActive = 5,
}

/// <summary>
/// The outcome, a detail for the Problem Details, the field errors (empty for
/// <see cref="DraftCheckOutcome.Checked"/> and <see cref="DraftCheckOutcome.ReleaseNotFound"/>) and,
/// only for <see cref="DraftCheckOutcome.Checked"/>, the checked draft.
/// </summary>
public sealed record DraftCheckResult(
    DraftCheckOutcome Outcome,
    string Detail,
    IReadOnlyDictionary<string, string[]> Errors,
    CheckedDraft? Draft);

/// <summary>A valid draft: the release, the selection as the catalog resolved it, and the decision for each of its criteria in list order.</summary>
public sealed record CheckedDraft(string ReleaseVersion, ResolvedCatalogSelection Selection, IReadOnlyList<CheckedCriterion> Criteria);

/// <summary>
/// A criterion of the selection with the user's decision. <see cref="OverrideLevel"/> is the level
/// the user sent; whether it equals the recommendation is the consumer's matter.
/// </summary>
public sealed record CheckedCriterion(
    ResolvedCatalogCriterion Criterion,
    bool IsIncluded,
    PriorityLevel? OverrideLevel,
    int? OverrideWeight,
    string? Note);
