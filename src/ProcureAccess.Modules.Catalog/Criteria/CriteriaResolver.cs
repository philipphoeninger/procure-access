// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Reading;

namespace ProcureAccess.Modules.Catalog.Criteria;

/// <summary>
/// Builds the answer of <c>GET /api/v1/catalog/criteria</c> (SPEC §8.2) for a checked selection
/// (<see cref="CriteriaSelectionReader"/>) and a complete release. Pure: the endpoint loads both.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><b>Which criteria:</b> every requirement criterion that applies to a selected app type
/// with no product part or a selected one, and the procedure criterion of each selected test type
/// (CAT-16); in list order (revision sort order, then key).</item>
/// <item><b>Priority:</b> <see cref="PriorityResolver"/> (CAT-01, CAT-02, CAT-14).</item>
/// <item><b>Texts and ratings (CAT-16, CAT-21, C31):</b> the criterion's procedure texts of the
/// test types the release maps to a selected app type; the allowed ratings are their union, all six
/// levels when none applies, and empty for a procedure criterion, which is never rated (owner,
/// S3-20 decision 3).</item>
/// <item><b><c>productPart</c> (decision 2):</b> of the applicability rows that match the
/// selection, the named product part with the lowest sort order; null when none names a part.</item>
/// <item><b><c>number</c>:</b> the key of a requirement criterion (the EN 301 549 number, S3-15);
/// null for a procedure criterion, as its export row has no number (SPEC §8.3).</item>
/// <item><b><c>appTypesWithoutCriteria</c> (CAT-20):</b> the selected app types to which no
/// requirement criterion applies under this selection, in sort order.</item>
/// </list>
/// </remarks>
internal static class CriteriaResolver
{
    public static CriteriaResponse Resolve(CatalogFilterValues values, CatalogReleaseSnapshot release, CriteriaSelection selection)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(release);
        ArgumentNullException.ThrowIfNull(selection);
        if (!release.IsComplete)
        {
            throw new ArgumentException($"Release {release.Version} is incomplete and cannot be served.", nameof(release));
        }

        var testTypeOrder = values.TestTypes.ToDictionary(t => t.Key, t => t.SortOrder, StringComparer.Ordinal);
        var partOrder = values.ProductParts.ToDictionary(p => p.Key, p => p.SortOrder, StringComparer.Ordinal);
        var mappedTestTypes = release.Procedures
            .Where(p => selection.AppTypes.Contains(p.AppType))
            .Select(p => p.TestType)
            .ToHashSet(StringComparer.Ordinal);

        var criteria = new List<CriterionItem>();
        foreach (var criterion in release.Criteria)
        {
            var isProcedure = criterion.Rules.Kind == CriterionKind.TestProcedure;
            var included = isProcedure
                ? criterion.TestType is not null && selection.TestTypes.Contains(criterion.TestType)
                : criterion.Rules.AppliesTo(selection);
            if (!included)
            {
                continue;
            }

            var priority = PriorityResolver.Resolve(criterion.Rules, selection, values)
                ?? throw new InvalidOperationException(
                    $"Criterion '{criterion.Rules.Key}' of complete release {release.Version} has no priority for the selection.");
            var texts = criterion.ProcedureTexts
                .Where(t => mappedTestTypes.Contains(t.TestType))
                .OrderBy(t => testTypeOrder.GetValueOrDefault(t.TestType, int.MaxValue))
                .ThenBy(t => t.TestType, StringComparer.Ordinal)
                .ToList();
            var allowed = isProcedure
                ? Rating.None
                : texts.Count == 0 ? Rating.All : texts.Aggregate(Rating.None, (union, t) => union | t.AllowedRatings);

            criteria.Add(new CriterionItem(
                criterion.RevisionId,
                criterion.Rules.Key,
                isProcedure ? "testProcedure" : "requirement",
                isProcedure ? null : criterion.Rules.Key,
                criterion.TitleDe,
                criterion.PlainLanguageDe,
                isProcedure ? null : ProductPartOf(criterion.Rules, selection, partOrder),
                [.. criterion.StandardReferences.Select(r => new StandardReferenceItem(StandardName(r.Standard), r.Clause, r.Url))],
                RatingNames(allowed),
                [.. texts.Select(t => new ProcedureTextItem(
                    t.TestType,
                    t.OriginalNumber,
                    t.OriginalTitleDe,
                    t.ApplicabilityDe,
                    t.RequirementDe,
                    t.RationaleWhyDe,
                    RatingNames(t.AllowedRatings),
                    t.PositiveExamplesDe,
                    t.NegativeExamplesDe,
                    t.SourceUrl))],
                new ResolvedPriorityItem(
                    priority.Level.ToString(),
                    priority.Weight,
                    priority.RationaleDe,
                    SourceName(priority.Source),
                    new MatchedRuleItem(RuleKindName(priority.MatchedRule.Kind), priority.MatchedRule.AppType, priority.MatchedRule.UseCase))));
        }

        var withoutCriteria = values.AppTypes
            .Where(a => selection.AppTypes.Contains(a.Key))
            .Where(a => !release.Criteria.Any(c =>
                c.Rules.Kind == CriterionKind.Requirement && c.Rules.AppTypesApplyingTo(selection).Contains(a.Key)))
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Key, StringComparer.Ordinal)
            .Select(a => new AppTypeItem(a.Key, a.NameDe))
            .ToList();

        return new CriteriaResponse(release.Version, criteria, withoutCriteria);
    }

    /// <summary>Owner, S3-20 decision 2: SPEC §8.2 sends one part (Kriterienkatalog K-4).</summary>
    private static string? ProductPartOf(CriterionRules rules, CriteriaSelection selection, Dictionary<string, int> partOrder) =>
        rules.Applicability
            .Where(a => selection.AppTypes.Contains(a.AppType) && a.ProductPart is not null && selection.ProductParts.Contains(a.ProductPart))
            .Select(a => a.ProductPart!)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(p => partOrder.GetValueOrDefault(p, int.MaxValue))
            .ThenBy(p => p, StringComparer.Ordinal)
            .FirstOrDefault();

    /// <summary>The set levels in the order of <see cref="Rating"/>, by their camelCase names (SPEC §8.2; never <c>unknown</c>).</summary>
    private static List<string> RatingNames(Rating ratings) =>
        [.. RatingOrder.Where(r => ratings.HasFlag(r.Rating)).Select(r => r.Name)];

    private static readonly (Rating Rating, string Name)[] RatingOrder =
    [
        (Rating.Fulfilled, "fulfilled"),
        (Rating.SlightRestriction, "slightRestriction"),
        (Rating.Restriction, "restriction"),
        (Rating.Barrier, "barrier"),
        (Rating.Blockade, "blockade"),
        (Rating.NotApplicable, "notApplicable"),
    ];

    private static string StandardName(Standard standard) => standard switch
    {
        Standard.En301549 => "en301549",
        Standard.Wcag => "wcag",
        Standard.BitInklusiv => "bitInklusiv",
        Standard.Iso9241 => "iso9241",
        _ => throw new ArgumentOutOfRangeException(nameof(standard), standard, "Unknown standard."),
    };

    private static string SourceName(PrioritySource source) => source switch
    {
        PrioritySource.ExpertJudgement => "expertJudgement",
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown priority source."),
    };

    private static string RuleKindName(MatchedRuleKind kind) => kind switch
    {
        MatchedRuleKind.AppTypeAndUseCase => "appTypeAndUseCase",
        MatchedRuleKind.AppType => "appType",
        MatchedRuleKind.UseCase => "useCase",
        MatchedRuleKind.Default => "default",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown rule kind."),
    };
}
