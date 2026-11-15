// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;

namespace ProcureAccess.Modules.IdentityAccess.Tenancy;

/// <summary>
/// The memberships of the current request's user (ARCHITECTURE §5.4, §9.1; S5-15), scoped per
/// request: <see cref="OrganizationContextMiddleware"/> loads them once after authentication, the
/// query filters read <see cref="OrganizationIds"/> and <see cref="OrganizationRoleHandler"/> reads
/// <see cref="RoleIn"/>. Outside a request nothing loads it, so it stays empty.
/// </summary>
internal sealed class OrganizationContext : IOrganizationContext
{
    private readonly Dictionary<Guid, OrganizationRole> _roles = [];

    public IReadOnlySet<Guid> OrganizationIds { get; private set; } = new HashSet<Guid>();

    /// <summary>True once the middleware ran for this request (also for an anonymous one).</summary>
    public bool IsLoaded { get; private set; }

    /// <summary>The user's role in <paramref name="organizationId"/>, or <c>null</c> for a non-member.</summary>
    public OrganizationRole? RoleIn(Guid organizationId) =>
        _roles.TryGetValue(organizationId, out var role) ? role : null;

    /// <summary>Sets the memberships of the request. Called once by the middleware; tests call it directly.</summary>
    public void Load(IEnumerable<KeyValuePair<Guid, OrganizationRole>> memberships)
    {
        ArgumentNullException.ThrowIfNull(memberships);
        _roles.Clear();
        foreach (var (organizationId, role) in memberships)
        {
            _roles[organizationId] = role;
        }

        OrganizationIds = _roles.Keys.ToHashSet();
        IsLoaded = true;
    }
}
