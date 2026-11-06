// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Persistence;

/// <summary>
/// Refuses every change and every delete of a stored catalog row (CAT-05, import part; owner,
/// S3-16 decision 6). The entities already refuse changes through their methods; this guard also
/// catches what EF allows around them: <c>Entry(…).Property(…).CurrentValue</c> and <c>Remove</c>.
/// </summary>
/// <remarks>
/// Adding rows stays allowed: that is how <c>catalog import</c> stores a release. Two changes are
/// allowed: <see cref="CatalogRelease.IsActive"/> (<c>catalog activate</c>, S3-17) and
/// <see cref="Criterion.SupersededByKey"/> while it is still empty
/// (<see cref="Criterion.MarkSupersededBy"/>, S3-16 decision 4).
/// </remarks>
internal sealed class CatalogImmutabilityInterceptor : SaveChangesInterceptor
{
    public static readonly CatalogImmutabilityInterceptor Instance = new();

    private CatalogImmutabilityInterceptor()
    {
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ThrowOnForbiddenChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ThrowOnForbiddenChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ThrowOnForbiddenChanges(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        context.ChangeTracker.DetectChanges();
        foreach (var entry in context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                    throw new PublishedCatalogChangeException(
                        $"Catalog rows are never deleted ({entry.Metadata.ClrType.Name}).");
                case EntityState.Modified when !IsAllowedChange(entry):
                    var properties = string.Join(", ", entry.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name));
                    throw new PublishedCatalogChangeException(
                        $"Stored catalog rows cannot be changed ({entry.Metadata.ClrType.Name}: {properties}).");
            }
        }
    }

    private static bool IsAllowedChange(EntityEntry entry)
    {
        var modified = entry.Properties.Where(p => p.IsModified).ToList();
        return entry.Entity switch
        {
            CatalogRelease => modified.All(p => p.Metadata.Name == nameof(CatalogRelease.IsActive)),
            Criterion => modified.All(p =>
                p.Metadata.Name == nameof(Criterion.SupersededByKey) && p.OriginalValue is null),
            _ => false,
        };
    }
}
