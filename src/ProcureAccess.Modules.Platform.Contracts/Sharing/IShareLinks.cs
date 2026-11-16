// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.Sharing;

/// <summary>
/// The management of share links (ARCHITECTURE §5.3, C59, C81; SPEC R-SAV-04, SD15) for the module
/// that owns the shared resource: it checks the role on the resource's organisation first, then
/// creates, lists or revokes through this service. Every call runs through the organisation filter
/// of the request (TEN-01): a link of another organisation does not exist for the caller.
/// </summary>
public interface IShareLinks
{
    /// <summary>
    /// A new link to the resource, valid until the end of <paramref name="expiresOn"/> in the
    /// configured time zone. The date must lie between tomorrow and the 🔧 maximum after today
    /// (TEN-12); a missing or other date gives <see cref="ShareLinkCreation.ExpiryError"/> and stores
    /// nothing. The token is in the answer only here: the table keeps its SHA-256 hash (TEN-08).
    /// </summary>
    Task<ShareLinkCreation> CreateAsync(
        Guid organizationId,
        ShareableResourceType resourceType,
        Guid resourceId,
        DateOnly? expiresOn,
        CancellationToken cancellationToken);

    /// <summary>
    /// The dates a new link may end on now: tomorrow, the 🔧 default S15 offers and the 🔧 maximum,
    /// counted in the configured time zone (TEN-12; owner decision S5-24 1a), so the client does no
    /// date arithmetic of its own.
    /// </summary>
    ShareLinkExpiryWindow ExpiryWindow();

    /// <summary>The links to the resource that are neither revoked nor expired, the oldest first.</summary>
    Task<IReadOnlyList<ActiveShareLink>> ListActiveAsync(
        ShareableResourceType resourceType, Guid resourceId, CancellationToken cancellationToken);

    /// <summary>The link with this id — also a revoked or expired one —, or <c>null</c> when it does not exist for the request.</summary>
    Task<ShareLinkTarget?> FindAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Ends the link at once (US-SAV-03); revoking a revoked link changes nothing. <c>false</c>
    /// when it does not exist for the request.
    /// </summary>
    Task<bool> RevokeAsync(Guid id, CancellationToken cancellationToken);
}

/// <summary>The result of <see cref="IShareLinks.CreateAsync"/>: the new link, or why the expiry date was refused.</summary>
/// <param name="Link">The new link with its token; <c>null</c> when the date was refused.</param>
/// <param name="ExpiryError">The field error of the expiry date; <c>null</c> when the link was created.</param>
public sealed record ShareLinkCreation(CreatedShareLink? Link, string? ExpiryError);

/// <summary>A new share link. <paramref name="Token"/> is shown once and never stored (TEN-08).</summary>
public sealed record CreatedShareLink(Guid Id, string Token, DateTimeOffset ExpiresAt);

/// <summary>A link as S15 lists it: no token, which only exists as a hash.</summary>
/// <param name="Id">The link, for its revocation.</param>
/// <param name="CreatedAt">When it was created.</param>
/// <param name="ExpiresAt">The instant it stops working: the midnight after <paramref name="ExpiresOn"/> in the configured zone.</param>
/// <param name="ExpiresOn">The last day it works, as it was chosen (S15 shows this day, owner decision S5-24 1a).</param>
public sealed record ActiveShareLink(Guid Id, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt, DateOnly ExpiresOn);

/// <summary>The last days a new link may have now, in the configured zone (TEN-12).</summary>
/// <param name="Earliest">Tomorrow.</param>
/// <param name="Default">The date S15 offers: today plus the 🔧 default (SD15: 90 days).</param>
/// <param name="Latest">Today plus the 🔧 maximum (SD15: one year).</param>
public sealed record ShareLinkExpiryWindow(DateOnly Earliest, DateOnly Default, DateOnly Latest);

/// <summary>What a link points to and whose it is, for the role check before a revocation.</summary>
public sealed record ShareLinkTarget(Guid Id, Guid OrganizationId, ShareableResourceType ResourceType, Guid ResourceId);
