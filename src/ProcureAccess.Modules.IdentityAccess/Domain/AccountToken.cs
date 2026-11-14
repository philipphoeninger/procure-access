// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

namespace ProcureAccess.Modules.IdentityAccess.Domain;

/// <summary>
/// A single-use token of an account e-mail (ARCHITECTURE §9.1, C72): only its SHA-256 hash is
/// stored, so the table alone never yields a usable link.
/// </summary>
/// <remarks>
/// <see cref="UserId"/> names a row of the legacy table <c>identity."Users"</c> without a foreign
/// key, like <see cref="UserContributorLink"/> (ADR-014).
/// </remarks>
internal sealed class AccountToken
{
    /// <summary>Length of the SHA-256 hash in bytes.</summary>
    public const int HashLength = 32;

    private AccountToken()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>The ASP.NET Core Identity user id.</summary>
    public string UserId { get; private set; } = null!;

    public AccountTokenPurpose Purpose { get; private set; }

    /// <summary>SHA-256 of the token's bytes.</summary>
    public byte[] TokenHash { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>When it was redeemed; null while unused.</summary>
    public DateTimeOffset? UsedAt { get; private set; }

    /// <summary>
    /// Creates an unused token. <paramref name="createdAt"/> comes from <see cref="TimeProvider"/>
    /// (CLAUDE.md §7).
    /// </summary>
    public static AccountToken Create(
        string userId, AccountTokenPurpose purpose, byte[] tokenHash, DateTimeOffset createdAt, TimeSpan lifetime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(tokenHash);
        if (tokenHash.Length != HashLength)
        {
            throw new ArgumentException($"A token hash has {HashLength} bytes.", nameof(tokenHash));
        }

        if (!Enum.IsDefined(purpose))
        {
            throw new ArgumentOutOfRangeException(nameof(purpose), purpose, "Unknown token purpose.");
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lifetime, TimeSpan.Zero);

        return new AccountToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Purpose = purpose,
            TokenHash = tokenHash,
            CreatedAt = createdAt,
            ExpiresAt = createdAt + lifetime,
        };
    }
}
