// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence;

namespace ProcureAccess.Modules.Catalog.Reading;

/// <summary>
/// Read side of the catalog (ARCHITECTURE §9.6, CAT-09): the active release, a release by its
/// version and the content of a release, cached in <see cref="IMemoryCache"/> by
/// <c>CatalogReleaseId</c>, plus the filter values. The catalog endpoints read through it (S3-19).
/// </summary>
/// <remarks>
/// <para><b>No invalidation (owner, S3-17 decision 2).</b> A stored release never changes (CAT-05),
/// so its content is cached without expiry. Which release is active, and which id a version has,
/// is not cached: it is read from the database on every call, because <c>catalog activate</c> and
/// <c>catalog import</c> run in another process and cannot clear this cache. The next call after an
/// activation returns the new release.</para>
/// <para><b>Filter values</b> belong to no release (ARCHITECTURE C104): only a catalog migration
/// changes them, and migrations run when the API starts, so they are cached without expiry too.</para>
/// <para>The cache is per instance (D10), like the rest of the API's state.</para>
/// </remarks>
internal sealed class CatalogReader(CatalogDbContext db, IMemoryCache cache)
{
    /// <summary>The active release, or null while no release has been activated.</summary>
    public async Task<CatalogReleaseSnapshot?> GetActiveReleaseAsync(CancellationToken cancellationToken)
    {
        var activeId = await db.Releases.AsNoTracking()
            .Where(r => r.IsActive)
            .Select(r => (int?)r.Id)
            .SingleOrDefaultAsync(cancellationToken);
        return activeId is null ? null : await GetReleaseAsync(activeId.Value, cancellationToken);
    }

    /// <summary>The stored release <paramref name="version"/>, active or not; null if it is not stored.</summary>
    public async Task<CatalogReleaseSnapshot?> GetReleaseByVersionAsync(string version, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(version);
        var id = await db.Releases.AsNoTracking()
            .Where(r => r.Version == version)
            .Select(r => (int?)r.Id)
            .SingleOrDefaultAsync(cancellationToken);
        return id is null ? null : await GetReleaseAsync(id.Value, cancellationToken);
    }

    /// <summary>The content of the stored release <paramref name="releaseId"/>, from the cache after the first call.</summary>
    public async Task<CatalogReleaseSnapshot> GetReleaseAsync(int releaseId, CancellationToken cancellationToken)
    {
        var snapshot = await cache.GetOrCreateAsync(
            new ReleaseCacheKey(releaseId),
            _ => LoadReleaseAsync(releaseId, cancellationToken));
        return snapshot!;
    }

    /// <summary>Every filter value of the four layers, inactive use cases included (CAT-18).</summary>
    public async Task<CatalogFilterValues> GetFilterValuesAsync(CancellationToken cancellationToken)
    {
        var values = await cache.GetOrCreateAsync(
            FilterValuesCacheKey.Instance,
            _ => LoadFilterValuesAsync(cancellationToken));
        return values!;
    }

    private async Task<CatalogReleaseSnapshot> LoadReleaseAsync(int releaseId, CancellationToken cancellationToken)
    {
        var release = await db.Releases.AsNoTracking()
            .Where(r => r.Id == releaseId)
            .Select(r => new
            {
                r.Id,
                r.Version,
                PublishedAt = r.PublishedAt!.Value,
                Criteria = r.Criteria.Select(c => new { c.Revision.Criterion.Key, c.Revision.SortOrder }).ToList(),
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Catalog release {releaseId} is not stored.");

        // The rules the release applies (CAT-19), by the keys of source and target.
        var exclusions = await db.Releases.AsNoTracking()
            .Where(r => r.Id == releaseId)
            .SelectMany(r => r.Exclusions.Select(e => e.Exclusion))
            .Select(e => new
            {
                SourceUseCase = (string?)e.SourceUseCase!.Key,
                SourceAppType = (string?)e.SourceAppType!.Key,
                SourceProductPart = (string?)e.SourceProductPart!.Key,
                TargetAppType = (string?)e.TargetAppType!.Key,
                TargetProductPart = (string?)e.TargetProductPart!.Key,
                TargetTestType = (string?)e.TargetTestType!.Key,
            })
            .ToListAsync(cancellationToken);

        var keys = release.Criteria
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Key, StringComparer.Ordinal)
            .Select(c => c.Key)
            .ToList();
        var rules = exclusions
            .Select(e => new ExclusionRule(
                e.SourceUseCase is not null ? new FilterValueRef(FilterLayer.UseCase, e.SourceUseCase)
                    : e.SourceAppType is not null ? new FilterValueRef(FilterLayer.AppType, e.SourceAppType)
                    : new FilterValueRef(FilterLayer.ProductPart, e.SourceProductPart!),
                e.TargetAppType is not null ? new FilterValueRef(FilterLayer.AppType, e.TargetAppType)
                    : e.TargetProductPart is not null ? new FilterValueRef(FilterLayer.ProductPart, e.TargetProductPart)
                    : new FilterValueRef(FilterLayer.TestType, e.TargetTestType!)))
            .ToList();
        return new CatalogReleaseSnapshot(release.Id, release.Version, release.PublishedAt, keys, rules);
    }

    private async Task<CatalogFilterValues> LoadFilterValuesAsync(CancellationToken cancellationToken) =>
        new(
            await db.UseCaseTemplates.AsNoTracking().ToListAsync(cancellationToken),
            await db.AppTypes.AsNoTracking().ToListAsync(cancellationToken),
            await db.ProductParts.AsNoTracking().ToListAsync(cancellationToken),
            await db.TestTypes.AsNoTracking().ToListAsync(cancellationToken));

    /// <summary>Own key types, so no other cache entry of the API can collide with a release id.</summary>
    private sealed record ReleaseCacheKey(int CatalogReleaseId);

    private sealed record FilterValuesCacheKey
    {
        public static readonly FilterValuesCacheKey Instance = new();
    }
}

/// <summary>
/// What the cache holds for one release: its identity, its criteria in list order and the exclusion
/// rules it applies. S3-20 adds what the criteria endpoint returns.
/// </summary>
internal sealed record CatalogReleaseSnapshot(
    int Id,
    string Version,
    DateTimeOffset PublishedAt,
    IReadOnlyList<string> CriterionKeys,
    IReadOnlyList<ExclusionRule> Exclusions);

/// <summary>A filter value named by its layer and key.</summary>
internal sealed record FilterValueRef(FilterLayer Layer, string Key);

/// <summary>An exclusion rule of a release: <see cref="Source"/> makes <see cref="Target"/> unavailable.</summary>
internal sealed record ExclusionRule(FilterValueRef Source, FilterValueRef Target);

/// <summary>
/// The filter values of the four layers as loaded, without tracking. The entities only have init
/// setters, so the cached objects cannot be changed.
/// </summary>
internal sealed record CatalogFilterValues(
    IReadOnlyList<UseCaseTemplate> UseCases,
    IReadOnlyList<AppType> AppTypes,
    IReadOnlyList<ProductPart> ProductParts,
    IReadOnlyList<TestType> TestTypes);
