// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.IdentityAccess;

/// <summary>
/// <see cref="Organization.Create"/>, <see cref="OrganizationMembership.Create"/> and
/// <see cref="OrganizationMembership.Founder"/> (S5-15, ARCHITECTURE §5.2, SPEC §7.2).
/// </summary>
public class OrganizationTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ValidValues_KeepsThemWithANewRandomId()
    {
        var first = Organization.Create("Stadt Musterhausen", OrganizationType.PublicBody, "user-1", Now);
        var second = Organization.Create("Stadt Musterhausen", OrganizationType.PublicBody, "user-1", Now);

        first.Name.ShouldBe("Stadt Musterhausen");
        first.Type.ShouldBe(OrganizationType.PublicBody);
        first.CreatedByUserId.ShouldBe("user-1");
        first.CreatedAt.ShouldBe(Now);
        first.Id.ShouldNotBe(Guid.Empty);
        second.Id.ShouldNotBe(first.Id);
    }

    [Fact]
    public void Create_NameOf120Characters_IsAccepted() =>
        Organization.Create(new string('a', 120), OrganizationType.Company, "user-1", Now).Name.Length.ShouldBe(120);

    [Fact]
    public void Create_NameOf121Characters_Throws() =>
        Should.Throw<ArgumentException>(() => Organization.Create(new string('a', 121), OrganizationType.Company, "user-1", Now))
              .ParamName.ShouldBe("name");

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_Throws(string name) =>
        Should.Throw<ArgumentException>(() => Organization.Create(name, OrganizationType.Association, "user-1", Now));

    [Fact]
    public void Create_UndefinedType_Throws() =>
        Should.Throw<ArgumentOutOfRangeException>(() => Organization.Create("Verein", (OrganizationType)42, "user-1", Now));

    [Fact]
    public void Create_EmptyCreator_Throws() =>
        Should.Throw<ArgumentException>(() => Organization.Create("Verein", OrganizationType.Association, " ", Now))
              .ParamName.ShouldBe("createdByUserId");

    [Fact]
    public void Founder_Organization_IsTheCreatorsAcceptedOwnerMembership()
    {
        var organization = Organization.Create("Verein", OrganizationType.Association, "user-1", Now);

        var membership = OrganizationMembership.Founder(organization, Now);

        membership.OrganizationId.ShouldBe(organization.Id);
        membership.UserId.ShouldBe("user-1");
        membership.Role.ShouldBe(OrganizationRole.Owner);
        membership.InvitedAt.ShouldBe(Now);
        membership.AcceptedAt.ShouldBe(Now);
    }

    [Fact]
    public void Create_PendingMembership_HasNoAcceptance()
    {
        var membership = OrganizationMembership.Create(Guid.NewGuid(), "user-2", OrganizationRole.Reader, Now, null);

        membership.Role.ShouldBe(OrganizationRole.Reader);
        membership.AcceptedAt.ShouldBeNull();
    }

    [Fact]
    public void Create_EmptyOrganization_Throws() =>
        Should.Throw<ArgumentException>(() => OrganizationMembership.Create(Guid.Empty, "user-2", OrganizationRole.Reader, Now, Now))
              .ParamName.ShouldBe("organizationId");

    [Fact]
    public void Create_EmptyUser_Throws() =>
        Should.Throw<ArgumentException>(() => OrganizationMembership.Create(Guid.NewGuid(), "", OrganizationRole.Reader, Now, Now));

    [Fact]
    public void Create_UndefinedRole_Throws() =>
        Should.Throw<ArgumentOutOfRangeException>(() =>
            OrganizationMembership.Create(Guid.NewGuid(), "user-2", (OrganizationRole)9, Now, Now));

    [Fact]
    public void Create_AcceptedBeforeInvited_Throws() =>
        Should.Throw<ArgumentException>(() =>
                  OrganizationMembership.Create(Guid.NewGuid(), "user-2", OrganizationRole.Editor, Now, Now.AddMinutes(-1)))
              .ParamName.ShouldBe("acceptedAt");

    [Fact]
    public void OrganizationRole_Order_ReaderBelowEditorBelowOwner()
    {
        (OrganizationRole.Reader < OrganizationRole.Editor).ShouldBeTrue();
        (OrganizationRole.Editor < OrganizationRole.Owner).ShouldBeTrue();
    }

    [Fact]
    public void ChangeRole_DefinedRole_SetsIt()
    {
        var membership = OrganizationMembership.Create(Guid.NewGuid(), "user-2", OrganizationRole.Reader, Now, Now);

        membership.ChangeRole(OrganizationRole.Owner);

        membership.Role.ShouldBe(OrganizationRole.Owner);
    }

    [Fact]
    public void ChangeRole_UndefinedRole_ThrowsAndKeepsTheRole()
    {
        var membership = OrganizationMembership.Create(Guid.NewGuid(), "user-2", OrganizationRole.Editor, Now, Now);

        Should.Throw<ArgumentOutOfRangeException>(() => membership.ChangeRole((OrganizationRole)7));

        membership.Role.ShouldBe(OrganizationRole.Editor);
    }

    [Theory(DisplayName = "INV:TEN-11 " + nameof(LeavesNoOwner_RoleChangeOrRemoval_IsTrueOnlyForTheLastOwner))]
    [Trait("Invariant", "TEN-11")]
    // The last Owner: demoting or removing leaves none.
    [InlineData(OrganizationRole.Owner, OrganizationRole.Editor, 1, true)]
    [InlineData(OrganizationRole.Owner, OrganizationRole.Reader, 1, true)]
    [InlineData(OrganizationRole.Owner, null, 1, true)]
    // The last Owner stays Owner: nothing changes.
    [InlineData(OrganizationRole.Owner, OrganizationRole.Owner, 1, false)]
    // Another Owner remains.
    [InlineData(OrganizationRole.Owner, OrganizationRole.Editor, 2, false)]
    [InlineData(OrganizationRole.Owner, null, 2, false)]
    // Members below Owner never take the last Owner away.
    [InlineData(OrganizationRole.Editor, OrganizationRole.Reader, 1, false)]
    [InlineData(OrganizationRole.Reader, null, 1, false)]
    [InlineData(OrganizationRole.Editor, OrganizationRole.Owner, 1, false)]
    public void LeavesNoOwner_RoleChangeOrRemoval_IsTrueOnlyForTheLastOwner(
        OrganizationRole current, OrganizationRole? next, int owners, bool expected) =>
        OrganizationMembership.LeavesNoOwner(current, next, owners).ShouldBe(expected);
}
