// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Domain;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.IdentityAccess;

/// <summary>
/// <see cref="ContributorProfile.Create"/> and <see cref="UserContributorLink.Create"/> (S4-25,
/// ARCHITECTURE §5.2, SPEC §7.2). Uniqueness of the display name is the database's
/// (<c>ContributorProfilePersistenceTests</c>).
/// </summary>
public class ContributorProfileTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ValidValues_KeepsThemWithANewId()
    {
        var first = ContributorProfile.Create("Leselupe", ContributorKind.Individual, Now);
        var second = ContributorProfile.Create("Leselupe", ContributorKind.Individual, Now);

        first.DisplayName.ShouldBe("Leselupe");
        first.Kind.ShouldBe(ContributorKind.Individual);
        first.CreatedAt.ShouldBe(Now);
        first.Id.ShouldNotBe(Guid.Empty);
        second.Id.ShouldNotBe(first.Id);
    }

    [Fact]
    public void Create_DisplayNameOf60Characters_IsAccepted() =>
        ContributorProfile.Create(new string('a', 60), ContributorKind.Vendor, Now).DisplayName.Length.ShouldBe(60);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyDisplayName_Throws(string displayName) =>
        Should.Throw<ArgumentException>(() => ContributorProfile.Create(displayName, ContributorKind.Auditor, Now));

    [Fact]
    public void Create_DisplayNameOf61Characters_Throws() =>
        Should.Throw<ArgumentException>(() => ContributorProfile.Create(new string('a', 61), ContributorKind.Institution, Now))
            .ParamName.ShouldBe("displayName");

    [Fact]
    public void Create_UndefinedKind_Throws() =>
        Should.Throw<ArgumentOutOfRangeException>(() => ContributorProfile.Create("Leselupe", (ContributorKind)99, Now));

    [Fact]
    public void LinkCreate_UserAndProfile_PointsAtTheProfile()
    {
        var profile = ContributorProfile.Create("Leselupe", ContributorKind.Individual, Now);

        var link = UserContributorLink.Create("user-1", profile);

        link.UserId.ShouldBe("user-1");
        link.ContributorProfileId.ShouldBe(profile.Id);
    }

    [Fact]
    public void LinkCreate_EmptyUserId_Throws() =>
        Should.Throw<ArgumentException>(() =>
            UserContributorLink.Create(" ", ContributorProfile.Create("Leselupe", ContributorKind.Individual, Now)));
}
