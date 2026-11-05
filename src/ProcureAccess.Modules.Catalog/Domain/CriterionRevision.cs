// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Catalog.Domain;

/// <summary>
/// Content of a criterion as one or more releases contain it (ARCHITECTURE §5.1). A revision is not
/// owned by a release: unchanged revisions are shared by the releases that contain them (CAT-07).
/// </summary>
/// <remarks>
/// <para><b>Immutability (CAT-05).</b> A revision can be changed only while it is a draft: created
/// with <see cref="Create"/> and not yet part of a published release. Publishing a release that
/// includes it seals it (<see cref="CatalogRelease.Publish"/>). A revision that EF loads from the
/// database is always sealed: catalog content reaches the database only through a published
/// release (owner, S3-12 decision 1). A change to a sealed revision throws
/// <see cref="PublishedCatalogChangeException"/>.</para>
/// <para>The applicability, priority and procedure-text rows come with S3-13 and follow the same
/// rule.</para>
/// </remarks>
internal sealed class CriterionRevision : CatalogEntity
{
    private readonly List<StandardReference> _standardReferences = [];

    /// <summary>Not mapped: false for every instance EF creates, so a loaded revision is sealed.</summary>
    private bool _isDraft;

    private CriterionRevision()
    {
    }

    public int CriterionId { get; private set; }

    public Criterion Criterion { get; private set; } = null!;

    /// <summary>1, 2, … per criterion.</summary>
    public int RevisionNumber { get; private set; }

    public string TitleDe { get; private set; } = null!;

    /// <summary>Plain-language title or summary (K-1); empty until the project team writes it (O-1).</summary>
    public string? PlainLanguageDe { get; private set; }

    public string? RationaleDe { get; private set; }

    public int SortOrder { get; private set; }

    public IReadOnlyCollection<StandardReference> StandardReferences => _standardReferences;

    /// <summary>True once a published release contains the revision, and for every loaded revision.</summary>
    public bool IsSealed => !_isDraft;

    public static CriterionRevision Create(
        Criterion criterion,
        int revisionNumber,
        string titleDe,
        string? plainLanguageDe = null,
        string? rationaleDe = null,
        int sortOrder = 0)
    {
        ArgumentNullException.ThrowIfNull(criterion);
        ArgumentOutOfRangeException.ThrowIfLessThan(revisionNumber, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(titleDe);
        return new CriterionRevision
        {
            Criterion = criterion,
            RevisionNumber = revisionNumber,
            TitleDe = titleDe,
            PlainLanguageDe = plainLanguageDe,
            RationaleDe = rationaleDe,
            SortOrder = sortOrder,
            _isDraft = true,
        };
    }

    public void AddStandardReference(StandardReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        ThrowIfSealed();
        if (_standardReferences.Contains(reference))
        {
            throw new ArgumentException("The revision already cites this standard reference.", nameof(reference));
        }

        _standardReferences.Add(reference);
    }

    /// <summary>Called by <see cref="CatalogRelease.Publish"/> for every revision of the release.</summary>
    internal void Seal() => _isDraft = false;

    private void ThrowIfSealed()
    {
        if (IsSealed)
        {
            throw new PublishedCatalogChangeException(
                $"Revision {RevisionNumber} of criterion '{Criterion?.Key}' belongs to a published release and cannot be changed.");
        }
    }
}
