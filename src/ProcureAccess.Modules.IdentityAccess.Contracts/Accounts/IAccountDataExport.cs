// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

/// <summary>
/// The IdentityAccess part of the personal data export (SPEC R-AUTH-09, §8.4; ARCHITECTURE C79;
/// PRV-07; S5-27): the memberships and contributor profiles of one user. The endpoint
/// <c>GET /api/v1/account/export</c> calls it for the signed-in user and adds the account fields.
/// </summary>
/// <remarks>
/// Nothing in the answer names another user or carries an id: no organisation id, no profile id,
/// no <c>UserContributorLink</c> row (PRV-07). The memberships are read through the organisation
/// query filter of the request, so they are only complete for the request's own user (TEN-01).
/// </remarks>
public interface IAccountDataExport
{
    /// <summary>The memberships and contributor profiles of <paramref name="userId"/>.</summary>
    Task<AccountDataPart> GetAsync(string userId, CancellationToken cancellationToken);
}

/// <summary>
/// The user's memberships by organisation name (German collation) and contributor profiles by
/// creation time.
/// </summary>
public sealed record AccountDataPart(
    IReadOnlyList<ExportedMembership> Memberships,
    IReadOnlyList<ExportedContributorProfile> ContributorProfiles);

/// <summary>A membership: the organisation's name, the user's role and since when the user is a member.</summary>
public sealed record ExportedMembership(string Organization, OrganizationRole Role, DateTimeOffset Since);

/// <summary>
/// A contributor profile of the user: display name, kind as SPEC §8.4 names it
/// (<c>individual</c>, <c>institution</c>, <c>vendor</c>, <c>auditor</c>) and creation time.
/// </summary>
public sealed record ExportedContributorProfile(string DisplayName, string Kind, DateTimeOffset CreatedAt);
