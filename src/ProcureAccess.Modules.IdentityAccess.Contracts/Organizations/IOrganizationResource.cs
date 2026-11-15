// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;

/// <summary>
/// A resource that belongs to one organisation (ARCHITECTURE §5.4). The resource of a
/// <see cref="OrganizationRoleRequirement"/> check: the organisation comes from the resource —
/// a loaded entity or, for an endpoint with the organisation in its route, an
/// <see cref="OrganizationReference"/> — never from a client-supplied header (TEN-05).
/// </summary>
public interface IOrganizationResource
{
    Guid OrganizationId { get; }
}

/// <summary>The organisation of a route (<c>/organizations/{orgId}/…</c>) as a resource.</summary>
public sealed record OrganizationReference(Guid OrganizationId) : IOrganizationResource;
