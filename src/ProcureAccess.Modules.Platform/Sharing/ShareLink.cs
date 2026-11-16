// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Cryptography;
using ProcureAccess.Modules.Platform.Contracts.Sharing;

namespace ProcureAccess.Modules.Platform.Sharing;

/// <summary>
/// A read-only link to one resource of an organisation (ARCHITECTURE §5.3, C59, C81; SPEC R-SAV-04,
/// SD15; S5-23). Only the SHA-256 hash of the token is stored, so the table alone never yields a
/// usable link (TEN-08). Scoped by its organisation for the management (TEN-01); the read by token
/// is the allow-listed unfiltered call (TEN-02), because the reader has no account. The expiry is
/// required (TEN-12); a revocation ends the link before it.
/// </summary>
/// <remarks>
/// No <c>Scope</c> column: v1 links only read (owner decision S5-23 6). No creator: S15 lists the
/// links without one, and the table then holds no personal reference.
/// </remarks>
internal sealed class ShareLink
{
    /// <summary>Length of <see cref="TokenHash"/>: a SHA-256 hash.</summary>
    public const int HashLength = SHA256.HashSizeInBytes;

    private ShareLink()
    {
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    /// <summary>SHA-256 of the token's bytes.</summary>
    public byte[] TokenHash { get; private set; } = null!;

    public ShareableResourceType ResourceType { get; private set; }

    public Guid ResourceId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>The first instant the link no longer works: the start of the day after the chosen date.</summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>When it was revoked; null while it was not.</summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>
    /// Creates a link. <paramref name="createdAt"/> comes from <see cref="TimeProvider"/>
    /// (CLAUDE.md §7). Throws for an empty organisation or resource, an undefined resource type, a
    /// hash of the wrong length or an expiry that is not after the creation.
    /// </summary>
    public static ShareLink Create(
        Guid organizationId,
        ShareableResourceType resourceType,
        Guid resourceId,
        byte[] tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("A share link needs its organisation.", nameof(organizationId));
        }

        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException("A share link needs its resource.", nameof(resourceId));
        }

        if (!Enum.IsDefined(resourceType))
        {
            throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, "Unknown resource type.");
        }

        ArgumentNullException.ThrowIfNull(tokenHash);
        if (tokenHash.Length != HashLength)
        {
            throw new ArgumentException($"A token hash has {HashLength} bytes.", nameof(tokenHash));
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiresAt, createdAt);

        return new ShareLink
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ResourceType = resourceType,
            ResourceId = resourceId,
            TokenHash = tokenHash,
            CreatedAt = createdAt,
            ExpiresAt = expiresAt,
        };
    }

    /// <summary>Whether the link still opens its resource at <paramref name="now"/>.</summary>
    public bool IsActiveAt(DateTimeOffset now) => RevokedAt is null && now < ExpiresAt;

    /// <summary>Ends the link at <paramref name="now"/>; a revoked link keeps its first revocation.</summary>
    public void Revoke(DateTimeOffset now) => RevokedAt ??= now;
}
