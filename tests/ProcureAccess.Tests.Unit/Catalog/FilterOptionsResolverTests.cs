// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.FilterOptions;
using ProcureAccess.Modules.Catalog.Reading;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Catalog;

/// <summary>
/// The answer of <c>GET /api/v1/catalog/filter-options</c> without HTTP (SPEC §8.1, S3-19):
/// <c>excludedBy[]</c> under the rules of a release (CAT-12, catalog-endpoint part), inactive use
/// cases (CAT-18), the order of the lists and the check of the query values. Table-driven over the
/// exclusions of the <see cref="MiniCatalog"/>, one per pair of layers. The HTTP part, the JSON shape
/// and the release pinning on stored releases (CAT-19) are in <c>FilterOptionsEndpointTests</c>.
/// </summary>
public class FilterOptionsResolverTests
{
    private static readonly MiniCatalogValues V = MiniCatalogValues.InMemory();
    private static readonly MiniCatalog Catalog = MiniCatalog.Build(V);
    private static readonly CatalogFilterValues Values = new(
        [V.Hr, V.DatabaseApplication],
        [V.Web, V.SoftwareOpen, V.MobileOpen, V.Hardware],
        [V.Base, V.Audio, V.Video],
        V.TestTypes);

    /// <summary>Every exclusion rule of <c>T.1</c> as (source layer, source key, target layer, target key).</summary>
    public static TheoryData<int, string, int, string> T1Rules()
    {
        var data = new TheoryData<int, string, int, string>();
        foreach (var rule in Snapshot(Catalog.T1).Exclusions)
        {
            data.Add((int)rule.Source.Layer, rule.Source.Key, (int)rule.Target.Layer, rule.Target.Key);
        }

        return data;
    }

    [Fact(DisplayName = "INV:CAT-12 " + nameof(Rules_OfT1_CoverEveryPairOfLayers))]
    [Trait("Invariant", "CAT-12")]
    public void Rules_OfT1_CoverEveryPairOfLayers()
    {
        // The theory below is only as good as its data: one rule for each of the six pairs.
        var pairs = Snapshot(Catalog.T1).Exclusions.Select(r => (r.Source.Layer, r.Target.Layer)).Distinct().ToList();

        pairs.Count.ShouldBe(6);
    }

    [Theory(DisplayName = "INV:CAT-12 " + nameof(Resolve_SourceOfARuleSelected_ExcludesItsTargetsAndNothingElse))]
    [Trait("Invariant", "CAT-12")]
    [MemberData(nameof(T1Rules))]
    public void Resolve_SourceOfARuleSelected_ExcludesItsTargetsAndNothingElse(int sourceLayer, string sourceKey, int targetLayer, string targetKey)
    {
        var selection = (FilterLayer)sourceLayer switch
        {
            FilterLayer.UseCase => new FilterSelection(sourceKey, [], []),
            FilterLayer.AppType => new FilterSelection(null, [sourceKey], []),
            _ => new FilterSelection(null, [], [sourceKey]),
        };

        var response = FilterOptionsResolver.Resolve(Values, Snapshot(Catalog.T1), selection);

        var source = Values.UseCases.Concat<FilterValue>(Values.AppTypes).Concat(Values.ProductParts)
            .Single(v => (int)v.Layer == sourceLayer && v.Key == sourceKey);
        var expected = new ExcludingValue(FilterOptionsResolver.LayerName(source.Layer), source.Key, source.NameDe);
        // The rule of this row and every other rule of the same source (HR excludes Hardware and Video).
        var targets = Snapshot(Catalog.T1).Exclusions
            .Where(r => r.Source == Ref(source))
            .Select(r => ((int)r.Target.Layer, r.Target.Key))
            .ToHashSet();
        targets.ShouldContain((targetLayer, targetKey));
        foreach (var (layer, id, excludedBy) in AllOptions(response))
        {
            if (targets.Contains((layer, id)))
            {
                excludedBy.ShouldBe([expected]);
            }
            else
            {
                excludedBy.ShouldBeEmpty($"{id} must not be excluded by {sourceKey} alone");
            }
        }
    }

    [Fact(DisplayName = "INV:CAT-12 " + nameof(Resolve_NoSelection_EveryExcludedByIsEmpty))]
    [Trait("Invariant", "CAT-12")]
    public void Resolve_NoSelection_EveryExcludedByIsEmpty()
    {
        var response = FilterOptionsResolver.Resolve(Values, Snapshot(Catalog.T1), FilterSelection.None);

        AllOptions(response).ShouldAllBe(option => option.ExcludedBy.Count == 0);
    }

    [Fact(DisplayName = "INV:CAT-12 " + nameof(Resolve_OptionExcludedBySeveralSelectedValues_ListsEachInLayerOrder))]
    [Trait("Invariant", "CAT-12")]
    public void Resolve_OptionExcludedBySeveralSelectedValues_ListsEachInLayerOrder()
    {
        // In T.1 the App-Test is excluded by the use case „Datenbankgestützte Fachanwendung“ and by
        // the app type „Website oder Webanwendung“; the Software-Test by the product part Audio.
        var selection = new FilterSelection(V.DatabaseApplication.Key, [V.Web.Key, V.SoftwareOpen.Key], [V.Base.Key, V.Audio.Key]);

        var response = FilterOptionsResolver.Resolve(Values, Snapshot(Catalog.T1), selection);

        var testTypes = response.Layers.TestTypes.ToDictionary(t => t.Id, t => t.ExcludedBy);
        testTypes[V.AppTest.Key].ShouldBe([
            new ExcludingValue("useCase", V.DatabaseApplication.Key, V.DatabaseApplication.NameDe),
            new ExcludingValue("appType", V.Web.Key, V.Web.NameDe),
        ]);
        testTypes[V.SoftwareTest.Key].ShouldBe([new ExcludingValue("productPart", V.Audio.Key, V.Audio.NameDe)]);
    }

    [Fact(DisplayName = "INV:CAT-12 " + nameof(Resolve_AlwaysSelectedPartNotSent_DoesNotCountAsSelected))]
    [Trait("Invariant", "CAT-12")]
    public void Resolve_AlwaysSelectedPartNotSent_DoesNotCountAsSelected()
    {
        // Owner, S3-19: only what the request names is selected; the client always sends Basis.
        var t1 = Snapshot(Catalog.T1);
        var snapshot = t1 with { Exclusions = [.. t1.Exclusions, new ExclusionRule(Ref(V.Base), Ref(V.PdfTest))] };

        var notSent = FilterOptionsResolver.Resolve(Values, snapshot, FilterSelection.None);
        var sent = FilterOptionsResolver.Resolve(Values, snapshot, new FilterSelection(null, [], [V.Base.Key]));

        notSent.Layers.TestTypes.Single(t => t.Id == V.PdfTest.Key).ExcludedBy.ShouldBeEmpty();
        sent.Layers.TestTypes.Single(t => t.Id == V.PdfTest.Key).ExcludedBy
            .ShouldBe([new ExcludingValue("productPart", V.Base.Key, V.Base.NameDe)]);
    }

    [Fact(DisplayName = "INV:CAT-18 " + nameof(Resolve_InactiveUseCase_IsNotOffered))]
    [Trait("Invariant", "CAT-18")]
    public void Resolve_InactiveUseCase_IsNotOffered()
    {
        var response = FilterOptionsResolver.Resolve(Values, Snapshot(Catalog.T1), FilterSelection.None);

        response.Layers.UseCases.Select(u => u.Id).ShouldBe([V.Hr.Key]);
    }

    [Fact(DisplayName = "INV:CAT-18 " + nameof(ReadSelection_InactiveUseCase_IsAcceptedAndItsRulesApply))]
    [Trait("Invariant", "CAT-18")]
    public void ReadSelection_InactiveUseCase_IsAcceptedAndItsRulesApply()
    {
        var (selection, errors) = FilterOptionsResolver.ReadSelection(Values, V.DatabaseApplication.Key, null, null);

        errors.ShouldBeEmpty();
        selection.ShouldNotBeNull().UseCase.ShouldBe(V.DatabaseApplication.Key);
        var response = FilterOptionsResolver.Resolve(Values, Snapshot(Catalog.T1), selection);
        response.Layers.TestTypes.Single(t => t.Id == V.AppTest.Key).ExcludedBy
            .ShouldBe([new ExcludingValue("useCase", V.DatabaseApplication.Key, V.DatabaseApplication.NameDe)]);
    }

    [Fact]
    public void Resolve_Lists_UseCasesByGermanNameOthersBySortOrder()
    {
        // „Ärztliche …“ sorts after „Abrechnung“ and before „Bericht“ in German, after „Z…“ by ordinal.
        var values = Values with
        {
            UseCases =
            [
                UseCaseTemplate.Create("zeit", "Zeiterfassung"),
                UseCaseTemplate.Create("bericht", "Bericht"),
                UseCaseTemplate.Create("arzt", "Ärztliche Anwendung"),
                UseCaseTemplate.Create("abrechnung", "Abrechnung"),
            ],
            AppTypes = [V.Hardware, V.MobileOpen, V.Web, V.SoftwareOpen],
            ProductParts = [V.Video, V.Base, V.Audio],
            TestTypes = [.. V.TestTypes.Reverse()],
        };

        var layers = FilterOptionsResolver.Resolve(values, Snapshot(Catalog.T1), FilterSelection.None).Layers;

        layers.UseCases.Select(u => u.Id).ShouldBe(["abrechnung", "arzt", "bericht", "zeit"]);
        layers.AppTypes.Select(a => a.Id).ShouldBe([V.Web.Key, V.SoftwareOpen.Key, V.MobileOpen.Key, V.Hardware.Key]);
        layers.ProductParts.Select(p => p.Id).ShouldBe([V.Base.Key, V.Audio.Key, V.Video.Key]);
        layers.TestTypes.Select(t => t.Id).ShouldBe(V.TestTypes.Select(t => t.Key));
    }

    [Fact]
    public void Resolve_Options_CarryTheFieldsOfSpec81()
    {
        var layers = FilterOptionsResolver.Resolve(Values, Snapshot(Catalog.T1), FilterSelection.None).Layers;

        var softwareOpen = layers.AppTypes.Single(a => a.Id == V.SoftwareOpen.Key);
        (softwareOpen.Name, softwareOpen.Hint).ShouldBe((V.SoftwareOpen.NameDe, V.SoftwareOpen.HintDe));
        var basePart = layers.ProductParts.Single(p => p.Id == V.Base.Key);
        (basePart.Name, basePart.Explanation, basePart.AlwaysSelected).ShouldBe((V.Base.NameDe, V.Base.ExplanationDe, true));
        layers.TestTypes.Single(t => t.Id == V.PdfTest.Key).Url.ShouldBe(V.PdfTest.Url);
        layers.UseCases.Single().Hint.ShouldBeNull();
    }

    [Fact]
    public void ReadSelection_UnknownKeys_ReturnsOneErrorPerKeyAsTheClientSendsIt()
    {
        var (selection, errors) = FilterOptionsResolver.ReadSelection(
            Values, "nope", [V.Web.Key, "nope", V.Web.Key], ["nope"]);

        selection.ShouldBeNull();
        errors.Keys.ShouldBe(["useCase", "appTypeIds[1]", "productPartIds[0]"], ignoreOrder: true);
    }

    [Fact]
    public void ReadSelection_EmptyAndRepeatedValues_CountAsNotGivenAndOnce()
    {
        var (selection, errors) = FilterOptionsResolver.ReadSelection(
            Values, "", [V.Web.Key, "", V.Web.Key, null], []);

        errors.ShouldBeEmpty();
        selection.ShouldNotBeNull();
        selection.UseCase.ShouldBeNull();
        selection.AppTypes.ShouldBe([V.Web.Key]);
        selection.ProductParts.ShouldBeEmpty();
    }

    private static FilterValueRef Ref(FilterValue value) => new(value.Layer, value.Key);

    private static CatalogReleaseSnapshot Snapshot(CatalogRelease release) => new(
        0,
        release.Version,
        release.PublishedAt!.Value,
        [],
        [.. release.Exclusions.Select(e => new ExclusionRule(Ref(e.Exclusion.Source!), Ref(e.Exclusion.Target!)))]);

    private static IEnumerable<(int Layer, string Id, IReadOnlyList<ExcludingValue> ExcludedBy)> AllOptions(FilterOptionsResponse response) =>
        response.Layers.UseCases.Select(o => ((int)FilterLayer.UseCase, o.Id, o.ExcludedBy))
            .Concat(response.Layers.AppTypes.Select(o => ((int)FilterLayer.AppType, o.Id, o.ExcludedBy)))
            .Concat(response.Layers.ProductParts.Select(o => ((int)FilterLayer.ProductPart, o.Id, o.ExcludedBy)))
            .Concat(response.Layers.TestTypes.Select(o => ((int)FilterLayer.TestType, o.Id, o.ExcludedBy)));
}
