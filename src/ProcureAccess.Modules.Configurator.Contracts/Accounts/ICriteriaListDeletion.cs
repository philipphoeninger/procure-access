// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Configurator.Contracts.Accounts;

/// <summary>
/// The Configurator part of the account deletion (SPEC R-AUTH-10; ARCHITECTURE C79; TEN-11;
/// S5-28a): an organisation whose last member deletes the account is deleted with its criteria
/// lists. The host's endpoint <c>DELETE /api/v1/account</c> calls it with the organisations
/// IdentityAccess named, before their memberships go.
/// </summary>
/// <remarks>
/// Deleted through the organisation query filter of the request (TEN-01): an organisation the
/// request's user is no member of loses nothing, whatever id is passed.
/// </remarks>
public interface ICriteriaListDeletion
{
    /// <summary>Deletes every criteria list of these organisations with its rows.</summary>
    Task DeleteOfOrganizationsAsync(IReadOnlyCollection<Guid> organizationIds, CancellationToken cancellationToken);
}
