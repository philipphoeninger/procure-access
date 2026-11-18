// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using MODELS.ProcureAccess.Entities;
using MODELS.ProcureAccess.Entities.Dto;
using MODELS.ProcureAccess.Entities.Mapping;
using Xunit;

namespace ProcureAccess.Tests.Unit.Mapping;

/// <summary>One map of the application: how to call it and which target members it leaves out on purpose.</summary>
public sealed record MapCase(string Name, Type Source, Type Target, Func<object, object> Map, string[] IgnoredTargetMembers)
{
    public override string ToString() => Name;
}

/// <summary>
/// Every map between an entity and a DTO (ADR-015). A new map is added here, so ARC-06 checks it.
/// </summary>
public static class MapCases
{
    public static readonly IReadOnlyList<MapCase> All =
    [
        Case<Criterion, CriterionDto>("Criterion → CriterionDto", criterion => criterion.ToDto()),
        Case<Criterion, CriterionDto>("Criterion → CriterionDto (query)", CatalogMappings.CriterionProjection.Compile()),
        Case<CriteriaFilter, CriteriaFilterDto>("CriteriaFilter → CriteriaFilterDto", filter => filter.ToDto()),
        Case<CriteriaFilter, CriteriaFilterDto>("CriteriaFilter → CriteriaFilterDto (query)", CatalogMappings.CriteriaFilterProjection.Compile()),
        Case<CriteriaFilter, LookupDto>("CriteriaFilter → LookupDto", filter => filter.ToLookupDto()),
        Case<FilterType, FilterTypeDto>("FilterType → FilterTypeDto", filterType => filterType.ToDto()),
        Case<Product, ProductDto>("Product → ProductDto", product => product.ToDto()),
        Case<Product, ProductDto>("Product → ProductDto (query)", CatalogMappings.ProductProjection.Compile()),
    ];

    public static TheoryData<MapCase> Theory() => [.. All];

    private static MapCase Case<TSource, TTarget>(string name, Func<TSource, TTarget> map, string[]? ignored = null)
        where TSource : notnull
        where TTarget : notnull =>
        new(name, typeof(TSource), typeof(TTarget), source => map((TSource)source), ignored ?? []);
}
