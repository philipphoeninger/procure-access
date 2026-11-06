// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Releases;

/// <summary>
/// The keys of the filter values a release folder may refer to, per layer. The values are global
/// and seeded by a catalog migration (O-S3-5 D1, ARCHITECTURE C104); a folder never defines them.
/// </summary>
internal sealed class FilterValueKeys
{
    private readonly Dictionary<FilterLayer, HashSet<string>> _keys;

    private FilterValueKeys(Dictionary<FilterLayer, HashSet<string>> keys)
    {
        _keys = keys;
    }

    /// <summary>The keys of <paramref name="values"/>, e.g. the seeded rows of the catalog context.</summary>
    public static FilterValueKeys From(IEnumerable<FilterValue> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var keys = Enum.GetValues<FilterLayer>().ToDictionary(layer => layer, _ => new HashSet<string>(StringComparer.Ordinal));
        foreach (var value in values)
        {
            keys[value.Layer].Add(value.Key);
        }

        return new FilterValueKeys(keys);
    }

    public bool Contains(FilterLayer layer, string key) => _keys[layer].Contains(key);
}
