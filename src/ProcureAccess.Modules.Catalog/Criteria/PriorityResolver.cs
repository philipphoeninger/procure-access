// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Reading;

namespace ProcureAccess.Modules.Catalog.Criteria;

/// <summary>Which level of the chain decided (SPEC §8.2 <c>matchedRule.kind</c>).</summary>
internal enum MatchedRuleKind
{
    /// <summary>Rule 1: app type and use case.</summary>
    AppTypeAndUseCase = 1,

    /// <summary>Rule 2: app type only.</summary>
    AppType = 2,

    /// <summary>Rule 3: use case only.</summary>
    UseCase = 3,

    /// <summary>Rule 4: the catalog default.</summary>
    Default = 4,
}

/// <summary>The rule that decided, by the keys it names; for rules 1 and 2 the app type is the deciding one (CAT-14).</summary>
internal sealed record MatchedRule(MatchedRuleKind Kind, string? AppType, string? UseCase);

/// <summary>
/// The recommended priority of a criterion for a selection, with the rule it comes from and that
/// rule's rationale and source, so it explains itself (CAT-02, PRD §7 „Transparenz“).
/// </summary>
internal sealed record ResolvedPriority(
    PriorityLevel Level,
    int? Weight,
    string RationaleDe,
    PrioritySource Source,
    MatchedRule MatchedRule);

/// <summary>
/// Priority resolution of ARCHITECTURE §5.1 (CAT-01, CAT-02, CAT-14). Pure and deterministic.
/// </summary>
/// <remarks>
/// <para><b>Per app type (owner, S3-20 decision 1).</b> Each selected app type the criterion applies
/// to (<see cref="CriterionRules.AppTypesApplyingTo"/>) runs the chain on its own, most specific
/// first: (app type, use case) → (app type, –) → (–, use case) → (–, –). Without a use case only
/// rules 2 and 4 apply. A procedure criterion has no applicability, so only rules 3 and 4 apply.</para>
/// <para><b>Strictest wins.</b> Of the results of the app types: the stricter level (A &gt; B &gt;
/// Info), then the higher weight (a rule without weight ranks below one with), then the lower
/// <c>AppType.SortOrder</c>, then the key. So a selected app type never gets a looser priority
/// than its own chain gives.</para>
/// <para><b>No guessing.</b> If the chain of an app type finds no rule, the release has no catalog
/// default for the criterion and is incomplete: the result is null. <c>catalog activate</c>
/// refuses such a release, and the criteria endpoint answers 404 for it (owner, S3-20).</para>
/// </remarks>
internal static class PriorityResolver
{
    public static ResolvedPriority? Resolve(CriterionRules criterion, CriteriaSelection selection, CatalogFilterValues values)
    {
        ArgumentNullException.ThrowIfNull(criterion);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(values);

        var appTypes = criterion.AppTypesApplyingTo(selection);
        if (criterion.Kind == CriterionKind.Requirement && appTypes.Count == 0)
        {
            throw new ArgumentException(
                $"Criterion '{criterion.Key}' does not apply to the selection, so it has no priority for it.", nameof(criterion));
        }

        var candidates = new List<(PriorityRule Rule, AppType? AppType)>();
        if (appTypes.Count == 0)
        {
            var rule = FirstMatch(criterion.Priorities, null, selection.UseCase);
            if (rule is null)
            {
                return null;
            }

            candidates.Add((rule, null));
        }

        foreach (var key in appTypes)
        {
            var rule = FirstMatch(criterion.Priorities, key, selection.UseCase);
            if (rule is null)
            {
                return null;
            }

            var appType = values.AppTypes.FirstOrDefault(a => a.Key == key)
                ?? throw new InvalidOperationException($"App type '{key}' is not a filter value.");
            candidates.Add((rule, appType));
        }

        var winner = candidates
            .OrderBy(c => c.Rule.Level)
            .ThenByDescending(c => c.Rule.Weight ?? int.MinValue)
            .ThenBy(c => c.AppType?.SortOrder ?? int.MaxValue)
            .ThenBy(c => c.AppType?.Key, StringComparer.Ordinal)
            .First()
            .Rule;
        return new ResolvedPriority(
            winner.Level,
            winner.Weight,
            winner.RationaleDe,
            winner.Source,
            new MatchedRule(KindOf(winner), winner.AppType, winner.UseCase));
    }

    /// <summary>The most specific rule for one app type (or none), following the chain of §5.1.</summary>
    private static PriorityRule? FirstMatch(IReadOnlyList<PriorityRule> rules, string? appType, string? useCase)
    {
        PriorityRule? Find(string? a, string? u) => rules.FirstOrDefault(r => r.AppType == a && r.UseCase == u);

        return (appType is not null && useCase is not null ? Find(appType, useCase) : null)
            ?? (appType is not null ? Find(appType, null) : null)
            ?? (useCase is not null ? Find(null, useCase) : null)
            ?? Find(null, null);
    }

    private static MatchedRuleKind KindOf(PriorityRule rule) => (rule.AppType, rule.UseCase) switch
    {
        (not null, not null) => MatchedRuleKind.AppTypeAndUseCase,
        (not null, null) => MatchedRuleKind.AppType,
        (null, not null) => MatchedRuleKind.UseCase,
        _ => MatchedRuleKind.Default,
    };
}
