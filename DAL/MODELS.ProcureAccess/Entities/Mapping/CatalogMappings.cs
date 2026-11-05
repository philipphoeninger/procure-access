// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Mapping;

/// <summary>
/// Hand-written maps of the legacy catalog entities (ADR-015, replaces AutoMapper, #31). Every
/// target member is set here or left out on purpose; ARC-06 checks that no target member stays
/// unset. The <c>*Projection</c> expressions are for EF queries (they replace <c>ProjectTo</c>); the
/// <c>ToDto</c> methods are for loaded entities. Retired with the catalog module (migration step 2).
/// </summary>
public static class CatalogMappings
{
    public static readonly Expression<Func<Criterion, CriterionDto>> CriterionProjection = criterion => new CriterionDto
    {
        Id = criterion.Id,
        Name = criterion.Name,
        Description = criterion.Description,
        CriteriaFilterId = criterion.CriteriaFilterId,
    };

    public static readonly Expression<Func<CriteriaFilter, CriteriaFilterDto>> CriteriaFilterProjection = filter => new CriteriaFilterDto
    {
        Id = filter.Id,
        Name = filter.Name,
        Description = filter.Description,
        FilterTypeId = filter.FilterTypeId,
        Exclusions = filter.Exclusions
            .Select(link => new LookupDto { Id = link.Exclusion.Id, Name = link.Exclusion.Name })
            .ToList(),
    };

    public static readonly Expression<Func<Product, ProductDto>> ProductProjection = product => new ProductDto
    {
        Id = product.Id,
        Name = product.Name,
        Link = product.Link,
        Description = product.Description,
        CriteriaFilters = product.ProductCriteriaFilters
            .Select(link => new LookupDto { Id = link.CriteriaFilter.Id, Name = link.CriteriaFilter.Name })
            .ToList(),
    };

    public static CriterionDto ToDto(this Criterion criterion) => new()
    {
        Id = criterion.Id,
        Name = criterion.Name,
        Description = criterion.Description,
        CriteriaFilterId = criterion.CriteriaFilterId,
    };

    public static CriteriaFilterDto ToDto(this CriteriaFilter filter) => new()
    {
        Id = filter.Id,
        Name = filter.Name,
        Description = filter.Description,
        FilterTypeId = filter.FilterTypeId,
        Exclusions = filter.Exclusions.Select(link => link.Exclusion.ToLookupDtoOrNull()).ToList()!,
    };

    public static LookupDto ToLookupDto(this CriteriaFilter filter) => new()
    {
        Id = filter.Id,
        Name = filter.Name,
    };

    public static FilterTypeDto ToDto(this FilterType filterType) => new()
    {
        Id = filterType.Id,
        Name = filterType.Name,
        Description = filterType.Description,
    };

    public static ProductDto ToDto(this Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Link = product.Link,
        Description = product.Description,
        CriteriaFilters = product.ProductCriteriaFilters.Select(link => link.CriteriaFilter.ToLookupDtoOrNull()).ToList()!,
    };

    /// <summary>A product from an approved proposal (<c>ProposalService</c>); the server sets key, audit fields and navigations.</summary>
    public static Product ToEntity(this CreateProductDto dto) => new()
    {
        Name = dto.Name,
        Link = dto.Link,
        Description = dto.Description,
        ProductCriteriaFilters = dto.CriteriaFilterIds
            .Select(id => new ProductCriteriaFilter { CriteriaFilterId = id })
            .ToList(),
    };

    /// <summary>A criterion from an approved proposal (<c>ProposalService</c>); the server sets key, audit fields and navigations.</summary>
    public static Criterion ToEntity(this CreateCriterionDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        CriteriaFilterId = dto.CriteriaFilterId,
    };

    /// <summary>
    /// A join row whose filter is not loaded gives a <c>null</c> element, as AutoMapper did: after an
    /// approval the new product's join rows carry only the filter id.
    /// </summary>
    private static LookupDto? ToLookupDtoOrNull(this CriteriaFilter? filter) => filter?.ToLookupDto();
}
