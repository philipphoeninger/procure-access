// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using MODELS.ProcureAccess.Entities.Authorization;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

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
/// The fixture users: the Tenants fixture of TESTING §6.2 (S5-15) — Owner, Editor and Reader of
/// organisations A and B, whose memberships <c>Tenants.Build</c> stores — the user without
/// organisation (<see cref="Member"/>) and the platform-role user <see cref="Moderator"/>.
/// A user exists only as a principal (no row in <c>identity.Users</c>); the memberships refer to
/// its <see cref="TestUser.Id"/>.
/// </summary>
public static class TestUsers
{
    /// <summary>
    /// A signed-in user without roles, permissions or organisation membership — the normal state of
    /// a contributor (ARCHITECTURE §5.2). The name stays from the legacy global <c>Member</c> role,
    /// which is gone since S5-12a (O-S5-3 (a)).
    /// </summary>
    public static readonly TestUser Member = new(
        Name: "member",
        Id: "00000000-0000-0000-0000-00000000a001",
        Email: "member@example.test",
        Roles: [],
        Permissions: []);

    /// <summary>Owner of organisation A (Tenants fixture).</summary>
    public static readonly TestUser OwnerA = Tenant("owner-a", "0a01");

    /// <summary>Editor of organisation A.</summary>
    public static readonly TestUser EditorA = Tenant("editor-a", "0a02");

    /// <summary>Reader of organisation A.</summary>
    public static readonly TestUser ReaderA = Tenant("reader-a", "0a03");

    /// <summary>Owner of organisation B.</summary>
    public static readonly TestUser OwnerB = Tenant("owner-b", "0b01");

    /// <summary>Editor of organisation B.</summary>
    public static readonly TestUser EditorB = Tenant("editor-b", "0b02");

    /// <summary>Reader of organisation B.</summary>
    public static readonly TestUser ReaderB = Tenant("reader-b", "0b03");

    /// <summary>
    /// A signed-in user with the <c>Moderator</c> role and its permission, as granted by the role
    /// claims (formerly <c>Approver</c>, S4-25).
    /// </summary>
    public static readonly TestUser Moderator = new(
        Name: "moderator",
        Id: "00000000-0000-0000-0000-00000000a002",
        Email: "moderator@example.test",
        Roles: [PlatformRoles.Moderator],
        Permissions: [MODELS.ProcureAccess.Entities.Authorization.Permissions.ModerationReview]);

    /// <summary>All fixture users, looked up by <see cref="TestUser.Name"/>.</summary>
    public static readonly IReadOnlyDictionary<string, TestUser> ByName =
        new[] { Member, Moderator, OwnerA, EditorA, ReaderA, OwnerB, EditorB, ReaderB }
            .ToDictionary(u => u.Name, StringComparer.Ordinal);

    /// <summary>A tenant user without roles or permissions; its rights come from its membership alone.</summary>
    private static TestUser Tenant(string name, string idSuffix) => new(
        Name: name,
        Id: $"00000000-0000-0000-0000-00000000{idSuffix}",
        Email: $"{name}@example.test",
        Roles: [],
        Permissions: []);
}
