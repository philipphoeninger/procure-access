// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using MODELS.ProcureAccess.Entities;
using MODELS.ProcureAccess.SeedData;
using ProcureAccess.Tests.Shared.Builders;

namespace ProcureAccess.Tests.Shared.Fixtures;

/// <summary>
/// The criteria catalog of the existing data model as migration <c>AddSeedData</c> inserts it:
/// filter types, criteria filters and criteria, with their fixed ids. This is the data that
/// migration step 2 moves into catalog release 2026.1, so the characterisation test CAT-10 runs
/// on it. Removed when the catalog import replaces the seed (step 2).
/// </summary>
/// <remarks>
/// Each call returns new entity instances with scalar values only. The seed arrays are the
/// static instances that the EF model uses for <c>HasData</c>; adding those to a context would
/// let EF fix up their navigations. <c>CreatedAt</c> is the fixed <see cref="TestData.DefaultCreatedAt"/>.
/// Exclusions, products and product links are left out: the criteria filter does not read them.
/// </remarks>
public static class ExistingCatalogSeed
{
    /// <summary>All rows in insert order (filter types, criteria filters, criteria).</summary>
    public static object[] All() => [.. FilterTypes(), .. CriteriaFilters(), .. Criteria()];

    public static IEnumerable<FilterType> FilterTypes() =>
        FilterTypeSeed.Data.Select(seed => new FilterType(seed.Name, seed.Description)
        {
            Id = seed.Id,
            IsDeleted = seed.IsDeleted,
            CreatedAt = TestData.DefaultCreatedAt,
        });

    public static IEnumerable<CriteriaFilter> CriteriaFilters() =>
        CriteriaFilterSeed.Data.Select(seed => new CriteriaFilter(seed.Name, seed.FilterTypeId, seed.Description)
        {
            Id = seed.Id,
            IsDeleted = seed.IsDeleted,
            CreatedAt = TestData.DefaultCreatedAt,
        });

    public static IEnumerable<Criterion> Criteria() =>
        CriterionSeed.Data.Select(seed => new Criterion(seed.Name, seed.Description)
        {
            Id = seed.Id,
            CriteriaFilterId = seed.CriteriaFilterId,
            IsDeleted = seed.IsDeleted,
            CreatedAt = TestData.DefaultCreatedAt,
        });
}
