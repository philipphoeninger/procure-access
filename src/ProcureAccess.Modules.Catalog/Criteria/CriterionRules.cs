// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Criteria;

/// <summary>Where a requirement criterion applies: an app type, optionally only with one product part (null = with every part).</summary>
internal sealed record ApplicabilityRule(string AppType, string? ProductPart);

/// <summary>
/// A priority rule of a criterion by key: app type and use case both set (rule 1), only the app type
/// (2), only the use case (3) or neither, the catalog default (4) (ARCHITECTURE §5.1).
/// </summary>
internal sealed record PriorityRule(
    string? AppType,
    string? UseCase,
    PriorityLevel Level,
    int? Weight,
    string RationaleDe,
    PrioritySource Source);

/// <summary>
/// What priority resolution needs of one criterion revision, by key and without entities, so the
/// release cache can hold it (S3-20b) and the resolver stays pure.
/// </summary>
internal sealed record CriterionRules(
    string Key,
    CriterionKind Kind,
    IReadOnlyList<ApplicabilityRule> Applicability,
    IReadOnlyList<PriorityRule> Priorities)
{
    public static CriterionRules From(CriterionRevision revision)
    {
        ArgumentNullException.ThrowIfNull(revision);
        return new CriterionRules(
            revision.Criterion.Key,
            revision.Criterion.Kind,
            [.. revision.Applicability.Select(a => new ApplicabilityRule(a.AppType.Key, a.ProductPart?.Key))],
            [.. revision.Priorities.Select(p => new PriorityRule(
                p.AppType?.Key, p.UseCase?.Key, p.Level, p.Weight, p.RationaleDe, p.Source))]);
    }

    /// <summary>
    /// The selected app types the criterion applies to: an applicability row names the app type and
    /// either no product part or a selected one. Always empty for a procedure criterion, which has
    /// no applicability (C30).
    /// </summary>
    public IReadOnlyList<string> AppTypesApplyingTo(CriteriaSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return [.. selection.AppTypes.Where(appType => Applicability.Any(a =>
            a.AppType == appType && (a.ProductPart is null || selection.ProductParts.Contains(a.ProductPart))))];
    }

    /// <summary>True if the criterion applies to at least one selected app type (requirement criteria only).</summary>
    public bool AppliesTo(CriteriaSelection selection) => AppTypesApplyingTo(selection).Count > 0;
}
