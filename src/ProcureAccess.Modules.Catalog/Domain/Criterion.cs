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
/// A procedure criterion („Geprüft nach …“, <see cref="CriterionKind.TestProcedure"/>) names its
/// <see cref="TestType"/>; a requirement criterion never does (ARCHITECTURE §5.1, C30; also a CHECK
/// constraint). Key, kind and test type never change: cross-release matching depends on them.
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

    /// <summary>Set exactly for <see cref="CriterionKind.TestProcedure"/>.</summary>
    public int? TestTypeId { get; private set; }

    public TestType? TestType { get; private set; }

    /// <summary>
    /// Creates a criterion. <paramref name="testType"/> is required for
    /// <see cref="CriterionKind.TestProcedure"/> and not allowed for
    /// <see cref="CriterionKind.Requirement"/>.
    /// </summary>
    public static Criterion Create(
        string key,
        CriterionKind kind,
        string? category = null,
        string? supersededByKey = null,
        TestType? testType = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown criterion kind.");
        }

        if ((kind == CriterionKind.TestProcedure) != (testType is not null))
        {
            throw new ArgumentException(
                $"A test type is required for a procedure criterion and not allowed for a requirement ('{key}').",
                nameof(testType));
        }

        return new Criterion
        {
            Key = key,
            Kind = kind,
            Category = category,
            SupersededByKey = supersededByKey,
            TestType = testType,
        };
    }
}
