// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.Tenancy;

/// <summary>
/// The organisations the current request may see: the ones the signed-in user is a member of
/// (ARCHITECTURE §5.4, ADR-007; S5-15). Every module context binds the global query filter of its
/// organisation-scoped entities to it, so a row of another organisation is invisible and a foreign
/// id answers 404 by construction (TEN-01). Declared here, because every module may use
/// <c>Platform.Contracts</c> and Platform's own share links are scoped too (C59); implemented by the
/// IdentityAccess module, which owns the memberships and fills it once per signed-in request.
/// </summary>
/// <remarks>
/// Never resolved from a client-supplied header or query parameter. Outside a request (command
/// hosts, migrations, background jobs) and for an anonymous request the set is empty, so a scoped
/// query returns nothing; a call site that must read across organisations uses
/// <c>IgnoreQueryFilters()</c> and is allow-listed (TEN-02).
/// </remarks>
public interface IOrganizationContext
{
    /// <summary>The ids of the organisations of the current principal's memberships; empty without any.</summary>
    IReadOnlySet<Guid> OrganizationIds { get; }
}
