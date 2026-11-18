// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;

/// <summary>
/// Organisations and their members (SPEC R-ORG-01, R-ORG-03, R-ORG-04; ARCHITECTURE §5.2, C80;
/// S5-16a). The organisation endpoints call it and add the members' e-mail addresses
/// (owner decision S5-16 4a). It applies the organisation
/// rules — the creator becomes Owner, the last Owner cannot be demoted, removed or leave (TEN-11) —
/// but never decides who may call it: the caller checks <see cref="OrganizationRoleRequirement"/>
/// on the organisation first. Reads and changes go through the organisation query filter, so an
/// organisation the request's user is no member of does not exist here (TEN-01).
/// </summary>
public interface IOrganizationMembers
{
    /// <summary>
    /// Creates an organisation with <paramref name="creatorUserId"/> as its accepted Owner
    /// (R-ORG-01) and returns its id. The name must be trimmed, non-empty and at most
    /// <see cref="OrganizationLimits.NameMaxLength"/> long; the caller validates it.
    /// </summary>
    Task<Guid> CreateAsync(string creatorUserId, string name, OrganizationType type, CancellationToken cancellationToken);

    /// <summary>
    /// The organisation and its members, Owners first, then Editors and Readers; <c>null</c> when
    /// the organisation does not exist for the request.
    /// </summary>
    Task<OrganizationMembers?> GetAsync(Guid organizationId, CancellationToken cancellationToken);

    /// <summary>
    /// The organisations of <paramref name="userId"/> with the user's role in each, by name (German
    /// collation, then id): the header's organisation display and switch (R-SHL-02, R-SHL-03; S5-18).
    /// Empty for a user without membership.
    /// </summary>
    Task<IReadOnlyList<OrganizationSummary>> ListOfAsync(string userId, CancellationToken cancellationToken);

    /// <summary>Gives the member <paramref name="userId"/> the role <paramref name="role"/> (R-ORG-03).</summary>
    Task<MembershipChangeResult> ChangeRoleAsync(
        Guid organizationId, string userId, OrganizationRole role, CancellationToken cancellationToken);

    /// <summary>Ends the membership of <paramref name="userId"/>: removal by an Owner or leaving (R-ORG-03, R-ORG-04).</summary>
    Task<MembershipChangeResult> RemoveAsync(Guid organizationId, string userId, CancellationToken cancellationToken);
}

/// <summary>Field limits of an organisation (SPEC §7.2).</summary>
public static class OrganizationLimits
{
    /// <summary>Longest name, counted in UTF-16 code units like <see cref="string.Length"/>.</summary>
    public const int NameMaxLength = 120;
}

/// <summary>An organisation with its members, as the members page S21 shows it.</summary>
public sealed record OrganizationMembers(Guid Id, string Name, OrganizationType Type, IReadOnlyList<OrganizationMember> Members);

/// <summary>An organisation of the signed-in user with the user's role in it, as the header lists it.</summary>
public sealed record OrganizationSummary(Guid Id, string Name, OrganizationType Type, OrganizationRole Role);

/// <summary>One member: the ASP.NET Core Identity user id and the role.</summary>
public sealed record OrganizationMember(string UserId, OrganizationRole Role);

/// <summary>The outcome of a role change or a removal.</summary>
public enum MembershipChangeResult
{
    /// <summary>Done (also when the role was already the requested one).</summary>
    Done = 1,

    /// <summary>The organisation or the membership does not exist for the request (404).</summary>
    NotFound = 2,

    /// <summary>Refused: the organisation would have no Owner left (409 <c>last-owner</c>, TEN-11).</summary>
    LastOwner = 3,
}
