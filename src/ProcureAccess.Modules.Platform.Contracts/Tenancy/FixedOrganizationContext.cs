// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.Tenancy;

/// <summary>
/// An <see cref="IOrganizationContext"/> with a fixed set: <see cref="None"/> for a context built
/// outside a request (design-time factories, command hosts, test fixtures), <see cref="Of"/> for a
/// test that reads as a member of given organisations.
/// </summary>
public sealed class FixedOrganizationContext : IOrganizationContext
{
    /// <summary>No organisation: every scoped query returns nothing.</summary>
    public static readonly FixedOrganizationContext None = new(new HashSet<Guid>());

    private FixedOrganizationContext(IReadOnlySet<Guid> organizationIds)
    {
        OrganizationIds = organizationIds;
    }

    public IReadOnlySet<Guid> OrganizationIds { get; }

    /// <summary>A context that sees exactly <paramref name="organizationIds"/>.</summary>
    public static FixedOrganizationContext Of(params IEnumerable<Guid> organizationIds) =>
        new(organizationIds.ToHashSet());
}
