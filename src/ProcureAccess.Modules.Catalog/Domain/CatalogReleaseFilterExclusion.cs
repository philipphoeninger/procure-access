// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// One exclusion rule a release applies (ARCHITECTURE §5.1, CAT-19); same pattern as
/// <see cref="CatalogReleaseCriterion"/>. Created only by <see cref="CatalogRelease.AddExclusion"/>.
/// </summary>
internal sealed class CatalogReleaseFilterExclusion
{
    private CatalogReleaseFilterExclusion()
    {
    }

    internal CatalogReleaseFilterExclusion(CatalogRelease release, FilterExclusion exclusion)
    {
        Release = release;
        Exclusion = exclusion;
    }

    public int CatalogReleaseId { get; private set; }

    public CatalogRelease Release { get; private set; } = null!;

    public int FilterExclusionId { get; private set; }

    public FilterExclusion Exclusion { get; private set; } = null!;
}
