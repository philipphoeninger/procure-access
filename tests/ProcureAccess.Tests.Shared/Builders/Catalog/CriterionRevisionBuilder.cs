// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Tests.Shared.Builders.Catalog;

/// <summary>
/// Builds a draft <see cref="CriterionRevision"/> with its standard references, applicability,
/// priority rules and procedure texts. Publishing the release that includes it seals it.
/// </summary>
/// <remarks>
/// Without <see cref="Of"/> the revision gets a new requirement criterion with the key of
/// <see cref="WithKey"/> (default <c>T-CRITERION</c>; <c>Criterion.Key</c> is unique, so a test that
/// stores two revisions gives each its own key). Rules are added in the order the methods are called, through the domain
/// methods, so a rule the domain refuses fails in <see cref="Build"/>.
/// </remarks>
internal sealed class CriterionRevisionBuilder : Builder<CriterionRevision>
{
    private readonly List<Action<CriterionRevision>> _rules = [];
    private Criterion? _criterion;
    private string _key = "T-CRITERION";
    private int _revisionNumber = 1;
    private string? _titleDe;
    private string? _plainLanguageDe;
    private string? _shortDescriptionDe;
    private string? _rationaleDe;
    private int _sortOrder;

    /// <summary>A revision of an existing criterion, e.g. the second revision of a changed criterion.</summary>
    public CriterionRevisionBuilder Of(Criterion criterion)
    {
        _criterion = criterion;
        return this;
    }

    /// <summary>The key of the new requirement criterion; ignored with <see cref="Of"/>.</summary>
    public CriterionRevisionBuilder WithKey(string key)
    {
        _key = key;
        return this;
    }

    public CriterionRevisionBuilder WithRevisionNumber(int revisionNumber)
    {
        _revisionNumber = revisionNumber;
        return this;
    }

    public CriterionRevisionBuilder WithTitle(string titleDe)
    {
        _titleDe = titleDe;
        return this;
    }

    public CriterionRevisionBuilder WithPlainLanguage(string plainLanguageDe)
    {
        _plainLanguageDe = plainLanguageDe;
        return this;
    }

    public CriterionRevisionBuilder WithShortDescription(string shortDescriptionDe)
    {
        _shortDescriptionDe = shortDescriptionDe;
        return this;
    }

    public CriterionRevisionBuilder WithRationale(string rationaleDe)
    {
        _rationaleDe = rationaleDe;
        return this;
    }

    public CriterionRevisionBuilder WithSortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
        return this;
    }

    public CriterionRevisionBuilder Citing(StandardReference reference)
    {
        _rules.Add(revision => revision.AddStandardReference(reference));
        return this;
    }

    /// <summary>The criterion applies to <paramref name="appType"/>, optionally only with <paramref name="productPart"/>.</summary>
    public CriterionRevisionBuilder AppliesTo(AppType appType, ProductPart? productPart = null)
    {
        _rules.Add(revision => revision.AddApplicability(appType, productPart));
        return this;
    }

    /// <summary>
    /// A priority rule; <c>null</c> for app type or use case gives the less specific levels
    /// (ARCHITECTURE §5.1: both set, app type only, use case only, neither = catalog default).
    /// </summary>
    public CriterionRevisionBuilder WithPriority(
        AppType? appType,
        UseCaseTemplate? useCase,
        PriorityLevel level,
        int? weight = null,
        string? rationaleDe = null)
    {
        _rules.Add(revision => revision.AddPriority(
            appType, useCase, level, rationaleDe ?? DefaultRationale(appType, useCase, level), weight));
        return this;
    }

    /// <summary>The catalog default (level 4 of the resolution: no app type, no use case).</summary>
    public CriterionRevisionBuilder WithDefaultPriority(PriorityLevel level, string? rationaleDe = null) =>
        WithPriority(null, null, level, rationaleDe: rationaleDe);

    /// <summary>A procedure text of <paramref name="testType"/> with made-up wording and the given allowed ratings.</summary>
    public CriterionRevisionBuilder WithProcedureText(TestType testType, Rating allowedRatings, string? originalNumber = null)
    {
        _rules.Add(revision =>
        {
            var number = originalNumber ?? $"{revision.Criterion.Key}-{testType.Key}";
            revision.AddProcedureText(
                testType,
                new ProcedureTextContent(
                    number,
                    $"Testtext {number}",
                    allowedRatings,
                    $"https://example.org/{testType.Key}#{number}",
                    RequirementDe: $"Anforderung zu {number}."));
        });
        return this;
    }

    public override CriterionRevision Build()
    {
        var criterion = _criterion ?? CatalogData.ACriterion().WithKey(_key).Build();
        var revision = CriterionRevision.Create(
            criterion,
            _revisionNumber,
            _titleDe ?? $"Titel {criterion.Key}",
            _plainLanguageDe,
            _rationaleDe,
            _sortOrder,
            _shortDescriptionDe);
        foreach (var rule in _rules)
        {
            rule(revision);
        }

        return revision;
    }

    private static string DefaultRationale(AppType? appType, UseCaseTemplate? useCase, PriorityLevel level) =>
        $"Testbegründung {level} für {appType?.Key ?? "jeden Anwendungstyp"} und {useCase?.Key ?? "jeden Verwendungszweck"}.";
}
