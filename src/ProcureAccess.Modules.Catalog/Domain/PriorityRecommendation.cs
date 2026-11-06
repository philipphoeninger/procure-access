// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// A priority rule of a criterion (ARCHITECTURE §5.1, „Priority resolution“). Which of the four
/// levels it is follows from what it names: app type and use case (1), app type (2), use case (3)
/// or neither, the catalog default (4). A revision holds at most one rule per level and value pair.
/// Created only by <see cref="CriterionRevision.AddPriority"/>; resolution comes with S3-20.
/// </summary>
/// <remarks>Every rule explains itself: <see cref="RationaleDe"/> is required (CAT-02).</remarks>
internal sealed class PriorityRecommendation : CatalogEntity
{
    public const int RationaleMaxLength = 2000;

    private PriorityRecommendation()
    {
    }

    internal PriorityRecommendation(
        CriterionRevision revision,
        AppType? appType,
        UseCaseTemplate? useCase,
        PriorityLevel level,
        int? weight,
        string rationaleDe,
        PrioritySource source)
    {
        Revision = revision;
        AppType = appType;
        UseCase = useCase;
        Level = level;
        Weight = weight;
        RationaleDe = rationaleDe;
        Source = source;
    }

    public int CriterionRevisionId { get; private set; }

    public CriterionRevision Revision { get; private set; } = null!;

    public int? AppTypeId { get; private set; }

    public AppType? AppType { get; private set; }

    public int? UseCaseTemplateId { get; private set; }

    public UseCaseTemplate? UseCase { get; private set; }

    public PriorityLevel Level { get; private set; }

    /// <summary>Optional; SPEC §8.2 returns <c>null</c> when the rule has none.</summary>
    public int? Weight { get; private set; }

    public string RationaleDe { get; private set; } = null!;

    public PrioritySource Source { get; private set; }
}
