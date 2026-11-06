// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Domain;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Shared.Builders.Catalog.CatalogData;

namespace ProcureAccess.Tests.Unit.Catalog;

/// <summary>
/// <see cref="Criterion.MarkSupersededBy"/> (owner, S3-16 decision 4): the import sets the successor
/// of a stored criterion once; another successor afterwards is a change of published data (CAT-05).
/// </summary>
public class CriterionSuccessorTests
{
    [Fact]
    public void MarkSupersededBy_NoSuccessorYet_SetsIt()
    {
        var criterion = ACriterion().WithKey("99.8").Build();

        criterion.MarkSupersededBy("99.9");
        criterion.MarkSupersededBy("99.9");

        criterion.SupersededByKey.ShouldBe("99.9");
    }

    [Fact(DisplayName = "INV:CAT-05 " + nameof(MarkSupersededBy_OtherSuccessor_ThrowsPublishedCatalogChange))]
    [Trait("Invariant", "CAT-05")]
    public void MarkSupersededBy_OtherSuccessor_ThrowsPublishedCatalogChange()
    {
        var criterion = ACriterion().WithKey("99.8").SupersededBy("99.9").Build();

        Should.Throw<PublishedCatalogChangeException>(() => criterion.MarkSupersededBy("99.10"));

        criterion.SupersededByKey.ShouldBe("99.9");
    }

    [Fact]
    public void MarkSupersededBy_OwnKey_IsRefused()
    {
        var criterion = ACriterion().WithKey("99.8").Build();

        Should.Throw<ArgumentException>(() => criterion.MarkSupersededBy("99.8"));

        criterion.SupersededByKey.ShouldBeNull();
    }
}
