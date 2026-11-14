// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

/// <summary>
/// Names of the authorization policies that other modules attach to their endpoints
/// (<c>RequireAuthorization(&lt;name&gt;)</c>; ARCHITECTURE §9.1).
/// </summary>
public static class IdentityAccessPolicies
{
    /// <summary>
    /// The e-mail confirmation gate (C72, SEC-10; SPEC R-AUTH-02, SD13): signed in and the address
    /// confirmed. Attach it to saving a list, creating an organisation, accepting an invitation,
    /// creating a contributor profile and every publication path. An unconfirmed account gets 403
    /// Problem Details <c>email-unconfirmed</c>, an anonymous request 401.
    /// </summary>
    public const string ConfirmedEmail = "ConfirmedEmail";
}
