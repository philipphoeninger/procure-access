// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;

namespace ProcureAccess.Modules.Configurator.Domain;

/// <summary>
/// A saved criteria list (ARCHITECTURE §5.3): owned by an organisation, pinned to one catalog
/// release (ADR-002), with the four selection layers and one <see cref="ConfigurationCriterion"/>
/// per criterion of the selection. The list is the aggregate: its rows are stored, read and changed
/// only through it, so the organisation filter on the list covers them (TEN-01).
/// </summary>
/// <remarks>
/// The release is stored by its version and the selection by value keys (owner decision S5-19 2a).
/// <c>ProcurementProjectId</c> (the project copy, C53) comes with migration step 5.
/// </remarks>
internal sealed class CriteriaConfiguration
{
    /// <summary>Longest name (SPEC §7.2, R-SAV-01), counted in UTF-16 code units like <see cref="string.Length"/>.</summary>
    public const int NameMaxLength = 120;

    /// <summary>Longest release version, as the catalog stores it.</summary>
    public const int ReleaseVersionMaxLength = 32;

    /// <summary>Longest key of a filter value, as the catalog stores it.</summary>
    public const int ValueKeyMaxLength = 64;

    private readonly List<ConfigurationAppType> _appTypes = [];
    private readonly List<ConfigurationProductPart> _productParts = [];
    private readonly List<ConfigurationTestType> _testTypes = [];
    private readonly List<ConfigurationCriterion> _criteria = [];

    private CriteriaConfiguration()
    {
    }

    /// <summary>Random, so the identifier says nothing about other lists or their order.</summary>
    public Guid Id { get; private set; }

    /// <summary>The owning organisation (IdentityAccess); no foreign key across contexts (ADR-014).</summary>
    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; } = null!;

    /// <summary>The version of the catalog release the list is pinned to.</summary>
    public string ReleaseVersion { get; private set; } = null!;

    /// <summary>The key of the use case; <c>null</c> for a selection without one.</summary>
    public string? UseCaseKey { get; private set; }

    /// <summary>The ASP.NET Core Identity user id of the creator; no foreign key while users are legacy (ADR-014).</summary>
    public string CreatedByUserId { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>When the list or one of its decisions last changed; the creation time until then.</summary>
    public DateTimeOffset ModifiedAt { get; private set; }

    public IReadOnlyList<ConfigurationAppType> AppTypes => _appTypes;

    public IReadOnlyList<ConfigurationProductPart> ProductParts => _productParts;

    public IReadOnlyList<ConfigurationTestType> TestTypes => _testTypes;

    public IReadOnlyList<ConfigurationCriterion> Criteria => _criteria;

    /// <summary>
    /// Creates a list from a selection the catalog has resolved and the user's decisions.
    /// <paramref name="createdAt"/> comes from <see cref="TimeProvider"/> (CLAUDE.md §7). Throws for
    /// an empty or too long name, release or key, a selection without app type, a value or a
    /// criterion named twice and an impossible decision. Whether the values and criteria belong to
    /// the release is the caller's check against the catalog.
    /// </summary>
    public static CriteriaConfiguration Create(
        Guid organizationId,
        string name,
        string createdByUserId,
        DateTimeOffset createdAt,
        string releaseVersion,
        string? useCaseKey,
        IReadOnlyCollection<string> appTypeKeys,
        IReadOnlyCollection<string> productPartKeys,
        IReadOnlyCollection<string> testTypeKeys,
        IReadOnlyCollection<ConfigurationCriterionDraft> criteria)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("A list belongs to an organisation.", nameof(organizationId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > NameMaxLength)
        {
            throw new ArgumentException($"A list name has at most {NameMaxLength} characters.", nameof(name));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(createdByUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseVersion);
        if (releaseVersion.Length > ReleaseVersionMaxLength)
        {
            throw new ArgumentException($"A release version has at most {ReleaseVersionMaxLength} characters.", nameof(releaseVersion));
        }

        if (useCaseKey is not null)
        {
            CheckKeys([useCaseKey], nameof(useCaseKey));
        }

        CheckKeys(appTypeKeys, nameof(appTypeKeys));
        CheckKeys(productPartKeys, nameof(productPartKeys));
        CheckKeys(testTypeKeys, nameof(testTypeKeys));
        if (appTypeKeys.Count == 0)
        {
            throw new ArgumentException("A selection has at least one app type.", nameof(appTypeKeys));
        }

        ArgumentNullException.ThrowIfNull(criteria);
        if (criteria.Select(c => c.CriterionRevisionId).Distinct().Count() != criteria.Count)
        {
            throw new ArgumentException("A criterion is named more than once.", nameof(criteria));
        }

        var configuration = new CriteriaConfiguration
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = name,
            ReleaseVersion = releaseVersion,
            UseCaseKey = useCaseKey,
            CreatedByUserId = createdByUserId,
            CreatedAt = createdAt,
            ModifiedAt = createdAt,
        };
        configuration._appTypes.AddRange(appTypeKeys.Select(key => new ConfigurationAppType(key)));
        configuration._productParts.AddRange(productPartKeys.Select(key => new ConfigurationProductPart(key)));
        configuration._testTypes.AddRange(testTypeKeys.Select(key => new ConfigurationTestType(key)));
        configuration._criteria.AddRange(criteria.Select(ConfigurationCriterion.Create));
        return configuration;
    }

    /// <summary>
    /// Stores the user's decision on one criterion of the list (R-LST-03 – R-LST-05) and stamps
    /// <see cref="ModifiedAt"/>. The recommended level of the criterion stays as it is (CAT-08).
    /// Throws for a criterion that is not on the list and for an impossible decision
    /// (<see cref="ConfigurationCriterion.Decide"/>).
    /// </summary>
    public void Decide(
        int criterionRevisionId,
        bool isIncluded,
        PriorityLevel? overrideLevel,
        int? overrideWeight,
        string? note,
        DateTimeOffset now)
    {
        var criterion = _criteria.SingleOrDefault(c => c.CriterionRevisionId == criterionRevisionId)
            ?? throw new ArgumentException($"Revision {criterionRevisionId} is not a criterion of this list.", nameof(criterionRevisionId));
        criterion.Decide(isIncluded, overrideLevel, overrideWeight, note);
        ModifiedAt = now;
    }

    private static void CheckKeys(IReadOnlyCollection<string> keys, string parameter)
    {
        ArgumentNullException.ThrowIfNull(keys, parameter);
        if (keys.Any(key => string.IsNullOrWhiteSpace(key) || key.Length > ValueKeyMaxLength))
        {
            throw new ArgumentException($"A value key is not empty and has at most {ValueKeyMaxLength} characters.", parameter);
        }

        if (keys.Distinct(StringComparer.Ordinal).Count() != keys.Count)
        {
            throw new ArgumentException("A value is named more than once.", parameter);
        }
    }
}
