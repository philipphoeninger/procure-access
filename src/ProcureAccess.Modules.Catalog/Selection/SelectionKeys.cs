// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Selection;

/// <summary>A key of a list parameter with its position in the request, for the error key (<c>appTypeIds[1]</c>).</summary>
internal sealed record SentKey(string Key, int Index);

/// <summary>
/// Reads the keys of a list parameter of a catalog request (<c>appTypeIds[]</c> …) the same way for
/// every catalog endpoint: empty values count as not given, a repeated known key once (at its first
/// position), and each unknown value gives a field error keyed as the client sends it
/// (<c>appTypeIds[1]</c>; SPEC §8.6, API-01).
/// </summary>
internal static class SelectionKeys
{
    public static List<SentKey> Read(
        IReadOnlyList<string?>? keys,
        IEnumerable<FilterValue> known,
        string parameter,
        string what,
        Dictionary<string, string[]> errors)
    {
        ArgumentNullException.ThrowIfNull(known);
        ArgumentNullException.ThrowIfNull(errors);
        var result = new List<SentKey>();
        if (keys is null)
        {
            return result;
        }

        var knownKeys = known.Select(v => v.Key).ToHashSet(StringComparer.Ordinal);
        for (var i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            if (string.IsNullOrEmpty(key) || result.Exists(k => k.Key == key))
            {
                continue;
            }

            if (knownKeys.Contains(key))
            {
                result.Add(new SentKey(key, i));
            }
            else
            {
                errors[$"{parameter}[{i}]"] = [$"Unknown {what} '{key}'."];
            }
        }

        return result;
    }
}
