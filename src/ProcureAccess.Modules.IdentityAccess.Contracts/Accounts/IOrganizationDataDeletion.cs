// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

/// <summary>
/// A module's part of the account deletion (SPEC R-AUTH-10; ARCHITECTURE C79; TEN-11; S5-28a): an
/// organisation whose last member deletes the account is deleted with everything a module holds
/// for it. Declared here and implemented by each module with organisation data (S6-10; the
/// Configurator's criteria lists first): <c>DELETE /api/v1/account</c> belongs to IdentityAccess,
/// which depends on no module (ARCHITECTURE §4.3), so it calls every registered implementation
/// with the organisations that end, before their memberships go.
/// </summary>
/// <remarks>
/// Delete through the organisation query filter of the request (TEN-01): an organisation the
/// request's user is no member of loses nothing, whatever id is passed. The modules share no
/// transaction (ADR-014), so an implementation must be repeatable: the deletion may run again
/// after a failure in a later step.
/// </remarks>
public interface IOrganizationDataDeletion
{
    /// <summary>Deletes the module's data of these organisations.</summary>
    Task DeleteOfOrganizationsAsync(IReadOnlyCollection<Guid> organizationIds, CancellationToken cancellationToken);
}
