// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProcureAccess.Modules.Catalog.Persistence;

namespace ProcureAccess.Modules.Catalog.Reading;

/// <summary>
/// Read side of the catalog (ARCHITECTURE §9.6, CAT-09): the active release and the content of a
/// release, cached in <see cref="IMemoryCache"/> by <c>CatalogReleaseId</c>. The catalog endpoints
/// (S3-19) read through it.
/// </summary>
/// <remarks>
/// <para><b>No invalidation (owner, S3-17 decision 2).</b> A stored release never changes (CAT-05),
/// so its content is cached without expiry. Which release is active is not cached: it is read from
/// the database on every call, because <c>catalog activate</c> runs in another process and cannot
/// clear this cache. The next call after an activation returns the new release.</para>
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

    /// <summary>The content of the stored release <paramref name="releaseId"/>, from the cache after the first call.</summary>
    public async Task<CatalogReleaseSnapshot> GetReleaseAsync(int releaseId, CancellationToken cancellationToken)
    {
        var snapshot = await cache.GetOrCreateAsync(
            new CacheKey(releaseId),
            _ => LoadAsync(releaseId, cancellationToken));
        return snapshot!;
    }

    private async Task<CatalogReleaseSnapshot> LoadAsync(int releaseId, CancellationToken cancellationToken)
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

        var keys = release.Criteria
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Key, StringComparer.Ordinal)
            .Select(c => c.Key)
            .ToList();
        return new CatalogReleaseSnapshot(release.Id, release.Version, release.PublishedAt, keys);
    }

    /// <summary>Own key type, so no other cache entry of the API can collide with a release id.</summary>
    private sealed record CacheKey(int CatalogReleaseId);
}

/// <summary>
/// What the cache holds for one release. So far its identity and its criteria in list order; the
/// catalog endpoints (S3-19) add what they return.
/// </summary>
internal sealed record CatalogReleaseSnapshot(
    int Id,
    string Version,
    DateTimeOffset PublishedAt,
    IReadOnlyList<string> CriterionKeys);
