// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Buffers.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Accounts;

/// <summary>
/// <see cref="IAccountTokens"/> on <c>identity."AccountTokens"</c> (ARCHITECTURE §9.1, C72):
/// 32 random bytes as Base64url in the link, their SHA-256 hash in the table. The bytes are random,
/// so a plain hash suffices (no salt, no slow hash): nothing can be guessed from it.
/// </summary>
internal sealed class AccountTokenService(
    IdentityAccessDbContext db,
    AccountTokenSettings settings,
    TimeProvider clock) : IAccountTokens
{
    /// <summary>Random bytes per token: 256 bits.</summary>
    internal const int TokenBytes = 32;

    public async Task<IssuedAccountToken> IssueAsync(
        string userId, AccountTokenPurpose purpose, CancellationToken cancellationToken = default)
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenBytes);
        var lifetime = settings.LifetimeOf(purpose);
        var token = AccountToken.Create(userId, purpose, SHA256.HashData(bytes), clock.GetUtcNow(), lifetime);

        db.AccountTokens.Add(token);
        await db.SaveChangesAsync(cancellationToken);

        return new IssuedAccountToken(Base64Url.EncodeToString(bytes), token.ExpiresAt, lifetime);
    }

    public async Task<AccountTokenRedemption> RedeemAsync(
        string token, AccountTokenPurpose purpose, CancellationToken cancellationToken = default)
    {
        if (!TryHash(token, out var hash))
        {
            return AccountTokenRedemption.Invalid;
        }

        var now = clock.GetUtcNow();

        // One statement: of two concurrent calls only one finds UsedAt still null.
        var redeemed = await db.AccountTokens
            .Where(t => t.TokenHash == hash && t.Purpose == purpose && t.UsedAt == null && t.ExpiresAt > now)
            .ExecuteUpdateAsync(set => set.SetProperty(t => t.UsedAt, now), cancellationToken);

        var row = await db.AccountTokens
            .AsNoTracking()
            .Where(t => t.TokenHash == hash && t.Purpose == purpose)
            .Select(t => new { t.UserId, t.UsedAt })
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return AccountTokenRedemption.Invalid;
        }

        if (redeemed == 1)
        {
            return new AccountTokenRedemption(AccountTokenOutcome.Redeemed, row.UserId);
        }

        return row.UsedAt is null
            ? AccountTokenRedemption.Invalid // expired before it was used
            : new AccountTokenRedemption(AccountTokenOutcome.AlreadyUsed, row.UserId);
    }

    /// <summary>The hash of a token in the link's form; false for anything that is not one.</summary>
    internal static bool TryHash(string? token, out byte[] hash)
    {
        hash = [];
        if (string.IsNullOrEmpty(token) || Base64Url.GetMaxDecodedLength(token.Length) < TokenBytes)
        {
            return false;
        }

        Span<byte> bytes = stackalloc byte[Base64Url.GetMaxDecodedLength(token.Length)];
        if (!Base64Url.TryDecodeFromChars(token, bytes, out var written) || written != TokenBytes)
        {
            return false;
        }

        hash = SHA256.HashData(bytes[..written]);
        return true;
    }
}
