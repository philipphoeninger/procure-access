// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Tests.Shared.Builders.Catalog;

/// <summary>
/// Entry point for the builders of the Catalog module (TESTING §6.1). Import it with
/// <c>using static ProcureAccess.Tests.Shared.Builders.Catalog.CatalogData;</c> and write
/// <c>ARevision().WithKey("T-1").AppliesTo(web).Build()</c>.
/// </summary>
/// <remarks>
/// Separate from <see cref="TestData"/>, whose <c>ACriterion()</c> builds a criterion of the legacy
/// tables until they go. The whole test catalog is <see cref="Fixtures.MiniCatalog"/>.
/// </remarks>
internal static class CatalogData
{
    /// <summary><c>PublishedAt</c> of every release a builder publishes, unless a test sets another.</summary>
    public static readonly DateTimeOffset DefaultPublishedAt = TestData.DefaultCreatedAt;

    /// <summary>A release; published by default, because the database holds published releases only.</summary>
    public static CatalogReleaseBuilder ACatalogRelease() => new();

    /// <summary>A requirement criterion, or with <see cref="CatalogCriterionBuilder.ProcedureFor"/> a procedure criterion.</summary>
    public static CatalogCriterionBuilder ACriterion() => new();

    /// <summary>A draft revision of a new requirement criterion, or of the criterion given with <see cref="CriterionRevisionBuilder.Of"/>.</summary>
    public static CriterionRevisionBuilder ARevision() => new();
}
