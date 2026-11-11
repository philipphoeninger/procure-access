// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence.Seed;
using ProcureAccess.Modules.Catalog.Releases;

namespace ProcureAccess.Tests.Shared.Fixtures;

/// <summary>
/// The keys of every seeded filter value (<see cref="FilterValueSeed"/>), without a database: what
/// <c>catalog import</c> lets a real release folder refer to (S4-05). The catalog migration writes
/// the same rows, which OPS-03 holds (no pending model changes).
/// </summary>
/// <remarks>
/// The seed rows are anonymous objects for EF's <c>HasData</c>, so their <c>Key</c> is read by
/// reflection. Only the keys count; names and the other fields of the values built here are
/// placeholders. Unit tests of the MiniCatalog use <see cref="MiniCatalogValues.InMemory"/> instead.
/// </remarks>
internal static class SeededFilterValueKeys
{
    public static FilterValueKeys Create() => FilterValueKeys.From(
    [
        .. KeysOf(FilterValueSeed.UseCases).Select(key => (FilterValue)UseCaseTemplate.Create(key, key)),
        .. KeysOf(FilterValueSeed.AppTypes).Select(key => (FilterValue)AppType.Create(key, key, AppTypeFunctionality.NotApplicable, 0)),
        .. KeysOf(FilterValueSeed.ProductParts).Select(key => (FilterValue)ProductPart.Create(key, key, key, 0)),
        .. KeysOf(FilterValueSeed.TestTypes).Select(key => (FilterValue)TestType.Create(key, key, "https://example.invalid/", 0)),
    ]);

    private static IEnumerable<string> KeysOf(IEnumerable<object> rows) =>
        rows.Select(row => row.GetType().GetProperty("Key")?.GetValue(row) as string
            ?? throw new InvalidOperationException($"The seed row {row} has no string property Key."));
}
