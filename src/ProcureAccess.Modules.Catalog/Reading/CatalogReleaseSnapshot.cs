// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Criteria;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Releases;

namespace ProcureAccess.Modules.Catalog.Reading;

/// <summary>
/// What the cache holds for one release (ARCHITECTURE §9.6, CAT-09): its identity, its criteria in
/// list order with everything the criteria endpoint returns, the exclusion rules and the app type →
/// procedure mapping it applies (CAT-19, CAT-21), and its gaps (<see cref="ReleaseCompleteness"/>).
/// Built from the entities by <see cref="From"/>, so the reader and the unit tests share one path.
/// </summary>
internal sealed record CatalogReleaseSnapshot(
    int Id,
    string Version,
    DateTimeOffset PublishedAt,
    IReadOnlyList<CriterionSnapshot> Criteria,
    IReadOnlyList<ExclusionRule> Exclusions,
    IReadOnlyList<ProcedureMapping> Procedures,
    IReadOnlyList<string> Gaps)
{
    /// <summary>The criterion keys in list order (revision sort order, then key).</summary>
    public IReadOnlyList<string> CriterionKeys => [.. Criteria.Select(c => c.Rules.Key)];

    /// <summary>
    /// False for a release <c>catalog activate</c> would refuse; the criteria endpoint does not serve
    /// it, because some priority could not explain itself (CAT-02; owner, S3-20 decision 7).
    /// </summary>
    public bool IsComplete => Gaps.Count == 0;

    /// <summary>
    /// The snapshot of a stored (or, in unit tests, built) release. <paramref name="testTypeKeys"/>
    /// are the seeded test types in sort order, which completeness checks against.
    /// </summary>
    public static CatalogReleaseSnapshot From(CatalogRelease release, IEnumerable<string> testTypeKeys)
    {
        ArgumentNullException.ThrowIfNull(release);
        ArgumentNullException.ThrowIfNull(testTypeKeys);
        if (release.PublishedAt is not { } publishedAt)
        {
            throw new ArgumentException($"Release {release.Version} is not published.", nameof(release));
        }

        var criteria = release.Criteria
            .Select(c => CriterionSnapshot.From(c.Revision))
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Rules.Key, StringComparer.Ordinal)
            .ToList();
        var exclusions = release.Exclusions
            .Select(e => new ExclusionRule(Ref(e.Exclusion.Source), Ref(e.Exclusion.Target)))
            .ToList();
        var procedures = release.Procedures
            .Select(p => new ProcedureMapping(p.AppType.Key, p.TestType.Key))
            .ToList();
        var gaps = ReleaseCompleteness.FindGaps(
            criteria.Select(c => new CriterionCompletenessFacts(
                c.Rules.Key, c.Rules.Kind, c.TestType, c.Rules.Priorities.Any(p => p.AppType is null && p.UseCase is null))),
            testTypeKeys);

        return new CatalogReleaseSnapshot(release.Id, release.Version, publishedAt, criteria, exclusions, procedures, gaps);

        static FilterValueRef Ref(FilterValue? value) =>
            value is null
                ? throw new InvalidOperationException("An exclusion rule was loaded without its filter values.")
                : new FilterValueRef(value.Layer, value.Key);
    }
}

/// <summary>One criterion revision of a release as the criteria endpoint needs it (SPEC §8.2).</summary>
internal sealed record CriterionSnapshot(
    int RevisionId,
    CriterionRules Rules,
    string? TestType,
    string TitleDe,
    string? PlainLanguageDe,
    string? ShortDescriptionDe,
    int SortOrder,
    IReadOnlyList<StandardReferenceSnapshot> StandardReferences,
    IReadOnlyList<ProcedureTextSnapshot> ProcedureTexts)
{
    public static CriterionSnapshot From(CriterionRevision revision)
    {
        ArgumentNullException.ThrowIfNull(revision);
        return new CriterionSnapshot(
            revision.Id,
            CriterionRules.From(revision),
            revision.Criterion.TestType?.Key,
            revision.TitleDe,
            revision.PlainLanguageDe,
            revision.ShortDescriptionDe,
            revision.SortOrder,
            [.. revision.StandardReferences
                .OrderBy(r => r.Standard).ThenBy(r => r.Clause, StringComparer.Ordinal)
                .Select(r => new StandardReferenceSnapshot(r.Standard, r.Clause, r.Url))],
            [.. revision.ProcedureTexts.Select(t => new ProcedureTextSnapshot(
                t.TestType.Key,
                t.OriginalNumber,
                t.OriginalTitleDe,
                t.ApplicabilityDe,
                t.RequirementDe,
                t.RationaleWhyDe,
                t.AllowedRatings,
                t.PositiveExamplesDe,
                t.NegativeExamplesDe,
                t.SourceUrl))]);
    }
}

/// <summary>A cited clause of a standard.</summary>
internal sealed record StandardReferenceSnapshot(Standard Standard, string Clause, string? Url);

/// <summary>The verbatim BIT inklusiv text of a criterion for one test type (ARCHITECTURE §5.1, CAT-16).</summary>
internal sealed record ProcedureTextSnapshot(
    string TestType,
    string OriginalNumber,
    string OriginalTitleDe,
    string? ApplicabilityDe,
    string? RequirementDe,
    string? RationaleWhyDe,
    Rating AllowedRatings,
    string? PositiveExamplesDe,
    string? NegativeExamplesDe,
    string SourceUrl);

/// <summary>A row of the release's app type → procedure mapping (<c>CatalogReleaseAppTypeProcedure</c>, CAT-21).</summary>
internal sealed record ProcedureMapping(string AppType, string TestType);
