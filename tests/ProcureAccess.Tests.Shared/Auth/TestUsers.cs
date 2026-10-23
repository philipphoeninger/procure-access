// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using MODELS.ProcureAccess.Entities.Authorization;

namespace ProcureAccess.Tests.Shared.Auth;

/// <summary>
/// A named fixture user that <see cref="TestAuthHandler"/> signs in (TESTING §5.2).
/// </summary>
/// <remarks>
/// The user exists only as a principal; there is no row in <c>identity.Users</c>. A test that
/// needs the row (e.g. <c>GET /api/me</c>) stores it itself with the same <see cref="Id"/>.
/// </remarks>
/// <param name="Name">Key sent in the <see cref="TestAuthHandler.UserHeader"/> header.</param>
/// <param name="Id">Value of the <c>NameIdentifier</c> claim.</param>
/// <param name="Email">Value of the <c>Email</c> claim.</param>
/// <param name="Roles">Role claims.</param>
/// <param name="Permissions">Permission claims (<see cref="CustomClaimTypes.Permission"/>).</param>
public sealed record TestUser(
    string Name,
    string Id,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

/// <summary>
/// The fixture users of the current (legacy) role model. The Tenants fixture of TESTING §6.2
/// (organisations A and B, Owner/Editor/Reader …) replaces them in migration step 3.
/// </summary>
public static class TestUsers
{
    /// <summary>A signed-in user with the <c>Member</c> role and no permissions.</summary>
    public static readonly TestUser Member = new(
        Name: "member",
        Id: "00000000-0000-0000-0000-00000000a001",
        Email: "member@example.test",
        Roles: [Roles.Member],
        Permissions: []);

    /// <summary>A signed-in user with the <c>Approver</c> role and its permission, as granted by the role claims.</summary>
    public static readonly TestUser Approver = new(
        Name: "approver",
        Id: "00000000-0000-0000-0000-00000000a002",
        Email: "approver@example.test",
        Roles: [Roles.Member, Roles.Approver],
        Permissions: [MODELS.ProcureAccess.Entities.Authorization.Permissions.ObjectsApprove]);

    /// <summary>All fixture users, looked up by <see cref="TestUser.Name"/>.</summary>
    public static readonly IReadOnlyDictionary<string, TestUser> ByName =
        new[] { Member, Approver }.ToDictionary(u => u.Name, StringComparer.Ordinal);
}
