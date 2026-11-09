// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Criteria;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Reading;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Shared.Builders.Catalog.CatalogData;
using Keys = ProcureAccess.Tests.Shared.Fixtures.MiniCatalog.Keys;

namespace ProcureAccess.Tests.Unit.Catalog.Criteria;

/// <summary>
/// Priority resolution of ARCHITECTURE §5.1 (CAT-01, CAT-02, CAT-14): each selected app type the
/// criterion applies to runs the chain 1 → 2 → 3 → 4 (without a use case 2 → 4), and the strictest
/// result wins — level, then weight, then <c>AppType.SortOrder</c> (owner, S3-20 decision 1).
/// Table-driven over the rules of <see cref="Keys.PriorityAllLevels"/> and
/// <see cref="Keys.PriorityAppTypes"/>; cases the MiniCatalog does not hold use small revisions of
/// their own.
/// </summary>
public class PriorityResolverTests
{
    private static readonly MiniCatalogValues V = MiniCatalogValues.InMemory();
    private static readonly MiniCatalog Catalog = MiniCatalog.Build(V);
    private static readonly CatalogFilterValues Values = new(
        [V.Hr, V.DatabaseApplication],
        [V.Web, V.SoftwareOpen, V.MobileOpen, V.Hardware],
        [V.Base, V.Audio, V.Video],
        V.TestTypes);

    /// <summary>
    /// One app type: one row per level of the chain and the fall-throughs (99.1: (Web, HR) A;
    /// (Software offen, –) Info; (–, HR) B weight 5; (–, Datenbank) A; default B).
    /// Columns: use case, app type, then the expected level, weight, rule kind (as in SPEC §8.2), rule app type, rule use case.
    /// </summary>
    public static TheoryData<string?, string, PriorityLevel, int?, string, string?, string?> OneAppType() => new()
    {
        // Rule 1.
        { "hr", "web", PriorityLevel.A, null, "appTypeAndUseCase", "web", "hr" },

        // Rule 1 missing → rule 2.
        { "hr", "softwareOpen", PriorityLevel.Info, null, "appType", "softwareOpen", null },

        // Rules 1 and 2 missing → rule 3.
        { "hr", "mobileOpen", PriorityLevel.B, 5, "useCase", null, "hr" },

        // Rule 3 of an inactive use case still applies (CAT-18; owner, S3-20).
        { "databaseApplication", "mobileOpen", PriorityLevel.A, null, "useCase", null, "databaseApplication" },
        { "databaseApplication", "web", PriorityLevel.A, null, "useCase", null, "databaseApplication" },

        // Rules 1 – 3 missing → rule 4.
        { null, "mobileOpen", PriorityLevel.B, null, "default", null, null },

        // Without a use case only rules 2 and 4: the rule (Web, HR) does not count.
        { null, "web", PriorityLevel.B, null, "default", null, null },
        { null, "softwareOpen", PriorityLevel.Info, null, "appType", "softwareOpen", null },
    };

    /// <summary>
    /// Several app types: the strictest result of the app types wins.
    /// Columns: criterion key, use case, app types, then the expected level, weight, rule kind, rule app type, rule use case.
    /// </summary>
    public static TheoryData<string, string?, string[], PriorityLevel, int?, string, string?, string?> SeveralAppTypes() => new()
    {
        // 99.1: rule 1 of Web (A) beats rule 2 of Software offen (Info).
        { Keys.PriorityAllLevels, "hr", ["web", "softwareOpen"], PriorityLevel.A, null, "appTypeAndUseCase", "web", "hr" },

        // 99.1: rule 3 for Mobile offen (B) beats rule 2 of Software offen (Info).
        { Keys.PriorityAllLevels, "hr", ["softwareOpen", "mobileOpen"], PriorityLevel.B, 5, "useCase", null, "hr" },

        // 99.1, owner's example (S3-20 decision 1): Web falls through to the default B, which beats Info of Software offen.
        { Keys.PriorityAllLevels, null, ["web", "softwareOpen"], PriorityLevel.B, null, "default", null, null },

        // 99.2: equal level and weight → the lower sort order (Web), also when the request names it last.
        { Keys.PriorityAppTypes, "hr", ["web", "softwareOpen"], PriorityLevel.B, 2, "appTypeAndUseCase", "web", "hr" },
        { Keys.PriorityAppTypes, "hr", ["softwareOpen", "web"], PriorityLevel.B, 2, "appTypeAndUseCase", "web", "hr" },

        // 99.2: equal level → the higher weight (Software offen 3 over Web 1).
        { Keys.PriorityAppTypes, null, ["web", "softwareOpen"], PriorityLevel.B, 3, "appType", "softwareOpen", null },

        // 99.2: the level comes before the weight; rule 2 of Mobile offen (A) beats rule 1 of the others (B, 2).
        { Keys.PriorityAppTypes, "hr", ["web", "softwareOpen", "mobileOpen"], PriorityLevel.A, null, "appType", "mobileOpen", null },
    };

    [Theory(DisplayName = "INV:CAT-01 " + nameof(Resolve_OneAppType_MostSpecificRuleWins))]
    [Trait("Invariant", "CAT-01")]
    [MemberData(nameof(OneAppType))]
    public void Resolve_OneAppType_MostSpecificRuleWins(
        string? useCase,
        string appType,
        PriorityLevel level,
        int? weight,
        string kind,
        string? ruleAppType,
        string? ruleUseCase)
    {
        var resolved = PriorityResolver.Resolve(Rules(Keys.PriorityAllLevels), Selection(useCase, appType), Values);

        ShouldMatch(resolved, Keys.PriorityAllLevels, level, weight, new MatchedRule(Kind(kind), ruleAppType, ruleUseCase));
    }

    [Theory(DisplayName = "INV:CAT-14 " + nameof(Resolve_SeveralAppTypes_StrictestResultWins))]
    [Trait("Invariant", "CAT-14")]
    [MemberData(nameof(SeveralAppTypes))]
    public void Resolve_SeveralAppTypes_StrictestResultWins(
        string key,
        string? useCase,
        string[] appTypes,
        PriorityLevel level,
        int? weight,
        string kind,
        string? ruleAppType,
        string? ruleUseCase)
    {
        var resolved = PriorityResolver.Resolve(Rules(key), Selection(useCase, appTypes), Values);

        ShouldMatch(resolved, key, level, weight, new MatchedRule(Kind(kind), ruleAppType, ruleUseCase));
    }

    [Fact(DisplayName = "INV:CAT-14 " + nameof(Resolve_EqualLevel_ARuleWithWeightBeatsOneWithout))]
    [Trait("Invariant", "CAT-14")]
    public void Resolve_EqualLevel_ARuleWithWeightBeatsOneWithout()
    {
        // Web sorts first, but its rule has no weight.
        var rules = CriterionRules.From(ARevision().WithKey("T-WEIGHT")
            .AppliesTo(V.Web).AppliesTo(V.SoftwareOpen)
            .WithPriority(V.Web, null, PriorityLevel.B)
            .WithPriority(V.SoftwareOpen, null, PriorityLevel.B, weight: 1)
            .WithDefaultPriority(PriorityLevel.Info)
            .Build());

        var resolved = PriorityResolver.Resolve(rules, Selection(null, "web", "softwareOpen"), Values);

        resolved.ShouldNotBeNull().MatchedRule.ShouldBe(new MatchedRule(MatchedRuleKind.AppType, "softwareOpen", null));
        resolved.Weight.ShouldBe(1);
    }

    [Fact(DisplayName = "INV:CAT-14 " + nameof(Resolve_SelectedAppTypeTheCriterionDoesNotApplyTo_DoesNotCount))]
    [Trait("Invariant", "CAT-14")]
    public void Resolve_SelectedAppTypeTheCriterionDoesNotApplyTo_DoesNotCount()
    {
        // The rule for Software offen would win, but the criterion applies only to Web.
        var rules = CriterionRules.From(ARevision().WithKey("T-APPLIES")
            .AppliesTo(V.Web)
            .WithPriority(V.SoftwareOpen, null, PriorityLevel.A)
            .WithDefaultPriority(PriorityLevel.Info)
            .Build());

        var resolved = PriorityResolver.Resolve(rules, Selection(null, "web", "softwareOpen"), Values);

        resolved.ShouldNotBeNull().Level.ShouldBe(PriorityLevel.Info);
        resolved.MatchedRule.Kind.ShouldBe(MatchedRuleKind.Default);
    }

    [Fact(DisplayName = "INV:CAT-14 " + nameof(Resolve_AppTypeThatAppliesOnlyWithAPart_CountsOnlyWithThatPartSelected))]
    [Trait("Invariant", "CAT-14")]
    public void Resolve_AppTypeThatAppliesOnlyWithAPart_CountsOnlyWithThatPartSelected()
    {
        var rules = CriterionRules.From(ARevision().WithKey("T-PART")
            .AppliesTo(V.Web, V.Audio).AppliesTo(V.SoftwareOpen)
            .WithPriority(V.Web, null, PriorityLevel.A)
            .WithPriority(V.SoftwareOpen, null, PriorityLevel.Info)
            .WithDefaultPriority(PriorityLevel.B)
            .Build());

        var withoutAudio = PriorityResolver.Resolve(rules, Selection(null, ["web", "softwareOpen"], ["base"]), Values);
        var withAudio = PriorityResolver.Resolve(rules, Selection(null, ["web", "softwareOpen"], ["base", "audio"]), Values);

        withoutAudio.ShouldNotBeNull().MatchedRule.ShouldBe(new MatchedRule(MatchedRuleKind.AppType, "softwareOpen", null));
        withAudio.ShouldNotBeNull().MatchedRule.ShouldBe(new MatchedRule(MatchedRuleKind.AppType, "web", null));
    }

    [Theory(DisplayName = "INV:CAT-01 " + nameof(Resolve_ProcedureCriterion_UsesOnlyTheRulesWithoutAppType))]
    [Trait("Invariant", "CAT-01")]
    [InlineData(null)]
    [InlineData("hr")]
    public void Resolve_ProcedureCriterion_UsesOnlyTheRulesWithoutAppType(string? useCase)
    {
        // A procedure criterion has no applicability (C30): no app type takes part, only rules 3 and 4.
        var resolved = PriorityResolver.Resolve(Rules(Keys.ProcedureSoftwareTest), Selection(useCase, "web"), Values);

        resolved.ShouldNotBeNull().Level.ShouldBe(PriorityLevel.B);
        resolved.MatchedRule.ShouldBe(new MatchedRule(MatchedRuleKind.Default, null, null));
    }

    [Fact]
    public void Resolve_AnAppTypeWithoutAnyMatchingRule_ReturnsNull()
    {
        // No catalog default: Software offen finds no rule, so the release is incomplete (S3-20 decision: 404).
        var rules = CriterionRules.From(ARevision().WithKey("T-INCOMPLETE")
            .AppliesTo(V.Web).AppliesTo(V.SoftwareOpen)
            .WithPriority(V.Web, null, PriorityLevel.A)
            .Build());

        PriorityResolver.Resolve(rules, Selection(null, "web"), Values).ShouldNotBeNull();
        PriorityResolver.Resolve(rules, Selection(null, "softwareOpen"), Values).ShouldBeNull();
        PriorityResolver.Resolve(rules, Selection(null, "web", "softwareOpen"), Values).ShouldBeNull();
    }

    [Fact]
    public void Resolve_RequirementCriterionThatDoesNotApply_Throws()
    {
        // 99.4 applies only with Audio; the criteria list never asks for it without.
        Should.Throw<ArgumentException>(() => PriorityResolver.Resolve(Rules(Keys.AudioOnly), Selection(null, "web"), Values));
    }

    /// <summary>
    /// Every criterion of both releases, under selections that reach every level of the chain:
    /// the resolved priority names the rule it comes from, with that rule's rationale and source.
    /// </summary>
    public static TheoryData<string, string, string?, string[]> EveryCriterionAndSelection()
    {
        var data = new TheoryData<string, string, string?, string[]>();
        (string? UseCase, string[] AppTypes)[] selections =
        [
            (null, ["web"]),
            (null, ["web", "softwareOpen", "mobileOpen"]),
            ("hr", ["web", "softwareOpen", "mobileOpen"]),
            ("hr", ["mobileOpen"]),
            ("databaseApplication", ["softwareOpen", "mobileOpen"]),
        ];
        foreach (var release in new[] { Catalog.T1, Catalog.T2 })
        {
            foreach (var key in release.Criteria.Select(c => c.Revision.Criterion.Key).Order(StringComparer.Ordinal))
            {
                foreach (var (useCase, appTypes) in selections)
                {
                    data.Add(release.Version, key, useCase, appTypes);
                }
            }
        }

        return data;
    }

    [Theory(DisplayName = "INV:CAT-02 " + nameof(Resolve_EveryCriterion_ExplainsItself))]
    [Trait("Invariant", "CAT-02")]
    [MemberData(nameof(EveryCriterionAndSelection))]
    public void Resolve_EveryCriterion_ExplainsItself(string version, string key, string? useCase, string[] appTypes)
    {
        var release = version == Catalog.T1.Version ? Catalog.T1 : Catalog.T2;
        var rules = CriterionRules.From(MiniCatalog.RevisionOf(release, key));
        var selection = Selection(useCase, appTypes, ["base", "audio", "video"]);
        if (rules.Kind == CriterionKind.Requirement && !rules.AppliesTo(selection))
        {
            return;
        }

        var resolved = PriorityResolver.Resolve(rules, selection, Values);

        resolved.ShouldNotBeNull();
        resolved.RationaleDe.ShouldNotBeNullOrWhiteSpace();
        resolved.Source.ShouldBe(PrioritySource.ExpertJudgement);
        resolved.MatchedRule.ShouldNotBeNull();
        var rule = rules.Priorities.Single(r => r.AppType == resolved.MatchedRule.AppType && r.UseCase == resolved.MatchedRule.UseCase);
        (rule.Level, rule.Weight, rule.RationaleDe).ShouldBe((resolved.Level, resolved.Weight, resolved.RationaleDe));
        resolved.MatchedRule.Kind.ShouldBe(KindOf(rule));
    }

    private static CriterionRules Rules(string key) => CriterionRules.From(MiniCatalog.RevisionOf(Catalog.T1, key));

    private static CriteriaSelection Selection(string? useCase, params string[] appTypes) => Selection(useCase, appTypes, ["base"]);

    private static CriteriaSelection Selection(string? useCase, string[] appTypes, string[] productParts) =>
        new(useCase, appTypes, productParts, []);

    /// <summary>The kind by its name in SPEC §8.2 (<c>matchedRule.kind</c>).</summary>
    private static MatchedRuleKind Kind(string name) => Enum.Parse<MatchedRuleKind>(name, ignoreCase: true);

    private static MatchedRuleKind KindOf(PriorityRule rule) => (rule.AppType, rule.UseCase) switch
    {
        (not null, not null) => MatchedRuleKind.AppTypeAndUseCase,
        (not null, null) => MatchedRuleKind.AppType,
        (null, not null) => MatchedRuleKind.UseCase,
        _ => MatchedRuleKind.Default,
    };

    private static void ShouldMatch(ResolvedPriority? resolved, string key, PriorityLevel level, int? weight, MatchedRule matchedRule)
    {
        resolved.ShouldNotBeNull();
        resolved.Level.ShouldBe(level);
        resolved.Weight.ShouldBe(weight);
        resolved.MatchedRule.ShouldBe(matchedRule);

        // The rationale is the one of the rule that decided (CAT-02).
        var rule = Rules(key).Priorities.Single(r => r.AppType == matchedRule.AppType && r.UseCase == matchedRule.UseCase);
        resolved.RationaleDe.ShouldBe(rule.RationaleDe);
        resolved.Source.ShouldBe(PrioritySource.ExpertJudgement);
    }
}
