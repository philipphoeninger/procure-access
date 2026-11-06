// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Shared.Builders.Catalog.CatalogData;

namespace ProcureAccess.Tests.Unit.Catalog;

/// <summary>
/// CAT-05, unit part: a published release and the revisions it contains cannot be changed; every
/// attempt fails with <see cref="PublishedCatalogChangeException"/> instead of being ignored
/// (ARCHITECTURE §5.1, ADR-002). Only <see cref="CatalogRelease.IsActive"/> still changes (owner,
/// S3-12 decision 1). The integration part (loaded entities, the import) is in
/// <c>CatalogPersistenceTests</c> and in S3-16.
/// </summary>
public class CatalogReleaseImmutabilityTests
{
    private static readonly DateTimeOffset PublishedAt = new(2026, 11, 6, 10, 0, 0, TimeSpan.Zero);

    [Fact(DisplayName = "INV:CAT-05 " + nameof(Include_PublishedRelease_ThrowsPublishedCatalogChange))]
    [Trait("Invariant", "CAT-05")]
    public void Include_PublishedRelease_ThrowsPublishedCatalogChange()
    {
        var release = PublishedRelease("T.1", Revision("K-1"));

        Should.Throw<PublishedCatalogChangeException>(() => release.Include(Revision("K-2")));

        release.Criteria.Count.ShouldBe(1);
    }

    [Fact(DisplayName = "INV:CAT-05 " + nameof(Publish_PublishedRelease_ThrowsAndKeepsPublishedAt))]
    [Trait("Invariant", "CAT-05")]
    public void Publish_PublishedRelease_ThrowsAndKeepsPublishedAt()
    {
        var release = PublishedRelease("T.1", Revision("K-1"));

        Should.Throw<PublishedCatalogChangeException>(() => release.Publish(PublishedAt.AddDays(1)));

        release.PublishedAt.ShouldBe(PublishedAt);
    }

    [Fact(DisplayName = "INV:CAT-05 " + nameof(AddStandardReference_RevisionOfPublishedRelease_ThrowsPublishedCatalogChange))]
    [Trait("Invariant", "CAT-05")]
    public void AddStandardReference_RevisionOfPublishedRelease_ThrowsPublishedCatalogChange()
    {
        var revision = Revision("K-1");
        PublishedRelease("T.1", revision);

        revision.IsSealed.ShouldBeTrue();
        Should.Throw<PublishedCatalogChangeException>(
            () => revision.AddStandardReference(StandardReference.Create(Standard.En301549, "11.2.1.1")));

        revision.StandardReferences.ShouldBeEmpty();
    }

    [Fact(DisplayName = "INV:CAT-05 " + nameof(Include_SealedRevisionInNewDraftRelease_SharesItAndKeepsItSealed))]
    [Trait("Invariant", "CAT-05")]
    public void Include_SealedRevisionInNewDraftRelease_SharesItAndKeepsItSealed()
    {
        // An unchanged revision of release n is shared by release n+1 (CAT-07); it stays sealed,
        // because release n still contains it.
        var shared = Revision("K-1");
        PublishedRelease("T.1", shared);
        var next = CatalogRelease.Create("T.2");

        next.Include(shared);

        next.Criteria.ShouldHaveSingleItem().Revision.ShouldBeSameAs(shared);
        Should.Throw<PublishedCatalogChangeException>(
            () => shared.AddStandardReference(StandardReference.Create(Standard.Wcag, "1.4.3")));
    }

    [Fact(DisplayName = "INV:CAT-05 " + nameof(ActivateAndDeactivate_PublishedRelease_ChangeOnlyIsActive))]
    [Trait("Invariant", "CAT-05")]
    public void ActivateAndDeactivate_PublishedRelease_ChangeOnlyIsActive()
    {
        var release = PublishedRelease("T.1", Revision("K-1"));

        release.Activate();
        release.IsActive.ShouldBeTrue();
        release.Deactivate();

        release.IsActive.ShouldBeFalse();
        release.PublishedAt.ShouldBe(PublishedAt);
        release.Criteria.Count.ShouldBe(1);
    }

    [Fact]
    public void DraftRelease_IncludeAndAddReference_ChangesUntilPublished()
    {
        var revision = Revision("K-1");
        var reference = StandardReference.Create(Standard.En301549, "11.2.1.1", "https://example.org/en301549#11.2.1.1");
        var release = CatalogRelease.Create("T.1", "Erste Testversion");

        revision.AddStandardReference(reference);
        release.Include(revision);

        release.IsPublished.ShouldBeFalse();
        revision.IsSealed.ShouldBeFalse();
        revision.StandardReferences.ShouldHaveSingleItem().ShouldBeSameAs(reference);
        release.Publish(PublishedAt);
        release.IsPublished.ShouldBeTrue();
        revision.IsSealed.ShouldBeTrue();
    }

    [Fact]
    public void Include_SecondRevisionOfSameCriterion_Throws()
    {
        var criterion = Criterion.Create("K-1", CriterionKind.Requirement);
        var release = CatalogRelease.Create("T.1");
        release.Include(CriterionRevision.Create(criterion, 1, "Titel 1"));

        Should.Throw<ArgumentException>(() => release.Include(CriterionRevision.Create(criterion, 2, "Titel 2")));

        release.Criteria.Count.ShouldBe(1);
    }

    [Fact]
    public void Activate_DraftRelease_Throws()
    {
        var release = CatalogRelease.Create("T.1");

        Should.Throw<InvalidOperationException>(release.Activate);

        release.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void AddStandardReference_SameReferenceTwice_Throws()
    {
        var revision = Revision("K-1");
        var reference = StandardReference.Create(Standard.BitInklusiv, "9.1.1.1");
        revision.AddStandardReference(reference);

        Should.Throw<ArgumentException>(() => revision.AddStandardReference(reference));
    }

    private static CriterionRevision Revision(string key) => ARevision().WithKey(key).Build();

    private static CatalogRelease PublishedRelease(string version, params CriterionRevision[] revisions) =>
        ACatalogRelease().WithVersion(version).Including(revisions).PublishedAt(PublishedAt).Build();
}
