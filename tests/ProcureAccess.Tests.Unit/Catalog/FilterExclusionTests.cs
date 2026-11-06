// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Domain;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Catalog;

/// <summary>
/// CAT-13, domain part: an exclusion rule always points from a higher to a lower selection layer
/// (ARCHITECTURE §5.1, Kriterienkatalog §3.1). The database part, the CHECK constraint, is in
/// <c>CatalogRulesPersistenceTests</c> (integration).
/// </summary>
public class FilterExclusionTests
{
    /// <summary>
    /// Every source and target layer pair (as layer numbers 1–4, because <see cref="FilterLayer"/>
    /// is internal); true when the target layer is lower, i.e. has the higher number.
    /// </summary>
    public static TheoryData<int, int, bool> LayerPairs()
    {
        var data = new TheoryData<int, int, bool>();
        foreach (var source in Enum.GetValues<FilterLayer>())
        {
            foreach (var target in Enum.GetValues<FilterLayer>())
            {
                data.Add((int)source, (int)target, target > source);
            }
        }

        return data;
    }

    [Theory(DisplayName = "INV:CAT-13 " + nameof(Create_SourceAndTargetLayer_OnlyALowerTargetLayerIsAccepted))]
    [Trait("Invariant", "CAT-13")]
    [MemberData(nameof(LayerPairs))]
    public void Create_SourceAndTargetLayer_OnlyALowerTargetLayerIsAccepted(int source, int target, bool accepted)
    {
        var sourceValue = Value((FilterLayer)source, "source");
        var targetValue = Value((FilterLayer)target, "target");

        if (accepted)
        {
            var exclusion = FilterExclusion.Create(sourceValue, targetValue, "Grund");
            exclusion.Source.ShouldBeSameAs(sourceValue);
            exclusion.Target.ShouldBeSameAs(targetValue);
        }
        else
        {
            Should.Throw<ArgumentException>(() => FilterExclusion.Create(sourceValue, targetValue, "Grund"));
        }
    }

    [Fact(DisplayName = "INV:CAT-13 " + nameof(Create_RulesOfKriterienkatalog_SetTheTypedSourceAndTarget))]
    [Trait("Invariant", "CAT-13")]
    public void Create_RulesOfKriterienkatalog_SetTheTypedSourceAndTarget()
    {
        // The two rules of Kriterienkatalog §3.1, one per source layer they use.
        var hr = UseCaseTemplate.Create("hr", "HR-Anwendung");
        var document = AppType.Create("document", "Dokument", AppTypeFunctionality.NotApplicable, 6);
        var web = AppType.Create("web", "Website oder Webanwendung", AppTypeFunctionality.NotApplicable, 1);
        var appTest = TestType.Create("appTest", "BIT inklusiv App-Test", "https://apptest.bit-inklusiv.de/", 2);

        var byUseCase = FilterExclusion.Create(hr, document, "Passt nicht zum Verwendungszweck");
        var byAppType = FilterExclusion.Create(web, appTest, "Passt nicht zum Anwendungstyp");

        byUseCase.SourceUseCase.ShouldBeSameAs(hr);
        byUseCase.TargetAppType.ShouldBeSameAs(document);
        byUseCase.SourceAppType.ShouldBeNull();
        byUseCase.TargetTestType.ShouldBeNull();
        byAppType.SourceAppType.ShouldBeSameAs(web);
        byAppType.TargetTestType.ShouldBeSameAs(appTest);
        byAppType.SourceUseCase.ShouldBeNull();
        byAppType.TargetAppType.ShouldBeNull();
    }

    [Fact]
    public void Create_EmptyReason_Throws()
    {
        var hr = UseCaseTemplate.Create("hr", "HR-Anwendung");
        var document = AppType.Create("document", "Dokument", AppTypeFunctionality.NotApplicable, 6);

        Should.Throw<ArgumentException>(() => FilterExclusion.Create(hr, document, " "));
    }

    private static FilterValue Value(FilterLayer layer, string key) => layer switch
    {
        FilterLayer.UseCase => UseCaseTemplate.Create(key, key),
        FilterLayer.AppType => AppType.Create(key, key, AppTypeFunctionality.NotApplicable, 1),
        FilterLayer.ProductPart => ProductPart.Create(key, key, "Erklärung", 1),
        FilterLayer.TestType => TestType.Create(key, key, "https://example.org/", 1),
        _ => throw new ArgumentOutOfRangeException(nameof(layer)),
    };
}
