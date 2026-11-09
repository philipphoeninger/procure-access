// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Reading;
using ProcureAccess.Modules.Catalog.Selection;

namespace ProcureAccess.Modules.Catalog.FilterOptions;

/// <summary>
/// The selection a filter-options request sends (SPEC §8.1): at most one use case, app types and
/// product parts, by key. Test types are the lowest layer and exclude nothing, so they are not sent.
/// </summary>
internal sealed record FilterSelection(string? UseCase, IReadOnlyList<string> AppTypes, IReadOnlyList<string> ProductParts)
{
    public static readonly FilterSelection None = new(null, [], []);
}

/// <summary>
/// Builds the answer of <c>GET /api/v1/catalog/filter-options</c> (SPEC §8.1, C77): every filter
/// value of the four layers, each with the selected higher-layer values that exclude it under the
/// rules of the release (CAT-12, CAT-19). Pure: the endpoint loads the values and the release.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>A value counts as selected only if the request names it, also a product part with
/// <c>IsAlwaysSelected</c>: without query parameters every <c>excludedBy</c> is empty (SPEC §8.1;
/// the client always sends those parts, CAT-11; owner, S3-19).</item>
/// <item>Inactive use cases are not listed but are accepted in the selection, so an old selection
/// still resolves (CAT-18; owner, S3-19 decision 4).</item>
/// <item>No reason text: the client builds it from UX copy (C77).</item>
/// </list>
/// </remarks>
internal static class FilterOptionsResolver
{
    public const string UseCaseParameter = "useCase";
    public const string AppTypesParameter = "appTypeIds[]";
    public const string ProductPartsParameter = "productPartIds[]";

    /// <summary>German order for the use-case list, which has no sort order of its own (owner, S3-19 decision 5).</summary>
    private static readonly StringComparer GermanNameOrder = StringComparer.Create(CultureInfo.GetCultureInfo("de-DE"), ignoreCase: false);

    /// <summary>
    /// Reads the query values into a <see cref="FilterSelection"/>. Empty values count as not given
    /// and repeated values once. Returns the field errors of unknown keys, keyed as the client sends
    /// them (<c>useCase</c>, <c>appTypeIds[1]</c>); the selection is null when there are any.
    /// </summary>
    public static (FilterSelection? Selection, Dictionary<string, string[]> Errors) ReadSelection(
        CatalogFilterValues values,
        string? useCase,
        IReadOnlyList<string?>? appTypeIds,
        IReadOnlyList<string?>? productPartIds)
    {
        ArgumentNullException.ThrowIfNull(values);
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        string? selectedUseCase = null;
        if (!string.IsNullOrEmpty(useCase))
        {
            if (values.UseCases.Any(u => u.Key == useCase))
            {
                selectedUseCase = useCase;
            }
            else
            {
                errors[UseCaseParameter] = [$"Unknown use case '{useCase}'."];
            }
        }

        var appTypes = SelectionKeys.Read(appTypeIds, values.AppTypes, "appTypeIds", "app type", errors)
            .Select(k => k.Key).ToList();
        var productParts = SelectionKeys.Read(productPartIds, values.ProductParts, "productPartIds", "product part", errors)
            .Select(k => k.Key).ToList();

        return errors.Count == 0
            ? (new FilterSelection(selectedUseCase, appTypes, productParts), errors)
            : (null, errors);
    }

    /// <summary>The answer for <paramref name="selection"/> under the rules of <paramref name="release"/>.</summary>
    public static FilterOptionsResponse Resolve(CatalogFilterValues values, CatalogReleaseSnapshot release, FilterSelection selection)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(release);
        ArgumentNullException.ThrowIfNull(selection);

        var useCases = values.UseCases
            .OrderBy(u => u.NameDe, GermanNameOrder).ThenBy(u => u.Key, StringComparer.Ordinal)
            .ToList();
        var appTypes = values.AppTypes.OrderBy(a => a.SortOrder).ThenBy(a => a.Key, StringComparer.Ordinal).ToList();
        var productParts = values.ProductParts.OrderBy(p => p.SortOrder).ThenBy(p => p.Key, StringComparer.Ordinal).ToList();
        var testTypes = values.TestTypes.OrderBy(t => t.SortOrder).ThenBy(t => t.Key, StringComparer.Ordinal).ToList();

        // Each value with its position in its list: an option's excludedBy follows the list order.
        var byRef = new Dictionary<FilterValueRef, (FilterValue Value, int Position)>();
        foreach (var list in new IReadOnlyList<FilterValue>[] { useCases, appTypes, productParts, testTypes })
        {
            for (var i = 0; i < list.Count; i++)
            {
                byRef[new FilterValueRef(list[i].Layer, list[i].Key)] = (list[i], i);
            }
        }

        var selected = new HashSet<FilterValueRef>(
            selection.AppTypes.Select(k => new FilterValueRef(FilterLayer.AppType, k))
                .Concat(selection.ProductParts.Select(k => new FilterValueRef(FilterLayer.ProductPart, k))));
        if (selection.UseCase is not null)
        {
            selected.Add(new FilterValueRef(FilterLayer.UseCase, selection.UseCase));
        }

        var excluders = release.Exclusions
            .Where(rule => selected.Contains(rule.Source))
            .ToLookup(rule => rule.Target, rule => rule.Source);

        IReadOnlyList<ExcludingValue> ExcludedBy(FilterValue target) =>
            excluders[new FilterValueRef(target.Layer, target.Key)]
                .Distinct()
                .Select(source => byRef.TryGetValue(source, out var found)
                    ? found
                    : throw new InvalidOperationException(
                        $"Release {release.Version} has a rule from unknown filter value '{source.Key}' ({source.Layer})."))
                .OrderBy(found => found.Value.Layer).ThenBy(found => found.Position)
                .Select(found => new ExcludingValue(LayerName(found.Value.Layer), found.Value.Key, found.Value.NameDe))
                .ToList();

        return new FilterOptionsResponse(
            release.Version,
            new FilterOptionsLayers(
                useCases.Where(u => u.IsActive)
                    .Select(u => new UseCaseOption(u.Key, u.NameDe, u.Description, ExcludedBy(u)))
                    .ToList(),
                appTypes.Select(a => new AppTypeOption(a.Key, a.NameDe, a.HintDe, ExcludedBy(a))).ToList(),
                productParts
                    .Select(p => new ProductPartOption(p.Key, p.NameDe, p.ExplanationDe, p.IsAlwaysSelected, ExcludedBy(p)))
                    .ToList(),
                testTypes.Select(t => new TestTypeOption(t.Key, t.NameDe, t.Url, ExcludedBy(t))).ToList()));
    }

    /// <summary>The name of a layer in <c>excludedBy[].layer</c> (SPEC §8.1).</summary>
    public static string LayerName(FilterLayer layer) => layer switch
    {
        FilterLayer.UseCase => "useCase",
        FilterLayer.AppType => "appType",
        FilterLayer.ProductPart => "productPart",
        FilterLayer.TestType => "testType",
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Unknown filter layer."),
    };
}
