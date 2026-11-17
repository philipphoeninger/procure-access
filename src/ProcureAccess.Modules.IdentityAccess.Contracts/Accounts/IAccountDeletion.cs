// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

/// <summary>
/// The IdentityAccess part of the account deletion (SPEC R-AUTH-10; ARCHITECTURE C79; TEN-11;
/// S5-28a): the organisation rules and the rows of one user. The host's endpoint
/// <c>DELETE /api/v1/account</c> calls it for the signed-in user, because the account itself is
/// still a legacy user (until M3.1).
/// </summary>
/// <remarks>
/// <para><b>Two steps, because the modules share no transaction (ADR-014).</b>
/// <see cref="BeginAsync"/> checks the rule and names the organisations that end with the account;
/// the caller deletes what other modules hold for them (criteria lists, share links), then
/// <see cref="CompleteAsync"/> deletes the rows of this module. Every step can be repeated, so a
/// failure in between is finished by the user's next attempt and leaves no rows nobody can reach.</para>
/// <para>Both steps read through the organisation query filter of the request, so they are only
/// complete for the request's own user (TEN-01).</para>
/// </remarks>
public interface IAccountDeletion
{
    /// <summary>
    /// Checks whether <paramref name="userId"/> may delete the account. When the user is the only
    /// Owner of an organisation that has other members, nothing changes and the answer names these
    /// organisations. Otherwise it names the organisations whose only member the user is and
    /// deletes their open invitations, so that nobody joins them before they are deleted.
    /// </summary>
    Task<AccountDeletionStart> BeginAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Checks the rule again and deletes the user's memberships, the organisations whose only member
    /// the user is (with their invitations), the user's settings and account tokens. Returns the
    /// names of the organisations that refuse the deletion; empty when it is done.
    /// </summary>
    Task<IReadOnlyList<string>> CompleteAsync(string userId, CancellationToken cancellationToken);
}

/// <summary>The outcome of <see cref="IAccountDeletion.BeginAsync"/>.</summary>
/// <param name="SoleOwnerOf">
/// The names of the organisations with other members whose only Owner the user is, by name (German
/// collation); empty when the deletion may go on.
/// </param>
/// <param name="OrganizationsToDelete">The organisations whose only member the user is; empty when refused.</param>
public sealed record AccountDeletionStart(IReadOnlyList<string> SoleOwnerOf, IReadOnlyList<Guid> OrganizationsToDelete)
{
    /// <summary>Whether the deletion is refused (409 <c>sole-owner</c>).</summary>
    public bool IsRefused => SoleOwnerOf.Count > 0;
}
