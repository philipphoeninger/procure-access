// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Tenancy;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.IdentityAccess;

/// <summary>
/// <see cref="OrganizationRoleHandler"/> on the memberships of a request and
/// <see cref="OrganizationAccess.StatusOf"/> (S5-15, ARCHITECTURE §9.1): the requirement, the
/// resource and the loaded memberships decide; nothing the client sends is read.
/// </summary>
public class OrganizationRoleHandlerTests
{
    private static readonly Guid OrganizationA = Guid.NewGuid();
    private static readonly Guid OrganizationB = Guid.NewGuid();

    [Theory]
    [InlineData(OrganizationRole.Reader, OrganizationRole.Reader)]
    [InlineData(OrganizationRole.Editor, OrganizationRole.Reader)]
    [InlineData(OrganizationRole.Owner, OrganizationRole.Editor)]
    [InlineData(OrganizationRole.Owner, OrganizationRole.Owner)]
    public async Task Handle_MemberWithAtLeastTheMinimum_Succeeds(OrganizationRole role, OrganizationRole minimum)
    {
        var context = await HandleAsync(MemberOf(OrganizationA, role), OrganizationRoleRequirement.For(minimum), OrganizationA);

        context.HasSucceeded.ShouldBeTrue();
        OrganizationAccess.StatusOf(ResultOf(context)).ShouldBeNull();
    }

    [Theory]
    [InlineData(OrganizationRole.Reader, OrganizationRole.Editor)]
    [InlineData(OrganizationRole.Reader, OrganizationRole.Owner)]
    [InlineData(OrganizationRole.Editor, OrganizationRole.Owner)]
    public async Task Handle_MemberBelowTheMinimum_FailsWithInsufficientRole_Status403(OrganizationRole role, OrganizationRole minimum)
    {
        var context = await HandleAsync(MemberOf(OrganizationA, role), OrganizationRoleRequirement.For(minimum), OrganizationA);

        context.HasFailed.ShouldBeTrue();
        context.FailureReasons.Select(r => r.Message).ShouldBe([OrganizationAccess.InsufficientRole]);
        OrganizationAccess.StatusOf(ResultOf(context)).ShouldBe(403);
    }

    [Fact]
    public async Task Handle_MemberOfAnotherOrganization_FailsWithNotAMember_Status404()
    {
        var context = await HandleAsync(MemberOf(OrganizationB, OrganizationRole.Owner), OrganizationRoleRequirement.Reader, OrganizationA);

        context.HasFailed.ShouldBeTrue();
        context.FailureReasons.Select(r => r.Message).ShouldBe([OrganizationAccess.NotAMember]);
        OrganizationAccess.StatusOf(ResultOf(context)).ShouldBe(404);
    }

    [Fact]
    public async Task Handle_UserWithoutOrganization_FailsWithNotAMember()
    {
        var organizations = new OrganizationContext();
        organizations.Load([]);

        var context = await HandleAsync(organizations, OrganizationRoleRequirement.Reader, OrganizationA, SignedIn());

        context.HasFailed.ShouldBeTrue();
        context.FailureReasons.Select(r => r.Message).ShouldBe([OrganizationAccess.NotAMember]);
    }

    [Fact]
    public async Task Handle_Anonymous_DecidesNothing_Status404()
    {
        var context = await HandleAsync(MemberOf(OrganizationA, OrganizationRole.Owner), OrganizationRoleRequirement.Reader, OrganizationA, Anonymous());

        context.HasSucceeded.ShouldBeFalse();
        context.HasFailed.ShouldBeFalse();
        OrganizationAccess.StatusOf(ResultOf(context)).ShouldBe(404);
    }

    [Fact]
    public void StatusOf_Success_IsNull() => OrganizationAccess.StatusOf(AuthorizationResult.Success()).ShouldBeNull();

    [Fact]
    public void StatusOf_FailureWithoutReason_Is404() =>
        OrganizationAccess.StatusOf(AuthorizationResult.Failed()).ShouldBe(404);

    [Fact]
    public void For_EveryRole_ReturnsTheSharedInstance()
    {
        OrganizationRoleRequirement.For(OrganizationRole.Reader).ShouldBeSameAs(OrganizationRoleRequirement.Reader);
        OrganizationRoleRequirement.For(OrganizationRole.Editor).ShouldBeSameAs(OrganizationRoleRequirement.Editor);
        OrganizationRoleRequirement.For(OrganizationRole.Owner).ShouldBeSameAs(OrganizationRoleRequirement.Owner);
        Should.Throw<ArgumentOutOfRangeException>(() => OrganizationRoleRequirement.For((OrganizationRole)0));
    }

    [Fact]
    public void Load_Memberships_SetsIdsAndRoles()
    {
        var organizations = new OrganizationContext();
        organizations.IsLoaded.ShouldBeFalse();

        organizations.Load([new(OrganizationA, OrganizationRole.Editor), new(OrganizationB, OrganizationRole.Reader)]);

        organizations.IsLoaded.ShouldBeTrue();
        organizations.OrganizationIds.ShouldBe([OrganizationA, OrganizationB], ignoreOrder: true);
        organizations.RoleIn(OrganizationA).ShouldBe(OrganizationRole.Editor);
        organizations.RoleIn(Guid.NewGuid()).ShouldBeNull();
    }

    private static OrganizationContext MemberOf(Guid organizationId, OrganizationRole role)
    {
        var organizations = new OrganizationContext();
        organizations.Load([new(organizationId, role)]);
        return organizations;
    }

    private static async Task<AuthorizationHandlerContext> HandleAsync(
        OrganizationContext organizations, OrganizationRoleRequirement requirement, Guid organizationId, ClaimsPrincipal? user = null)
    {
        var context = new AuthorizationHandlerContext([requirement], user ?? SignedIn(), new OrganizationReference(organizationId));
        await new OrganizationRoleHandler(organizations).HandleAsync(context);
        return context;
    }

    private static AuthorizationResult ResultOf(AuthorizationHandlerContext context) =>
        context.HasSucceeded
            ? AuthorizationResult.Success()
            : AuthorizationResult.Failed(AuthorizationFailure.Failed(context.FailureReasons));

    private static ClaimsPrincipal SignedIn() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], "Test"));

    private static ClaimsPrincipal Anonymous() => new(new ClaimsIdentity());
}
