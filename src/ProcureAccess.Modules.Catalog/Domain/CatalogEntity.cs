// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// Key and creation time of every catalog entity except the join rows (ARCHITECTURE §5 BaseEntity;
/// the <c>xmin</c> concurrency token is a shadow property, see <c>CatalogEntityConfiguration</c>).
/// </summary>
internal abstract class CatalogEntity
{
    public int Id { get; private set; }

    /// <summary>Set by <c>CatalogDbContext</c> from <see cref="TimeProvider"/> when the row is added.</summary>
    public DateTimeOffset CreatedAt { get; private set; }
}
