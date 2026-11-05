// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// One revision of a release (ARCHITECTURE §5.1, „Release composition“). Created only by
/// <see cref="CatalogRelease.Include"/>.
/// </summary>
internal sealed class CatalogReleaseCriterion
{
    private CatalogReleaseCriterion()
    {
    }

    internal CatalogReleaseCriterion(CatalogRelease release, CriterionRevision revision)
    {
        Release = release;
        Revision = revision;
    }

    public int CatalogReleaseId { get; private set; }

    public CatalogRelease Release { get; private set; } = null!;

    public int CriterionRevisionId { get; private set; }

    public CriterionRevision Revision { get; private set; } = null!;
}
