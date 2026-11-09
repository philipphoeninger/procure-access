// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Reading;
using ProcureAccess.Modules.Catalog.Selection;

namespace ProcureAccess.Modules.Catalog.Criteria;

/// <summary>
/// A complete selection of the four filter layers by key (ARCHITECTURE §5.1, CAT-11): at most one
/// use case, at least one app type, every product part with <c>IsAlwaysSelected</c> among the
/// product parts, and any test types. Lists keep the order of the request.
/// </summary>
internal sealed record CriteriaSelection(
    string? UseCase,
    IReadOnlyList<string> AppTypes,
    IReadOnlyList<string> ProductParts,
    IReadOnlyList<string> TestTypes);

/// <summary>Why a selection was refused.</summary>
internal enum SelectionProblem
{
    /// <summary>Not a complete selection: unknown key, two use cases, no app type, an always-selected part missing (problem type <c>validation</c>).</summary>
    Invalid = 1,

    /// <summary>Complete, but a value is excluded by a selected higher-layer value under the rules of the release (problem type <c>selection-excluded</c>; owner, S3-20).</summary>
    Excluded = 2,
}

/// <summary>
/// The selection, or why it was refused with one error per field, keyed as the client sends it
/// (<c>useCase</c>, <c>appTypeIds</c>, <c>appTypeIds[1]</c>).
/// </summary>
internal sealed record CriteriaSelectionResult(
    CriteriaSelection? Selection,
    SelectionProblem? Problem,
    IReadOnlyDictionary<string, string[]> Errors);

/// <summary>
/// Reads and checks the selection of a criteria request (CAT-11, CAT-19). Pure: the caller passes
/// the filter values and the exclusion rules of the release.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Empty values count as not given and a repeated key once, as in filter-options.</item>
/// <item>An inactive use case is accepted and its rules apply (CAT-18; owner, S3-20): the
/// filter-options endpoint does not offer it, which is what keeps it out of new selections.</item>
/// <item>Only a selection without <see cref="SelectionProblem.Invalid"/> errors is checked against
/// the exclusions, so the two problem types never mix.</item>
/// </list>
/// </remarks>
internal static class CriteriaSelectionReader
{
    public const string UseCaseParameter = "useCase";
    public const string AppTypesParameter = "appTypeIds[]";
    public const string ProductPartsParameter = "productPartIds[]";
    public const string TestTypesParameter = "testTypeIds[]";

    private const string AppTypesField = "appTypeIds";
    private const string ProductPartsField = "productPartIds";
    private const string TestTypesField = "testTypeIds";

    public static CriteriaSelectionResult Read(
        CatalogFilterValues values,
        IReadOnlyList<ExclusionRule> exclusions,
        IReadOnlyList<string?>? useCases,
        IReadOnlyList<string?>? appTypeIds,
        IReadOnlyList<string?>? productPartIds,
        IReadOnlyList<string?>? testTypeIds)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(exclusions);
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        var useCase = ReadUseCase(values, useCases, errors);
        var appTypes = SelectionKeys.Read(appTypeIds, values.AppTypes, AppTypesField, "app type", errors);
        var productParts = SelectionKeys.Read(productPartIds, values.ProductParts, ProductPartsField, "product part", errors);
        var testTypes = SelectionKeys.Read(testTypeIds, values.TestTypes, TestTypesField, "test type", errors);

        if (appTypes.Count == 0 && !errors.Keys.Any(k => k.StartsWith(AppTypesField + "[", StringComparison.Ordinal)))
        {
            errors[AppTypesField] = ["Select at least one app type."];
        }

        var missingParts = values.ProductParts
            .Where(p => p.IsAlwaysSelected && !productParts.Exists(k => k.Key == p.Key))
            .Select(p => p.Key)
            .ToList();
        if (missingParts.Count > 0)
        {
            errors[ProductPartsField] = [$"Product part(s) {Quoted(missingParts)} are always selected and must be sent."];
        }

        if (errors.Count > 0)
        {
            return new CriteriaSelectionResult(null, SelectionProblem.Invalid, errors);
        }

        var selected = appTypes.Select(k => new FilterValueRef(FilterLayer.AppType, k.Key))
            .Concat(productParts.Select(k => new FilterValueRef(FilterLayer.ProductPart, k.Key)))
            .ToHashSet();
        if (useCase is not null)
        {
            selected.Add(new FilterValueRef(FilterLayer.UseCase, useCase));
        }

        var excluders = exclusions
            .Where(rule => selected.Contains(rule.Source))
            .ToLookup(rule => rule.Target, rule => rule.Source);
        CheckExcluded(appTypes, FilterLayer.AppType, AppTypesField, excluders, errors);
        CheckExcluded(productParts, FilterLayer.ProductPart, ProductPartsField, excluders, errors);
        CheckExcluded(testTypes, FilterLayer.TestType, TestTypesField, excluders, errors);
        if (errors.Count > 0)
        {
            return new CriteriaSelectionResult(null, SelectionProblem.Excluded, errors);
        }

        return new CriteriaSelectionResult(
            new CriteriaSelection(
                useCase,
                [.. appTypes.Select(k => k.Key)],
                [.. productParts.Select(k => k.Key)],
                [.. testTypes.Select(k => k.Key)]),
            null,
            errors);
    }

    private static string? ReadUseCase(CatalogFilterValues values, IReadOnlyList<string?>? useCases, Dictionary<string, string[]> errors)
    {
        var sent = (useCases ?? []).Where(u => !string.IsNullOrEmpty(u)).Select(u => u!).Distinct(StringComparer.Ordinal).ToList();
        if (sent.Count > 1)
        {
            errors[UseCaseParameter] = [$"Select at most one use case, not {Quoted(sent)}."];
            return null;
        }

        if (sent.Count == 0)
        {
            return null;
        }

        if (values.UseCases.Any(u => u.Key == sent[0]))
        {
            return sent[0];
        }

        errors[UseCaseParameter] = [$"Unknown use case '{sent[0]}'."];
        return null;
    }

    private static void CheckExcluded(
        List<SentKey> keys,
        FilterLayer layer,
        string field,
        ILookup<FilterValueRef, FilterValueRef> excluders,
        Dictionary<string, string[]> errors)
    {
        foreach (var key in keys)
        {
            var sources = excluders[new FilterValueRef(layer, key.Key)]
                .Distinct()
                .OrderBy(s => s.Layer).ThenBy(s => s.Key, StringComparer.Ordinal)
                .Select(s => $"{LayerName(s.Layer)} '{s.Key}'")
                .ToList();
            if (sources.Count > 0)
            {
                errors[$"{field}[{key.Index}]"] = [$"'{key.Key}' is excluded by {string.Join(", ", sources)}."];
            }
        }
    }

    private static string LayerName(FilterLayer layer) => layer switch
    {
        FilterLayer.UseCase => "use case",
        FilterLayer.AppType => "app type",
        FilterLayer.ProductPart => "product part",
        FilterLayer.TestType => "test type",
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Unknown filter layer."),
    };

    private static string Quoted(IEnumerable<string> keys) => string.Join(", ", keys.Select(k => $"'{k}'"));
}
