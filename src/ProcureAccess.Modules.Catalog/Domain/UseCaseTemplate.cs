// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// Layer 1, „Verwendungszweck“ (ARCHITECTURE §5.1). Global only; there is no row for „Keiner davon
/// passt“ (no template).
/// </summary>
/// <remarks>
/// An inactive template is not offered for new selections but still resolves for old
/// configurations and entries (CAT-18, from S3-19); „Datenbankgestützte Fachanwendung“ is seeded
/// inactive.
/// </remarks>
internal sealed class UseCaseTemplate : FilterValue
{
    public const int DescriptionMaxLength = 2000;

    private UseCaseTemplate()
    {
    }

    /// <summary>No document has a text for it yet, so the seed leaves it empty.</summary>
    public string? Description { get; private init; }

    public bool IsActive { get; private init; }

    public override FilterLayer Layer => FilterLayer.UseCase;

    public static UseCaseTemplate Create(string key, string nameDe, bool isActive = true, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(nameDe);
        return new UseCaseTemplate { Key = key, NameDe = nameDe, IsActive = isActive, Description = description };
    }
}
