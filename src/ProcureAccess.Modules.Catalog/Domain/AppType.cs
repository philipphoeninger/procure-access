// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// Layer 2, „Anwendungstyp“ (ARCHITECTURE §5.1, Kriterienkatalog §2.2). Eight values; a selection
/// holds one or more.
/// </summary>
internal sealed class AppType : FilterValue
{
    public const int HintMaxLength = 500;

    private AppType()
    {
    }

    /// <summary>Hint below the name, e.g. what „offen“ means; null for most values.</summary>
    public string? HintDe { get; private init; }

    public AppTypeFunctionality Functionality { get; private init; }

    public int SortOrder { get; private init; }

    public override FilterLayer Layer => FilterLayer.AppType;

    public static AppType Create(
        string key,
        string nameDe,
        AppTypeFunctionality functionality,
        int sortOrder,
        string? hintDe = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(nameDe);
        if (!Enum.IsDefined(functionality))
        {
            throw new ArgumentOutOfRangeException(nameof(functionality), functionality, "Unknown functionality.");
        }

        return new AppType
        {
            Key = key,
            NameDe = nameDe,
            Functionality = functionality,
            SortOrder = sortOrder,
            HintDe = hintDe,
        };
    }
}
