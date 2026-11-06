// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// Layer 3, „Funktionsbereich“ (ARCHITECTURE §5.1, Kriterienkatalog §2.3). Seven values; only
/// „Basisfunktionalität“ is always selected and cannot be deselected (CAT-11).
/// </summary>
internal sealed class ProductPart : FilterValue
{
    public const int ExplanationMaxLength = 2000;

    private ProductPart()
    {
    }

    /// <summary>Explanation quoted from the Standardanforderungskatalog (Kriterienkatalog §2.3).</summary>
    public string ExplanationDe { get; private init; } = null!;

    public bool IsAlwaysSelected { get; private init; }

    public int SortOrder { get; private init; }

    public override FilterLayer Layer => FilterLayer.ProductPart;

    public static ProductPart Create(string key, string nameDe, string explanationDe, int sortOrder, bool isAlwaysSelected = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(nameDe);
        ArgumentException.ThrowIfNullOrWhiteSpace(explanationDe);
        return new ProductPart
        {
            Key = key,
            NameDe = nameDe,
            ExplanationDe = explanationDe,
            SortOrder = sortOrder,
            IsAlwaysSelected = isAlwaysSelected,
        };
    }
}
