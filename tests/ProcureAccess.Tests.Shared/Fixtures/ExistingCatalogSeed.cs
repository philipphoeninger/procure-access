// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore.Metadata;
using MODELS.ProcureAccess.Entities;

namespace ProcureAccess.Tests.Shared.Fixtures;

/// <summary>
/// The criteria catalog of the existing data model as migration <c>AddSeedData</c> inserts it:
/// filter types, criteria filters and criteria, with their fixed ids. The characterisation test
/// CAT-10 and the legacy catalog read tests run on it. Removed with the legacy filter.
/// </summary>
/// <remarks>
/// The rows are read from the seed data (<c>HasData</c>) of the legacy context's design-time
/// model, the same source the migrations are generated from, and every value of a row is copied
/// into a new entity (#92). So a column seeded later is carried over without a change here, and a
/// seeded value this copy cannot carry (a shadow property, a filled navigation) fails instead of
/// being dropped. New instances are needed because the seed arrays are the static instances of
/// the EF model; adding those to a context would let EF fix up their navigations (S1-15 F5). As in
/// the migration, <c>CreatedAt</c> is not seeded, so the database sets it.
/// Exclusions, products and product links are left out: the criteria filter does not read them.
/// </remarks>
/// <param name="designTimeModel">
/// The design-time model of the legacy context (<c>IDesignTimeModel</c>); the runtime model
/// carries no seed data.
/// </param>
public sealed class ExistingCatalogSeed(IModel designTimeModel)
{
    /// <summary>All rows in insert order (filter types, criteria filters, criteria).</summary>
    public object[] All() => [.. FilterTypes(), .. CriteriaFilters(), .. Criteria()];

    public IEnumerable<FilterType> FilterTypes() => Rows<FilterType>();

    public IEnumerable<CriteriaFilter> CriteriaFilters() => Rows<CriteriaFilter>();

    public IEnumerable<Criterion> Criteria() => Rows<Criterion>();

    private IEnumerable<T> Rows<T>()
        where T : new()
    {
        var entityType = designTimeModel.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException($"The model does not map {typeof(T).Name}.");
        var seed = entityType.GetSeedData().ToList();
        if (seed.Count == 0)
        {
            throw new InvalidOperationException(
                $"{typeof(T).Name} has no seed data: pass the design-time model, not the runtime model.");
        }

        return seed.Select(row => Copy<T>(entityType, row)).ToList();
    }

    private static T Copy<T>(IEntityType entityType, IDictionary<string, object?> row)
        where T : new()
    {
        var entity = new T();
        foreach (var (name, value) in row)
        {
            // Seeded from entity instances, a row also lists their navigations, empty here.
            if (entityType.FindNavigation(name) is not null && IsEmpty(value))
            {
                continue;
            }

            var property = entityType.FindProperty(name)?.PropertyInfo
                ?? throw new InvalidOperationException(
                    $"The seed of {entityType.ClrType.Name} sets '{name}', which this copy cannot carry over.");
            property.SetValue(entity, value);
        }

        return entity;
    }

    private static bool IsEmpty(object? navigation) =>
        navigation is null || (navigation is System.Collections.IEnumerable items && !items.GetEnumerator().MoveNext());
}
