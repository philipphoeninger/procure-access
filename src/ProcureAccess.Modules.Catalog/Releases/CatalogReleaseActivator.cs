// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Catalog.Persistence;

namespace ProcureAccess.Modules.Catalog.Releases;

/// <summary>
/// <c>catalog activate &lt;version&gt;</c> (SPEC R-ADM-01; CAT-23 activate part): makes an imported
/// release the active one, which the catalog reads from then on (CAT-09).
/// </summary>
/// <remarks>
/// <para><b>Complete releases only (O-S3-5 Q2 (iii); owner, S3-17 decision 1).</b> Every criterion,
/// procedure criteria included, has a catalog-default priority (a rule without app type and use
/// case), and every seeded test type has its procedure criterion (ARCHITECTURE §5.1, C30). A stored
/// release never changes, so an incomplete one is fixed in its folder and imported as a new
/// version.</para>
/// <para><b>One active release (decision 4).</b> The release active so far is deactivated in the
/// same transaction; a partial unique index on <c>IsActive</c> refuses a second active release.
/// An older release may be activated again (rollback). Activating the active release writes
/// nothing. There is no command that leaves no release active.</para>
/// </remarks>
internal sealed class CatalogReleaseActivator(CatalogDbContext db)
{
    public async Task<CatalogActivationResult> ActivateAsync(string version, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        var release = await db.Releases.AsNoTracking()
            .Where(r => r.Version == version)
            .Select(r => new { r.Id, r.IsActive })
            .SingleOrDefaultAsync(cancellationToken);
        if (release is null)
        {
            return new CatalogActivationResult(null, null, [$"Release {version} is not imported; run catalog import {version} first."]);
        }

        if (release.IsActive)
        {
            return new CatalogActivationResult(version, version, []);
        }

        var gaps = await FindGapsAsync(release.Id, cancellationToken);
        if (gaps.Count > 0)
        {
            return new CatalogActivationResult(null, null, gaps, IsIncomplete: true);
        }

        string? previous = null;
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(
            async ct =>
            {
                // A retry starts again from the database.
                db.ChangeTracker.Clear();
                await using var transaction = await db.Database.BeginTransactionAsync(ct);
                var current = await db.Releases.SingleOrDefaultAsync(r => r.IsActive, ct);
                var target = await db.Releases.SingleAsync(r => r.Id == release.Id, ct);

                // Two saves: the unique index on IsActive is checked per row, so the old release
                // must be inactive before the new one becomes active.
                if (current is not null)
                {
                    current.Deactivate();
                    await db.SaveChangesAsync(ct);
                }

                target.Activate();
                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                previous = current?.Version;
            },
            cancellationToken);

        return new CatalogActivationResult(version, previous, []);
    }

    /// <summary>What the release lacks to be activated, in a stable order (<see cref="ReleaseCompleteness"/>).</summary>
    private async Task<List<string>> FindGapsAsync(int releaseId, CancellationToken cancellationToken)
    {
        var criteria = await db.Releases.AsNoTracking()
            .Where(r => r.Id == releaseId)
            .SelectMany(r => r.Criteria)
            .Select(c => new CriterionCompletenessFacts(
                c.Revision.Criterion.Key,
                c.Revision.Criterion.Kind,
                c.Revision.Criterion.TestType == null ? null : c.Revision.Criterion.TestType.Key,
                c.Revision.Priorities.Any(p => p.AppTypeId == null && p.UseCaseTemplateId == null)))
            .ToListAsync(cancellationToken);
        var testTypeKeys = await db.TestTypes.AsNoTracking()
            .OrderBy(t => t.SortOrder)
            .Select(t => t.Key)
            .ToListAsync(cancellationToken);

        return ReleaseCompleteness.FindGaps(criteria, testTypeKeys);
    }
}

internal sealed record CatalogActivationResult(
    string? Active,
    string? Previous,
    IReadOnlyList<string> Errors,
    bool IsIncomplete = false)
{
    public bool Succeeded => Errors.Count == 0;

    public bool WasAlreadyActive => Succeeded && Active == Previous;
}
