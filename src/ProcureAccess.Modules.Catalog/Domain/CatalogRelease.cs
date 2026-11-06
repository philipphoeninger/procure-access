// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// A catalog release: a set of criterion revisions (ARCHITECTURE §5.1, ADR-002). Configurations
/// and published entries are pinned to a release, so its content never changes once published.
/// </summary>
/// <remarks>
/// <para><b>Published (owner, S3-12 decision 1).</b> The import (S3-16) builds a release with
/// <see cref="Create"/> and <see cref="Include"/> and seals it with <see cref="Publish"/> before it
/// stores it; <c>catalog import</c> is the only way into the database, and <see cref="PublishedAt"/>
/// is the import time. The column is <c>NOT NULL</c>, so the database holds published releases
/// only, and a release EF loads is published.</para>
/// <para><b>Immutability (CAT-05).</b> Once published, <see cref="Include"/> and <see cref="Publish"/>
/// throw <see cref="PublishedCatalogChangeException"/>, and so does any change to its revisions.
/// Only <see cref="IsActive"/> still changes, through <see cref="Activate"/> and
/// <see cref="Deactivate"/> (<c>catalog activate</c>, S3-17).</para>
/// <para>The release also pins the exclusion rules it applies (<see cref="AddExclusion"/>, CAT-19)
/// and the app type → procedure mapping (<see cref="MapProcedure"/>, CAT-21); both follow the same
/// rule.</para>
/// </remarks>
internal sealed class CatalogRelease : CatalogEntity
{
    public const int VersionMaxLength = 32;

    private readonly List<CatalogReleaseCriterion> _criteria = [];
    private readonly List<CatalogReleaseFilterExclusion> _exclusions = [];
    private readonly List<CatalogReleaseAppTypeProcedure> _procedures = [];

    private CatalogRelease()
    {
    }

    /// <summary>Release version, e.g. <c>2026.1</c> (the folder name under <c>catalog/releases/</c>).</summary>
    public string Version { get; private set; } = null!;

    /// <summary>Null only while the release is being built in memory; never null in the database.</summary>
    public DateTimeOffset? PublishedAt { get; private set; }

    public bool IsActive { get; private set; }

    public string? ReleaseNotes { get; private set; }

    public IReadOnlyCollection<CatalogReleaseCriterion> Criteria => _criteria;

    public IReadOnlyCollection<CatalogReleaseFilterExclusion> Exclusions => _exclusions;

    public IReadOnlyCollection<CatalogReleaseAppTypeProcedure> Procedures => _procedures;

    public bool IsPublished => PublishedAt is not null;

    public static CatalogRelease Create(string version, string? releaseNotes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        return new CatalogRelease { Version = version, ReleaseNotes = releaseNotes };
    }

    /// <summary>
    /// Adds a revision: a new draft revision or an unchanged one of an earlier release, which both
    /// releases then share (CAT-07). A release holds one revision per criterion.
    /// </summary>
    public void Include(CriterionRevision revision)
    {
        ArgumentNullException.ThrowIfNull(revision);
        ThrowIfPublished();
        if (revision.Criterion is null)
        {
            throw new ArgumentException("Load the revision together with its criterion.", nameof(revision));
        }

        if (_criteria.Any(c => c.Revision.Criterion.Key == revision.Criterion.Key))
        {
            throw new ArgumentException(
                $"Release {Version} already contains a revision of criterion '{revision.Criterion.Key}'.", nameof(revision));
        }

        _criteria.Add(new CatalogReleaseCriterion(this, revision));
    }

    /// <summary>
    /// Adds an exclusion rule to the release: a new rule or one of an earlier release, which both
    /// releases then share (CAT-19).
    /// </summary>
    public void AddExclusion(FilterExclusion exclusion)
    {
        ArgumentNullException.ThrowIfNull(exclusion);
        ThrowIfPublished();
        if (_exclusions.Any(e => ReferenceEquals(e.Exclusion, exclusion)))
        {
            throw new ArgumentException($"Release {Version} already applies this exclusion.", nameof(exclusion));
        }

        _exclusions.Add(new CatalogReleaseFilterExclusion(this, exclusion));
    }

    /// <summary>
    /// States that the texts and allowed ratings of <paramref name="testType"/> apply to
    /// <paramref name="appType"/> in this release (CAT-21). An app type may map to several procedures.
    /// </summary>
    public void MapProcedure(AppType appType, TestType testType)
    {
        ArgumentNullException.ThrowIfNull(appType);
        ArgumentNullException.ThrowIfNull(testType);
        ThrowIfPublished();
        if (_procedures.Any(p => p.AppType.Key == appType.Key && p.TestType.Key == testType.Key))
        {
            throw new ArgumentException(
                $"Release {Version} already maps app type '{appType.Key}' to test type '{testType.Key}'.", nameof(testType));
        }

        _procedures.Add(new CatalogReleaseAppTypeProcedure(this, appType, testType));
    }

    /// <summary>Publishes the release and seals its revisions. After this its content never changes.</summary>
    public void Publish(DateTimeOffset publishedAt)
    {
        ThrowIfPublished();
        PublishedAt = publishedAt;
        foreach (var entry in _criteria)
        {
            entry.Revision.Seal();
        }
    }

    public void Activate()
    {
        ThrowIfNotPublished();
        IsActive = true;
    }

    public void Deactivate()
    {
        ThrowIfNotPublished();
        IsActive = false;
    }

    private void ThrowIfPublished()
    {
        if (IsPublished)
        {
            throw new PublishedCatalogChangeException($"Release {Version} is published and cannot be changed.");
        }
    }

    private void ThrowIfNotPublished()
    {
        if (!IsPublished)
        {
            throw new InvalidOperationException($"Release {Version} is not published yet.");
        }
    }
}
