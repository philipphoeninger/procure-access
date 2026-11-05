// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// Stable identity of a criterion across releases (ARCHITECTURE §5.1). Its content lives in
/// <see cref="CriterionRevision"/>; releases are matched by <see cref="Key"/> (ADR-002).
/// </summary>
/// <remarks>
/// <c>TestTypeId</c> (with <see cref="CriterionKind.TestProcedure"/>) comes with the test types in
/// S3-13. Key and kind never change: cross-release matching depends on them.
/// </remarks>
internal sealed class Criterion : CatalogEntity
{
    public const int KeyMaxLength = 64;
    public const int CategoryMaxLength = 128;

    private Criterion()
    {
    }

    /// <summary>Stable key, e.g. <c>BITV-SW-2.1.1</c>; the scheme is decided in S3-15 (S3-11 Q4).</summary>
    public string Key { get; private set; } = null!;

    public CriterionKind Kind { get; private set; }

    public string? Category { get; private set; }

    /// <summary>Key of the criterion that replaces this one in a later release (CAT-03).</summary>
    public string? SupersededByKey { get; private set; }

    public static Criterion Create(string key, CriterionKind kind, string? category = null, string? supersededByKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown criterion kind.");
        }

        return new Criterion
        {
            Key = key,
            Kind = kind,
            Category = category,
            SupersededByKey = supersededByKey,
        };
    }
}
