// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// A value of one of the four selection layers (ARCHITECTURE §5.1): <see cref="UseCaseTemplate"/>,
/// <see cref="AppType"/>, <see cref="ProductPart"/> or <see cref="TestType"/>. Not mapped itself;
/// each value type has its own table.
/// </summary>
/// <remarks>
/// The values are global, not part of a release, and the catalog migration seeds them (O-S3-5 D1,
/// <c>Persistence/Seed/FilterValueSeed.cs</c>); release folders refer to them by <see cref="Key"/>.
/// No value type has a method that changes it: a changed name or hint is a new migration.
/// </remarks>
internal abstract class FilterValue : CatalogEntity
{
    public const int KeyMaxLength = 64;
    public const int NameMaxLength = 200;

    /// <summary>Stable key, e.g. <c>web</c> or <c>pdfTest</c> (SPEC §8.1).</summary>
    public string Key { get; protected init; } = null!;

    /// <summary>Name in the UI (Kriterienkatalog §2).</summary>
    public string NameDe { get; protected init; } = null!;

    public abstract FilterLayer Layer { get; }
}
