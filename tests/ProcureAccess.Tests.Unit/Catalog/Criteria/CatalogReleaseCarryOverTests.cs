// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Criteria;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Reading;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Catalog.Criteria;

/// <summary>
/// Which values of a saved selection the active release still allows (S5-21a; CAT-22): an inactive
/// use case and every value a kept higher-layer value excludes are dropped, top layer first.
/// </summary>
public class CatalogReleaseCarryOverTests
{
    private static readonly MiniCatalogValues V = MiniCatalogValues.InMemory();
    private static readonly MiniCatalog Catalog = MiniCatalog.Build(V);
    private static readonly CatalogFilterValues Values = new(
        [V.Hr, V.DatabaseApplication],
        [V.Web, V.SoftwareOpen, V.MobileOpen, V.Hardware],
        [V.Base, V.Audio, V.Video],
        V.TestTypes);

    [Fact(DisplayName = "INV:CAT-22 " + nameof(Trim_ValueTheNewReleaseExcludes_IsDroppedAndNamed))]
    [Trait("Invariant", "CAT-22")]
    public void Trim_ValueTheNewReleaseExcludes_IsDroppedAndNamed()
    {
        // T.1 allows audio under HR; T.2 excludes it.
        var (query, dropped) = CatalogReleaseCarryOver.Trim(Selection("hr", ["web"], ["base", "audio"], []), Values, Exclusions(Catalog.T2));

        query.ShouldSatisfyAllConditions(
            q => q.UseCase.ShouldBe("hr"),
            q => q.AppTypeIds.ShouldBe(["web"]),
            q => q.ProductPartIds.ShouldBe(["base"]),
            q => q.TestTypeIds.ShouldBeEmpty());
        dropped.Select(v => v.Key).ShouldBe(["audio"]);
    }

    [Fact(DisplayName = "INV:CAT-22 " + nameof(Trim_DroppedValue_NoLongerExcludesLowerLayers))]
    [Trait("Invariant", "CAT-22")]
    public void Trim_DroppedValue_NoLongerExcludesLowerLayers()
    {
        // The inactive use case goes first (CAT-18); its exclusion of the app test then no longer applies.
        var (query, dropped) = CatalogReleaseCarryOver.Trim(
            Selection("databaseApplication", ["softwareOpen"], ["base"], ["appTest"]), Values, Exclusions(Catalog.T2));

        query.UseCase.ShouldBeNull();
        query.TestTypeIds.ShouldBe(["appTest"]);
        dropped.Select(v => v.Key).ShouldBe(["databaseApplication"]);
    }

    [Fact(DisplayName = "INV:CAT-22 " + nameof(Trim_ExclusionsDownTheLayers_DropInLayerOrder))]
    [Trait("Invariant", "CAT-22")]
    public void Trim_ExclusionsDownTheLayers_DropInLayerOrder()
    {
        // Web excludes the app test; mobile excludes audio, and audio would exclude the software test.
        var (query, dropped) = CatalogReleaseCarryOver.Trim(
            Selection("hr", ["web", "mobileOpen"], ["base", "audio"], ["softwareTest", "appTest"]), Values, Exclusions(Catalog.T1));

        query.ProductPartIds.ShouldBe(["base"]);
        query.TestTypeIds.ShouldBe(["softwareTest"]);
        dropped.Select(v => v.Key).ShouldBe(["audio", "appTest"]);
    }

    [Fact]
    public void Trim_SelectionTheReleaseAllows_DropsNothing()
    {
        var (query, dropped) = CatalogReleaseCarryOver.Trim(
            Selection("hr", ["web"], ["base"], ["softwareTest"]), Values, Exclusions(Catalog.T2));

        query.ShouldSatisfyAllConditions(
            q => q.UseCase.ShouldBe("hr"),
            q => q.AppTypeIds.ShouldBe(["web"]),
            q => q.ProductPartIds.ShouldBe(["base"]),
            q => q.TestTypeIds.ShouldBe(["softwareTest"]));
        dropped.ShouldBeEmpty();
    }

    private static ResolvedCatalogSelection Selection(string? useCase, string[] appTypes, string[] productParts, string[] testTypes) =>
        new(
            useCase is null ? null : new CatalogValueName(useCase, useCase),
            [.. appTypes.Select(Name)],
            [.. productParts.Select(Name)],
            [.. testTypes.Select(Name)],
            []);

    private static CatalogValueName Name(string key) => new(key, key);

    private static List<ExclusionRule> Exclusions(CatalogRelease release) =>
        [.. release.Exclusions.Select(e => new ExclusionRule(Ref(e.Exclusion.Source!), Ref(e.Exclusion.Target!)))];

    private static FilterValueRef Ref(FilterValue value) => new(value.Layer, value.Key);
}
