// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MODELS.ProcureAccess.Entities;
using MODELS.ProcureAccess.Entities.Dto;
using MODELS.ProcureAccess.Entities.Mapping;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Mapping;

/// <summary>
/// Temporary (S3-07 run 2): the hand-written maps give the same JSON as the AutoMapper profile, on
/// fully filled sources and on the edge cases the legacy code meets. Deleted with the package.
/// </summary>
public class AutoMapperEquivalenceTests
{
    private static readonly IMapper AutoMapper =
        new MapperConfiguration(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly)).CreateMapper();

    private static readonly JsonSerializerOptions Json = new() { ReferenceHandler = ReferenceHandler.IgnoreCycles, WriteIndented = true };

    [Theory]
    [MemberData(nameof(MapCases.Theory), MemberType = typeof(MapCases))]
    public void Map_FilledSource_SameJsonAsAutoMapper(MapCase map)
    {
        var source = typeof(SampleObjects).GetMethod(nameof(SampleObjects.Filled))!.MakeGenericMethod(map.Source).Invoke(null, null)!;

        Serialize(map.Map(source)).ShouldBe(Serialize(ViaAutoMapper(map, source)));
    }

    public static TheoryData<string, object> EdgeCases() => new()
    {
        { "Product → ProductDto", new Product("p") { ProductCriteriaFilters = { new ProductCriteriaFilter { CriteriaFilterId = 3 } } } },
        { "Product → ProductDto", new Product("p") },
        { "Product → ProductDto (query)", new Product("p") },
        { "CriteriaFilter → CriteriaFilterDto", new CriteriaFilter("f", 1, "d") { Exclusions = { new CriteriaFilterExclusion { ExclusionId = 2 } } } },
        { "Proposal → ProposalDto", new Proposal { ProposerId = "u" } },
        { "User → UserDto", new User() },
        { "UICustomization → UICustomizationDto", new UICustomization() },
        { "CreateProductDto → Product", new CreateProductDto { Name = "p" } },
        { "CreateCriterionDto → Criterion", new CreateCriterionDto() },
    };

    [Theory]
    [MemberData(nameof(EdgeCases))]
    public void Map_EdgeCase_SameJsonAsAutoMapper(string name, object source)
    {
        var map = MapCases.All.Single(map => map.Name == name);

        Serialize(map.Map(source)).ShouldBe(Serialize(ViaAutoMapper(map, source)));
    }

    [Theory]
    [InlineData("en")]
    [InlineData("FR")]
    [InlineData("xx")]
    [InlineData(null)]
    public void LanguageCode_ToEntity_SameAsAutoMapper(string? code)
    {
        var viaAutoMapper = AutoMapper.Map<UICustomization>(new UICustomizationDto { Language = code }).Language;

        code.ToLanguage().ShouldBe(viaAutoMapper);
    }

    private static object ViaAutoMapper(MapCase map, object source)
    {
        if (!map.Name.EndsWith("(query)", StringComparison.Ordinal))
        {
            return AutoMapper.Map(source, map.Source, map.Target);
        }

        var array = Array.CreateInstance(map.Source, 1);
        array.SetValue(source, 0);
        var projected = Queryable.AsQueryable(array).ProjectTo(map.Target, AutoMapper.ConfigurationProvider);
        return projected.Cast<object>().Single();
    }

    private static string Serialize(object value) => JsonSerializer.Serialize(value, value.GetType(), Json);
}
