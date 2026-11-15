// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;

/// <summary>
/// Invitations into an organisation by e-mail (SPEC R-ORG-02, R-ORG-03; ARCHITECTURE §5.2, §9.1,
/// C72, C80; S5-17a). The host's invitation endpoints call it, because the invited and the signed-in
/// addresses come from the legacy users (until M3.1). An invitation is bound to the invited address,
/// carries the role chosen on invitation, is valid for the 🔧 <see cref="Lifetime"/> and accepted at
/// most once; only the hash of the link's token is stored (SEC-11).
/// </summary>
/// <remarks>
/// Like <see cref="IOrganizationMembers"/> it never decides who may call it: the caller checks
/// <see cref="OrganizationRoleRequirement.Owner"/> on the organisation before inviting, listing or
/// revoking. Those three go through the organisation query filter (TEN-01). The two calls by token
/// read the invitation past it, because the invitee is no member yet; the link's token is the
/// credential there.
/// </remarks>
public interface IOrganizationInvitations
{
    /// <summary>The 🔧 lifetime of an invitation (14 days), for the e-mail and the answer.</summary>
    TimeSpan Lifetime { get; }

    /// <summary>
    /// Invites <paramref name="email"/> (trimmed, a valid address the caller checked) with
    /// <paramref name="role"/>. An open invitation of the same organisation for the same address is
    /// replaced, so its link stops working (owner decision S5-17 D3 (a)). <c>null</c> when the
    /// organisation does not exist for the request.
    /// </summary>
    Task<IssuedInvitation?> InviteAsync(Guid organizationId, string email, OrganizationRole role, CancellationToken cancellationToken);

    /// <summary>
    /// The open invitations of the organisation (not accepted, not expired) by address; <c>null</c>
    /// when the organisation does not exist for the request.
    /// </summary>
    Task<IReadOnlyList<PendingInvitation>?> GetPendingAsync(Guid organizationId, CancellationToken cancellationToken);

    /// <summary>Deletes the open invitation <paramref name="invitationId"/>; false when there is none (R-ORG-03).</summary>
    Task<bool> RevokeAsync(Guid organizationId, Guid invitationId, CancellationToken cancellationToken);

    /// <summary>
    /// What the link of <paramref name="token"/> invites to, for the page that opens it; the
    /// signed-in user's <paramref name="callerEmail"/> (<c>null</c> when anonymous) decides
    /// <see cref="InvitationDetails.AddressMatches"/>. <c>null</c> for an unknown, expired, revoked or
    /// replaced token, and for an accepted one unless the request's user is a member of its
    /// organisation.
    /// </summary>
    Task<InvitationDetails?> FindAsync(string? token, string? callerEmail, CancellationToken cancellationToken);

    /// <summary>
    /// Accepts the invitation of <paramref name="token"/> for the user <paramref name="userId"/>
    /// with the address <paramref name="userEmail"/>: creates the membership with the invitation's
    /// role and marks the invitation accepted, under the organisation's row lock. The caller has
    /// checked the confirmed address (SEC-10).
    /// </summary>
    Task<InvitationAcceptance> AcceptAsync(string? token, string userId, string userEmail, CancellationToken cancellationToken);
}

/// <summary>A new invitation: its id, the value for the link (never stored) and what the e-mail names.</summary>
/// <param name="Id">The invitation's id, for revoking it.</param>
/// <param name="OrganizationName">The name of the organisation (<c>{{org}}</c> of the e-mail).</param>
/// <param name="Token">The value for the link; URL-safe.</param>
/// <param name="ExpiresAt">When it stops being valid.</param>
/// <param name="Lifetime">The 🔧 lifetime, for <c>{{days}}</c>.</param>
public sealed record IssuedInvitation(Guid Id, string OrganizationName, string Token, DateTimeOffset ExpiresAt, TimeSpan Lifetime)
{
    /// <summary>The lifetime in whole days, rounded up, at least 1 (<c>{{days}}</c> of the texts).</summary>
    public int ValidDays => DaysOf(Lifetime);

    /// <summary>A lifetime in whole days, rounded up, at least 1.</summary>
    public static int DaysOf(TimeSpan lifetime) => Math.Max(1, (int)Math.Ceiling(lifetime.TotalDays));

    /// <summary>Only the type: the token must never reach a log.</summary>
    public override string ToString() => nameof(IssuedInvitation);
}

/// <summary>An open invitation on the members page S21 (<c>org.invite.pending</c>).</summary>
public sealed record PendingInvitation(Guid Id, string Email, OrganizationRole Role, DateTimeOffset ExpiresAt)
{
    /// <summary>Only the type: the address must never reach a log (PRV-11).</summary>
    public override string ToString() => nameof(PendingInvitation);
}

/// <summary>What an invitation link invites to.</summary>
/// <param name="OrganizationId">The organisation.</param>
/// <param name="OrganizationName">Its name (<c>org.invite.accept.text</c>).</param>
/// <param name="Role">The role it grants.</param>
/// <param name="InvitedEmail">The invited address.</param>
/// <param name="AddressMatches"><c>null</c> for an anonymous caller, else whether the caller's address is the invited one.</param>
/// <param name="AlreadyMember">True when the request's user is a member of the organisation (e.g. a second click after accepting).</param>
public sealed record InvitationDetails(
    Guid OrganizationId, string OrganizationName, OrganizationRole Role, string InvitedEmail, bool? AddressMatches, bool AlreadyMember)
{
    /// <summary>Only the type: the address must never reach a log (PRV-11).</summary>
    public override string ToString() => nameof(InvitationDetails);
}

/// <summary>The outcome of <see cref="IOrganizationInvitations.AcceptAsync"/>.</summary>
/// <param name="Outcome">What happened.</param>
/// <param name="OrganizationId">The organisation for <see cref="InvitationAcceptanceOutcome.Accepted"/> and <see cref="InvitationAcceptanceOutcome.AlreadyMember"/>.</param>
/// <param name="InvitedEmail">The invited address for <see cref="InvitationAcceptanceOutcome.AddressMismatch"/> (<c>org.invite.mismatch</c>).</param>
public sealed record InvitationAcceptance(InvitationAcceptanceOutcome Outcome, Guid? OrganizationId = null, string? InvitedEmail = null)
{
    public static InvitationAcceptance Invalid { get; } = new(InvitationAcceptanceOutcome.Invalid);

    /// <summary>Only the outcome: the address must never reach a log (PRV-11).</summary>
    public override string ToString() => $"{nameof(InvitationAcceptance)} {Outcome}";
}

/// <summary>Outcomes of accepting an invitation.</summary>
public enum InvitationAcceptanceOutcome
{
    /// <summary>The user is a member with the invitation's role now.</summary>
    Accepted = 1,

    /// <summary>
    /// The user was a member already: an invitation used before (a second click on the link, owner
    /// decision S5-17 D7 (a)), or an open one, which is marked accepted and changes no role.
    /// </summary>
    AlreadyMember = 2,

    /// <summary>Unknown, expired, revoked, replaced, or accepted by someone else (400 at <c>token</c>).</summary>
    Invalid = 3,

    /// <summary>The invitation is for another address (403 <c>invitation-mismatch</c>); it stays open.</summary>
    AddressMismatch = 4,
}
