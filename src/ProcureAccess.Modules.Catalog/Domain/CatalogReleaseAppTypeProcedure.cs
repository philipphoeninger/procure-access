// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// Which procedure's texts and allowed ratings apply to an app type in a release (ARCHITECTURE §5.1
/// „Which texts apply“, C45, CAT-21); m:n, so Website oder Webanwendung may later map to the
/// Software-Test and the BIK BITV-Test (Web). Created only by <see cref="CatalogRelease.MapProcedure"/>.
/// </summary>
internal sealed class CatalogReleaseAppTypeProcedure
{
    private CatalogReleaseAppTypeProcedure()
    {
    }

    internal CatalogReleaseAppTypeProcedure(CatalogRelease release, AppType appType, TestType testType)
    {
        Release = release;
        AppType = appType;
        TestType = testType;
    }

    public int CatalogReleaseId { get; private set; }

    public CatalogRelease Release { get; private set; } = null!;

    public int AppTypeId { get; private set; }

    public AppType AppType { get; private set; } = null!;

    public int TestTypeId { get; private set; }

    public TestType TestType { get; private set; } = null!;
}
