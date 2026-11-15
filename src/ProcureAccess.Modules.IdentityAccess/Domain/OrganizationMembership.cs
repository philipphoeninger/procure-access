// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;

namespace ProcureAccess.Modules.IdentityAccess.Domain;

/// <summary>
/// A user's membership in an organisation with its role (ARCHITECTURE §5.2, §9.1): one row per
/// organisation and user. The rows of an organisation are visible only to its members (TEN-01);
/// the loader of the current user's own memberships is the one call site that reads them unfiltered
/// (TEN-02). Role changes, removal and the last-Owner rule come with S5-16.
/// </summary>
internal sealed class OrganizationMembership
{
    private OrganizationMembership()
    {
    }

    public Guid OrganizationId { get; private set; }

    /// <summary>The ASP.NET Core Identity user id; no foreign key while users are legacy (ADR-014).</summary>
    public string UserId { get; private set; } = null!;

    public OrganizationRole Role { get; private set; }

    /// <summary>When the user was invited; for the creator the creation time.</summary>
    public DateTimeOffset InvitedAt { get; private set; }

    /// <summary>When the user accepted; set at once for the creator, by the accept flow of S5-17 otherwise.</summary>
    public DateTimeOffset? AcceptedAt { get; private set; }

    /// <summary>
    /// The creator's Owner membership (R-ORG-01): invited and accepted at <paramref name="createdAt"/>.
    /// </summary>
    public static OrganizationMembership Founder(Organization organization, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(organization);
        return Create(organization.Id, organization.CreatedByUserId, OrganizationRole.Owner, createdAt, createdAt);
    }

    /// <summary>
    /// A membership with <paramref name="role"/>. Throws for an empty organisation or user id, an
    /// undefined role or an acceptance before the invitation.
    /// </summary>
    public static OrganizationMembership Create(
        Guid organizationId, string userId, OrganizationRole role, DateTimeOffset invitedAt, DateTimeOffset? acceptedAt)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("A membership needs its organisation.", nameof(organizationId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown organisation role.");
        }

        if (acceptedAt < invitedAt)
        {
            throw new ArgumentException("A membership is not accepted before it is offered.", nameof(acceptedAt));
        }

        return new OrganizationMembership
        {
            OrganizationId = organizationId,
            UserId = userId,
            Role = role,
            InvitedAt = invitedAt,
            AcceptedAt = acceptedAt,
        };
    }
}
