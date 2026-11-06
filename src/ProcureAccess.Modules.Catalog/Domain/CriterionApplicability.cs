// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// Where a requirement criterion applies: for an app type, optionally limited to one product part
/// (ARCHITECTURE §5.1). Part of its <see cref="CriterionRevision"/>; created only by
/// <see cref="CriterionRevision.AddApplicability"/>. How a selection is matched against these rows
/// is S3-20's.
/// </summary>
internal sealed class CriterionApplicability : CatalogEntity
{
    private CriterionApplicability()
    {
    }

    internal CriterionApplicability(CriterionRevision revision, AppType appType, ProductPart? productPart)
    {
        Revision = revision;
        AppType = appType;
        ProductPart = productPart;
    }

    public int CriterionRevisionId { get; private set; }

    public CriterionRevision Revision { get; private set; } = null!;

    public int AppTypeId { get; private set; }

    public AppType AppType { get; private set; } = null!;

    public int? ProductPartId { get; private set; }

    public ProductPart? ProductPart { get; private set; }
}
