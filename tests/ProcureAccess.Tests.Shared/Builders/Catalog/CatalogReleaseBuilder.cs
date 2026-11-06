// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Tests.Shared.Builders.Catalog;

/// <summary>
/// Builds a <see cref="CatalogRelease"/> with its revisions, exclusion rules and procedure mapping.
/// Published at <see cref="CatalogData.DefaultPublishedAt"/> unless <see cref="Draft"/> is called,
/// because the import stores published releases only (ARCHITECTURE §5.1, C101).
/// </summary>
/// <remarks>
/// For an integration test, build the release from filter values loaded through the context that
/// stores it (<see cref="Fixtures.MiniCatalogValues.LoadAsync"/>): the values have fixed ids, and
/// values from elsewhere would be inserted again (S3-13 P5).
/// </remarks>
internal sealed class CatalogReleaseBuilder : Builder<CatalogRelease>
{
    private readonly List<CriterionRevision> _revisions = [];
    private readonly List<FilterExclusion> _exclusions = [];
    private readonly List<(AppType AppType, TestType TestType)> _procedures = [];

    // Not a MiniCatalog version: Version is unique, so both can be stored in one test.
    private string _version = "T.0";
    private string? _releaseNotes;
    private DateTimeOffset? _publishedAt = CatalogData.DefaultPublishedAt;
    private bool _isActive;

    public CatalogReleaseBuilder WithVersion(string version)
    {
        _version = version;
        return this;
    }

    public CatalogReleaseBuilder WithReleaseNotes(string releaseNotes)
    {
        _releaseNotes = releaseNotes;
        return this;
    }

    /// <summary>Includes the revisions; a revision of an earlier release is shared, not copied (CAT-07).</summary>
    public CatalogReleaseBuilder Including(params CriterionRevision[] revisions)
    {
        _revisions.AddRange(revisions);
        return this;
    }

    public CatalogReleaseBuilder WithExclusion(FilterExclusion exclusion)
    {
        _exclusions.Add(exclusion);
        return this;
    }

    public CatalogReleaseBuilder WithProcedure(AppType appType, TestType testType)
    {
        _procedures.Add((appType, testType));
        return this;
    }

    public CatalogReleaseBuilder PublishedAt(DateTimeOffset publishedAt)
    {
        _publishedAt = publishedAt;
        return this;
    }

    /// <summary>Leaves the release unpublished, for tests of the draft state; the database refuses it.</summary>
    public CatalogReleaseBuilder Draft()
    {
        _publishedAt = null;
        _isActive = false;
        return this;
    }

    /// <summary>Publishes and activates the release.</summary>
    public CatalogReleaseBuilder Active()
    {
        _publishedAt ??= CatalogData.DefaultPublishedAt;
        _isActive = true;
        return this;
    }

    public override CatalogRelease Build()
    {
        var release = CatalogRelease.Create(_version, _releaseNotes);
        foreach (var revision in _revisions)
        {
            release.Include(revision);
        }

        foreach (var exclusion in _exclusions)
        {
            release.AddExclusion(exclusion);
        }

        foreach (var (appType, testType) in _procedures)
        {
            release.MapProcedure(appType, testType);
        }

        if (_publishedAt is DateTimeOffset publishedAt)
        {
            release.Publish(publishedAt);
            if (_isActive)
            {
                release.Activate();
            }
        }

        return release;
    }
}
