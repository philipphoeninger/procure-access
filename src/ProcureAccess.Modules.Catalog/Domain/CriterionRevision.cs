// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// Content of a criterion as one or more releases contain it (ARCHITECTURE §5.1). A revision is not
/// owned by a release: unchanged revisions are shared by the releases that contain them (CAT-07).
/// </summary>
/// <remarks>
/// <para><b>Immutability (CAT-05).</b> A revision can be changed only while it is a draft: created
/// with <see cref="Create"/> and not yet part of a published release. Publishing a release that
/// includes it seals it (<see cref="CatalogRelease.Publish"/>). A revision that EF loads from the
/// database is always sealed: catalog content reaches the database only through a published
/// release (owner, S3-12 decision 1). A change to a sealed revision throws
/// <see cref="PublishedCatalogChangeException"/>.</para>
/// <para>The revision owns its applicability (<see cref="AddApplicability"/>), priority rules
/// (<see cref="AddPriority"/>) and procedure texts (<see cref="AddProcedureText"/>); they are added
/// under the same rule and have no method that changes them.</para>
/// </remarks>
internal sealed class CriterionRevision : CatalogEntity
{
    private readonly List<StandardReference> _standardReferences = [];
    private readonly List<CriterionApplicability> _applicability = [];
    private readonly List<PriorityRecommendation> _priorities = [];
    private readonly List<CriterionProcedureText> _procedureTexts = [];

    /// <summary>Not mapped: false for every instance EF creates, so a loaded revision is sealed.</summary>
    private bool _isDraft;

    private CriterionRevision()
    {
    }

    public int CriterionId { get; private set; }

    public Criterion Criterion { get; private set; } = null!;

    /// <summary>1, 2, … per criterion.</summary>
    public int RevisionNumber { get; private set; }

    public string TitleDe { get; private set; } = null!;

    /// <summary>Plain-language title (K-1); empty until the project team writes it (O-1).</summary>
    public string? PlainLanguageDe { get; private set; }

    /// <summary>
    /// Short description (Kurzbeschreibung, Kriterienkatalog §6.1): one or two plain sentences of the
    /// project team, no Markdown; empty until the project team writes it (K-1, ADR-016).
    /// </summary>
    public string? ShortDescriptionDe { get; private set; }

    public string? RationaleDe { get; private set; }

    public int SortOrder { get; private set; }

    public IReadOnlyCollection<StandardReference> StandardReferences => _standardReferences;

    public IReadOnlyCollection<CriterionApplicability> Applicability => _applicability;

    public IReadOnlyCollection<PriorityRecommendation> Priorities => _priorities;

    public IReadOnlyCollection<CriterionProcedureText> ProcedureTexts => _procedureTexts;

    /// <summary>True once a published release contains the revision, and for every loaded revision.</summary>
    public bool IsSealed => !_isDraft;

    public static CriterionRevision Create(
        Criterion criterion,
        int revisionNumber,
        string titleDe,
        string? plainLanguageDe = null,
        string? rationaleDe = null,
        int sortOrder = 0,
        string? shortDescriptionDe = null)
    {
        ArgumentNullException.ThrowIfNull(criterion);
        ArgumentOutOfRangeException.ThrowIfLessThan(revisionNumber, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(titleDe);
        return new CriterionRevision
        {
            Criterion = criterion,
            RevisionNumber = revisionNumber,
            TitleDe = titleDe,
            PlainLanguageDe = plainLanguageDe,
            ShortDescriptionDe = shortDescriptionDe,
            RationaleDe = rationaleDe,
            SortOrder = sortOrder,
            _isDraft = true,
        };
    }

    public void AddStandardReference(StandardReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ThrowIfSealed();
        if (_standardReferences.Contains(reference))
        {
            throw new ArgumentException("The revision already cites this standard reference.", nameof(reference));
        }

        _standardReferences.Add(reference);
    }

    /// <summary>
    /// States that the criterion applies to <paramref name="appType"/>, optionally only for
    /// <paramref name="productPart"/>. Not for a procedure criterion, which enters a selection
    /// only through its test type (ARCHITECTURE §5.1, C30).
    /// </summary>
    public void AddApplicability(AppType appType, ProductPart? productPart = null)
    {
        ArgumentNullException.ThrowIfNull(appType);
        ThrowIfSealed();
        ThrowIfProcedureCriterion("applicability");
        if (_applicability.Any(a => a.AppType.Key == appType.Key && a.ProductPart?.Key == productPart?.Key))
        {
            throw new ArgumentException(
                $"The revision already applies to app type '{appType.Key}' and product part '{productPart?.Key}'.",
                nameof(productPart));
        }

        _applicability.Add(new CriterionApplicability(this, appType, productPart));
    }

    /// <summary>
    /// Adds a priority rule. <paramref name="appType"/> and <paramref name="useCase"/> select the
    /// level of the rule (both null = catalog default); a revision has at most one rule per pair.
    /// </summary>
    public void AddPriority(
        AppType? appType,
        UseCaseTemplate? useCase,
        PriorityLevel level,
        string rationaleDe,
        int? weight = null,
        PrioritySource source = PrioritySource.ExpertJudgement)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rationaleDe);
        if (!Enum.IsDefined(level))
        {
            throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown priority level.");
        }

        if (!Enum.IsDefined(source))
        {
            throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown priority source.");
        }

        ThrowIfSealed();
        if (_priorities.Any(p => p.AppType?.Key == appType?.Key && p.UseCase?.Key == useCase?.Key))
        {
            throw new ArgumentException(
                $"The revision already has a priority rule for app type '{appType?.Key}' and use case '{useCase?.Key}'.",
                nameof(useCase));
        }

        _priorities.Add(new PriorityRecommendation(this, appType, useCase, level, weight, rationaleDe, source));
    }

    /// <summary>
    /// Adds the text of <paramref name="testType"/> for this criterion; one per test type. Not for a
    /// procedure criterion, which has no BIT inklusiv text (ARCHITECTURE §5.1, C30).
    /// </summary>
    public void AddProcedureText(TestType testType, ProcedureTextContent content)
    {
        ArgumentNullException.ThrowIfNull(testType);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(content.OriginalNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(content.OriginalTitleDe);
        ArgumentException.ThrowIfNullOrWhiteSpace(content.SourceUrl);
        if (content.AllowedRatings == Rating.None || (content.AllowedRatings & ~Rating.All) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(content), content.AllowedRatings, "Allowed ratings must name at least one of the six levels and nothing else.");
        }

        ThrowIfSealed();
        ThrowIfProcedureCriterion("procedure texts");
        if (_procedureTexts.Any(t => t.TestType.Key == testType.Key))
        {
            throw new ArgumentException($"The revision already has a text for test type '{testType.Key}'.", nameof(testType));
        }

        _procedureTexts.Add(new CriterionProcedureText(this, testType, content));
    }

    /// <summary>Called by <see cref="CatalogRelease.Publish"/> for every revision of the release.</summary>
    internal void Seal() => _isDraft = false;

    private void ThrowIfProcedureCriterion(string what)
    {
        if (Criterion.Kind == CriterionKind.TestProcedure)
        {
            throw new InvalidOperationException($"The procedure criterion '{Criterion.Key}' has no {what}.");
        }
    }

    private void ThrowIfSealed()
    {
        if (IsSealed)
        {
            throw new PublishedCatalogChangeException(
                $"Revision {RevisionNumber} of criterion '{Criterion?.Key}' belongs to a published release and cannot be changed.");
        }
    }
}
