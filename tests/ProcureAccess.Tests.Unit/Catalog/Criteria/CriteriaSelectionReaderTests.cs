// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Criteria;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Reading;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Catalog.Criteria;

/// <summary>
/// The selection of a criteria request (CAT-11): at most one use case, at least one app type, every
/// product part with <c>IsAlwaysSelected</c>, any test types, each key known; and no value that a
/// selected higher-layer value excludes under the rules of the release (CAT-19). Errors are keyed as
/// the client sends the field. A broken selection is <see cref="SelectionProblem.Invalid"/>
/// (<c>validation</c>); only a well-formed one is checked against the exclusions,
/// <see cref="SelectionProblem.Excluded"/> (<c>selection-excluded</c>; owner, S3-20).
/// </summary>
public class CriteriaSelectionReaderTests
{
    private static readonly MiniCatalogValues V = MiniCatalogValues.InMemory();
    private static readonly MiniCatalog Catalog = MiniCatalog.Build(V);
    private static readonly CatalogFilterValues Values = new(
        [V.Hr, V.DatabaseApplication],
        [V.Web, V.SoftwareOpen, V.MobileOpen, V.Hardware],
        [V.Base, V.Audio, V.Video],
        V.TestTypes);

    [Fact(DisplayName = "INV:CAT-11 " + nameof(Read_CompleteSelection_ReturnsItWithoutErrors))]
    [Trait("Invariant", "CAT-11")]
    public void Read_CompleteSelection_ReturnsItWithoutErrors()
    {
        var result = Read(["hr"], ["web", "softwareOpen"], ["base", "audio"], ["pdfTest"]);

        result.Problem.ShouldBeNull();
        result.Errors.ShouldBeEmpty();
        result.Selection.ShouldBe(
            new CriteriaSelection("hr", ["web", "softwareOpen"], ["base", "audio"], ["pdfTest"]),
            SelectionComparer.Instance);
    }

    [Fact(DisplayName = "INV:CAT-11 " + nameof(Read_NoUseCaseAndNoTestType_IsComplete))]
    [Trait("Invariant", "CAT-11")]
    public void Read_NoUseCaseAndNoTestType_IsComplete()
    {
        var result = Read(null, ["web"], ["base"], null);

        result.Problem.ShouldBeNull();
        result.Selection.ShouldNotBeNull().UseCase.ShouldBeNull();
        result.Selection.TestTypes.ShouldBeEmpty();
    }

    [Fact]
    public void Read_EmptyAndRepeatedValues_CountAsNotGivenAndOnce()
    {
        var result = Read(["", "hr", "hr"], ["web", "", "web", null], ["base", "base"], [null]);

        result.Problem.ShouldBeNull();
        result.Selection.ShouldBe(new CriteriaSelection("hr", ["web"], ["base"], []), SelectionComparer.Instance);
    }

    [Fact(DisplayName = "INV:CAT-18 " + nameof(Read_InactiveUseCase_IsAccepted))]
    [Trait("Invariant", "CAT-18")]
    public void Read_InactiveUseCase_IsAccepted()
    {
        // Not offered by filter-options, but accepted like there (owner, S3-20).
        var result = Read(["databaseApplication"], ["mobileOpen"], ["base"], null);

        result.Problem.ShouldBeNull();
        result.Selection.ShouldNotBeNull().UseCase.ShouldBe("databaseApplication");
    }

    /// <summary>Broken selections. Columns: use cases, app types, product parts, test types, expected error keys.</summary>
    public static TheoryData<string?[]?, string?[]?, string?[]?, string?[]?, string[]> InvalidSelections() => new()
    {
        // No app type, also when every value is empty.
        { null, null, ["base"], null, ["appTypeIds"] },
        { null, ["", null], ["base"], null, ["appTypeIds"] },

        // Two use cases.
        { ["hr", "databaseApplication"], ["web"], ["base"], null, ["useCase"] },

        // Basisfunktionalität missing.
        { null, ["web"], null, null, ["productPartIds"] },
        { null, ["web"], ["audio"], null, ["productPartIds"] },

        // Unknown keys, keyed as sent; an unknown app type alone is not also reported as missing.
        { ["nope"], ["web", "nope"], ["base", "nope"], ["nope"], ["useCase", "appTypeIds[1]", "productPartIds[1]", "testTypeIds[0]"] },
        { null, ["nope"], ["base"], null, ["appTypeIds[0]"] },

        // A broken selection is not checked against the exclusions: HR excludes Hardware.
        { ["hr"], ["hardware"], null, null, ["productPartIds"] },
    };

    [Theory(DisplayName = "INV:CAT-11 " + nameof(Read_BrokenSelection_IsInvalidWithFieldErrors))]
    [Trait("Invariant", "CAT-11")]
    [MemberData(nameof(InvalidSelections))]
    public void Read_BrokenSelection_IsInvalidWithFieldErrors(
        string?[]? useCases,
        string?[]? appTypes,
        string?[]? productParts,
        string?[]? testTypes,
        string[] errorKeys)
    {
        var result = Read(useCases, appTypes, productParts, testTypes);

        result.Selection.ShouldBeNull();
        result.Problem.ShouldBe(SelectionProblem.Invalid);
        result.Errors.Keys.ShouldBe(errorKeys, ignoreOrder: true);
    }

    /// <summary>
    /// One row per exclusion of <c>T.1</c> (one per pair of layers). Columns: use case, app types,
    /// product parts, test types, expected error key, the key of the excluding value.
    /// </summary>
    public static TheoryData<string?, string[], string[], string[], string, string> ExcludedSelections() => new()
    {
        { "hr", ["web", "hardware"], ["base"], [], "appTypeIds[1]", "hr" },
        { "hr", ["web"], ["base", "video"], [], "productPartIds[1]", "hr" },
        { "databaseApplication", ["softwareOpen"], ["base"], ["appTest"], "testTypeIds[0]", "databaseApplication" },
        { null, ["mobileOpen"], ["base", "audio"], [], "productPartIds[1]", "mobileOpen" },
        { null, ["web"], ["base"], ["softwareTest", "appTest"], "testTypeIds[1]", "web" },
        { null, ["softwareOpen"], ["base", "audio"], ["softwareTest"], "testTypeIds[0]", "audio" },
    };

    [Theory(DisplayName = "INV:CAT-19 " + nameof(Read_ValueExcludedByTheRulesOfTheRelease_IsExcludedWithFieldError))]
    [Trait("Invariant", "CAT-19")]
    [MemberData(nameof(ExcludedSelections))]
    public void Read_ValueExcludedByTheRulesOfTheRelease_IsExcludedWithFieldError(
        string? useCase,
        string[] appTypes,
        string[] productParts,
        string[] testTypes,
        string errorKey,
        string excludedBy)
    {
        var result = Read(useCase is null ? null : [useCase], appTypes, productParts, testTypes);

        result.Selection.ShouldBeNull();
        result.Problem.ShouldBe(SelectionProblem.Excluded);
        result.Errors.Keys.ShouldBe([errorKey]);
        result.Errors[errorKey].ShouldHaveSingleItem().ShouldContain($"'{excludedBy}'");
    }

    [Fact(DisplayName = "INV:CAT-19 " + nameof(Read_SameSelection_FollowsTheRulesOfTheNamedRelease))]
    [Trait("Invariant", "CAT-19")]
    public void Read_SameSelection_FollowsTheRulesOfTheNamedRelease()
    {
        // T.1: HR excludes Video; T.2: HR excludes Audio instead.
        var video = Read(["hr"], ["web"], ["base", "video"], null, Catalog.T1);
        var videoT2 = Read(["hr"], ["web"], ["base", "video"], null, Catalog.T2);
        var audioT2 = Read(["hr"], ["web"], ["base", "audio"], null, Catalog.T2);

        video.Problem.ShouldBe(SelectionProblem.Excluded);
        videoT2.Problem.ShouldBeNull();
        audioT2.Problem.ShouldBe(SelectionProblem.Excluded);
        audioT2.Errors.Keys.ShouldBe(["productPartIds[1]"]);
    }

    [Fact]
    public void Read_ValueExcludedBySeveralSelectedValues_NamesEach()
    {
        // In T.1 both „Datenbank“ and Web exclude the App-Test.
        var result = Read(["databaseApplication"], ["web"], ["base"], ["appTest"]);

        result.Problem.ShouldBe(SelectionProblem.Excluded);
        var message = result.Errors["testTypeIds[0]"].ShouldHaveSingleItem();
        message.ShouldContain("'databaseApplication'");
        message.ShouldContain("'web'");
    }

    [Fact]
    public void Read_SeveralExcludedValues_GiveOneErrorEach()
    {
        var result = Read(["hr"], ["softwareOpen"], ["base", "video", "audio"], ["softwareTest"]);

        result.Problem.ShouldBe(SelectionProblem.Excluded);
        result.Errors.Keys.ShouldBe(["productPartIds[1]", "testTypeIds[0]"], ignoreOrder: true);
    }

    private static CriteriaSelectionResult Read(
        string?[]? useCases,
        string?[]? appTypes,
        string?[]? productParts,
        string?[]? testTypes,
        CatalogRelease? release = null) =>
        CriteriaSelectionReader.Read(Values, Exclusions(release ?? Catalog.T1), useCases, appTypes, productParts, testTypes);

    private static List<ExclusionRule> Exclusions(CatalogRelease release) =>
        [.. release.Exclusions.Select(e => new ExclusionRule(Ref(e.Exclusion.Source!), Ref(e.Exclusion.Target!)))];

    private static FilterValueRef Ref(FilterValue value) => new(value.Layer, value.Key);

    /// <summary>Compares selections by their values; records compare lists by reference.</summary>
    private sealed class SelectionComparer : IEqualityComparer<CriteriaSelection?>
    {
        public static readonly SelectionComparer Instance = new();

        public bool Equals(CriteriaSelection? x, CriteriaSelection? y) =>
            x is not null && y is not null
            && x.UseCase == y.UseCase
            && x.AppTypes.SequenceEqual(y.AppTypes)
            && x.ProductParts.SequenceEqual(y.ProductParts)
            && x.TestTypes.SequenceEqual(y.TestTypes);

        public int GetHashCode(CriteriaSelection? obj) => 0;
    }
}
