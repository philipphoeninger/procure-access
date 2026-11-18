// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;

namespace ProcureAccess.Modules.Catalog.Reading;

/// <summary>
/// <see cref="ICatalogTestTypes"/> from the cached filter values of <see cref="CatalogReader"/>
/// (no query after the first call; a catalog migration that changes them runs at the start).
/// </summary>
internal sealed class CatalogTestTypes(CatalogReader reader) : ICatalogTestTypes
{
    public async Task<IReadOnlyList<string>> GetKeysAsync(CancellationToken cancellationToken)
    {
        var values = await reader.GetFilterValuesAsync(cancellationToken);
        return [.. values.TestTypes.OrderBy(t => t.SortOrder).ThenBy(t => t.Key, StringComparer.Ordinal).Select(t => t.Key)];
    }
}
