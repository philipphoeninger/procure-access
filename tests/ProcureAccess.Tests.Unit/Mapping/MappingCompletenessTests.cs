// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Reflection;
using MODELS.ProcureAccess.Entities.Mapping;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Mapping;

/// <summary>
/// ARC-06 (TESTING_v27 §4.10): every map between an entity and its DTO sets every member of its
/// target or names it as ignored in <see cref="MapCases"/>. Replaces AutoMapper's
/// <c>AssertConfigurationIsValid</c> (ADR-015, #31), which could not see a missing map (#90).
/// </summary>
public class MappingCompletenessTests
{
    [Theory(DisplayName = "INV:ARC-06 " + nameof(Map_SourceFullyFilled_SetsEveryTargetMemberNotIgnored))]
    [Trait("Invariant", "ARC-06")]
    [MemberData(nameof(MapCases.Theory), MemberType = typeof(MapCases))]
    public void Map_SourceFullyFilled_SetsEveryTargetMemberNotIgnored(MapCase map)
    {
        var source = Fill(map.Source);

        var target = map.Map(source);

        var unset = SampleObjects.SettableProperties(map.Target)
            .Where(property => !map.IgnoredTargetMembers.Contains(property.Name))
            .Where(property => SampleObjects.IsUnset(property.GetValue(target)))
            .Select(property => property.Name)
            .ToList();
        unset.ShouldBeEmpty($"{map.Name} leaves these target members unset; map them or add them to the ignore list in MapCases");
    }

    [Theory(DisplayName = "INV:ARC-06 " + nameof(Map_IgnoreList_NamesOnlyExistingTargetMembers))]
    [Trait("Invariant", "ARC-06")]
    [MemberData(nameof(MapCases.Theory), MemberType = typeof(MapCases))]
    public void Map_IgnoreList_NamesOnlyExistingTargetMembers(MapCase map)
    {
        var members = SampleObjects.SettableProperties(map.Target).Select(property => property.Name).ToHashSet();

        map.IgnoredTargetMembers.Where(name => !members.Contains(name)).ShouldBeEmpty();
    }

    [Fact(DisplayName = "INV:ARC-06 " + nameof(MapCases_AllMappingMethods_AreListed))]
    [Trait("Invariant", "ARC-06")]
    public void MapCases_AllMappingMethods_AreListed()
    {
        // Every public map method and query projection of the mapping classes has a case, so a new
        // map cannot skip the check above. ToLanguage/ToLanguageCode map values, not objects.
        var methods = new[] { typeof(CatalogMappings), typeof(AccountMappings) }
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(method => method.Name is not (nameof(AccountMappings.ToLanguage) or nameof(AccountMappings.ToLanguageCode)))
            .Select(method => $"{method.GetParameters()[0].ParameterType.Name} → {method.ReturnType.Name}");
        var projections = typeof(CatalogMappings).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => field.FieldType.GetGenericArguments()[0].GetGenericArguments())
            .Select(types => $"{types[0].Name} → {types[1].Name} (query)");
        var listed = MapCases.All.Select(map => map.Name).ToHashSet();

        methods.Concat(projections).Where(name => !listed.Contains(name)).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("en", MODELS.ProcureAccess.Entities.Language.English)]
    [InlineData("DE", MODELS.ProcureAccess.Entities.Language.German)]
    [InlineData("es", MODELS.ProcureAccess.Entities.Language.Spanish)]
    [InlineData("fr", MODELS.ProcureAccess.Entities.Language.French)]
    [InlineData("xx", MODELS.ProcureAccess.Entities.Language.German)]
    [InlineData(null, MODELS.ProcureAccess.Entities.Language.German)]
    public void LanguageCode_Mapped_GivesLanguageAndBack(string? code, MODELS.ProcureAccess.Entities.Language expected)
    {
        var language = code.ToLanguage();

        language.ShouldBe(expected);
        language.ToLanguageCode().ShouldBe(code is "en" or "es" or "fr" ? code : "de");
    }

    private static object Fill(Type type) =>
        typeof(SampleObjects).GetMethod(nameof(SampleObjects.Filled))!.MakeGenericMethod(type).Invoke(null, null)!;
}
