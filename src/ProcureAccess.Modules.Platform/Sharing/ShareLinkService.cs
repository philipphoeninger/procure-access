// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Buffers.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Platform.Contracts.Sharing;
using ProcureAccess.Modules.Platform.Persistence;

namespace ProcureAccess.Modules.Platform.Sharing;

/// <summary>
/// Share links (ARCHITECTURE §5.3, C59, C81; SPEC R-SAV-04, R-SAV-05, SD15; S5-23): the management
/// for the owning module (<see cref="IShareLinks"/>, through the organisation filter) and the
/// anonymous read of <see cref="SharedEndpoints"/> (<see cref="ReadSharedAsync"/>, past it).
/// </summary>
/// <remarks>
/// A token is 32 random bytes in Base64url, as the other link tokens (SEC-11); only its SHA-256
/// hash is stored, and nothing here logs it (TEN-08). The token travels in the path of the read
/// (owner decision S5-23 1a), so the log sinks mask that path (<c>LinkTokenMasking</c>).
/// </remarks>
internal sealed class ShareLinkService(
    PlatformDbContext db,
    IEnumerable<IShareableResourceProvider> providers,
    ShareLinkSettings settings,
    TimeProvider clock) : IShareLinks
{
    /// <summary>Bytes of a token: as the account and invitation tokens (SEC-11).</summary>
    public const int TokenBytes = 32;

    public async Task<ShareLinkCreation> CreateAsync(
        Guid organizationId,
        ShareableResourceType resourceType,
        Guid resourceId,
        DateOnly? expiresOn,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        if (ExpiryErrorOf(expiresOn) is { } error)
        {
            return new ShareLinkCreation(null, error);
        }

        var bytes = RandomNumberGenerator.GetBytes(TokenBytes);
        var link = ShareLink.Create(organizationId, resourceType, resourceId, SHA256.HashData(bytes), now, EndOf(expiresOn!.Value));
        db.ShareLinks.Add(link);
        await db.SaveChangesAsync(cancellationToken);
        return new ShareLinkCreation(new CreatedShareLink(link.Id, Base64Url.EncodeToString(bytes), link.ExpiresAt), null);
    }

    public ShareLinkExpiryWindow ExpiryWindow()
    {
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), settings.TimeZone).DateTime);
        return new ShareLinkExpiryWindow(
            today.AddDays(1), today.AddDays(settings.DefaultExpiryDays), today.AddMonths(settings.MaxExpiryMonths));
    }

    public async Task<IReadOnlyList<ActiveShareLink>> ListActiveAsync(
        ShareableResourceType resourceType, Guid resourceId, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var links = await db.ShareLinks.AsNoTracking()
                            .Where(l => l.ResourceType == resourceType && l.ResourceId == resourceId
                                        && l.RevokedAt == null && l.ExpiresAt > now)
                            .OrderBy(l => l.CreatedAt)
                            .ThenBy(l => l.Id)
                            .Select(l => new { l.Id, l.CreatedAt, l.ExpiresAt })
                            .ToListAsync(cancellationToken);
        return [.. links.Select(l => new ActiveShareLink(l.Id, l.CreatedAt, l.ExpiresAt, LastDayOf(l.ExpiresAt)))];
    }

    public async Task<ShareLinkTarget?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await db.ShareLinks.AsNoTracking()
                .Where(l => l.Id == id)
                .Select(l => new ShareLinkTarget(l.Id, l.OrganizationId, l.ResourceType, l.ResourceId))
                .SingleOrDefaultAsync(cancellationToken);

    public async Task<bool> RevokeAsync(Guid id, CancellationToken cancellationToken)
    {
        var link = await db.ShareLinks.SingleOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (link is null)
        {
            return false;
        }

        link.Revoke(clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// The redacted resource behind <paramref name="token"/> for someone without an account
    /// (R-SAV-05, TEN-09), or <c>null</c> — one answer for a malformed, unknown, revoked or expired
    /// token and for a resource that no longer exists, so the 404 tells nothing apart (TEN-08).
    /// </summary>
    public async Task<object?> ReadSharedAsync(string token, CancellationToken cancellationToken)
    {
        if (!TryHash(token, out var hash))
        {
            return null;
        }

        // TEN-02 allow-list: the reader has no account and so no organisation; the token is the
        // authorisation, and the link names the one resource it opens.
        var link = await db.ShareLinks.IgnoreQueryFilters().AsNoTracking()
                           .SingleOrDefaultAsync(l => l.TokenHash == hash, cancellationToken);
        if (link is null || !link.IsActiveAt(clock.GetUtcNow()))
        {
            return null;
        }

        var provider = providers.SingleOrDefault(p => p.ResourceType == link.ResourceType)
            ?? throw new InvalidOperationException($"No {nameof(IShareableResourceProvider)} reads {link.ResourceType}.");
        return await provider.ReadSharedAsync(link.ResourceId, link.ExpiresAt, cancellationToken);
    }

    /// <summary>
    /// Why <paramref name="expiresOn"/> cannot be a link's last day now, or
    /// <c>null</c>: it is required and lies between tomorrow and today plus the 🔧 maximum, both
    /// counted in the configured zone (TEN-12; UX copy <c>criteria.share.expiry.error</c>).
    /// </summary>
    private string? ExpiryErrorOf(DateOnly? expiresOn)
    {
        var window = ExpiryWindow();
        return expiresOn switch
        {
            null => "The expiry date is required.",
            { } date when date < window.Earliest || date > window.Latest =>
                $"The expiry date lies between {window.Earliest:yyyy-MM-dd} and {window.Latest:yyyy-MM-dd}.",
            _ => null,
        };
    }

    /// <summary>The last day a link ending at <paramref name="expiresAt"/> works: the day before that midnight in the configured zone.</summary>
    private DateOnly LastDayOf(DateTimeOffset expiresAt) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(expiresAt, settings.TimeZone).DateTime).AddDays(-1);

    /// <summary>The end of <paramref name="day"/> in the configured zone: the start of the next day, in UTC.</summary>
    private DateTimeOffset EndOf(DateOnly day)
    {
        // Midnight is never skipped or repeated in the zones of the DACH region (their clocks change at 2 and 3 a.m.).
        var nextMidnight = day.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(nextMidnight, settings.TimeZone), TimeSpan.Zero);
    }

    /// <summary>The hash of a token in the link's form; false for anything that is not one.</summary>
    private static bool TryHash(string? token, out byte[] hash)
    {
        hash = [];
        // Exactly the length of a token, so the buffer stays small whatever the path holds.
        if (token is null || token.Length != Base64Url.GetEncodedLength(TokenBytes))
        {
            return false;
        }

        Span<byte> bytes = stackalloc byte[TokenBytes];
        int written;
        try
        {
            if (!Base64Url.TryDecodeFromChars(token, bytes, out written) || written != TokenBytes)
            {
                return false;
            }
        }
        catch (FormatException)
        {
            // TryDecodeFromChars returns false only for a short buffer: a text of the right length
            // that is no Base64url (a character outside it, or spare bits set in the last one)
            // throws. It is a malformed link like any other: 404 (R-SAV-05; found in S5-24).
            return false;
        }

        hash = SHA256.HashData(bytes[..written]);
        return true;
    }
}
