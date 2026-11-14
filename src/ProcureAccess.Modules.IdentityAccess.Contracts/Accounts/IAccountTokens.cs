// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

/// <summary>
/// The single-use tokens of the account e-mails (ARCHITECTURE §9.1, C72; SPEC §7.8, R-AUTH-02):
/// a random value that only the e-mail holds, stored as its hash, valid for a 🔧 lifetime and
/// redeemed at most once. Implemented by the IdentityAccess module; the host's registration
/// endpoints use it while the users are still legacy (S5-12a), the password reset reuses it (S5-13).
/// </summary>
public interface IAccountTokens
{
    /// <summary>
    /// Issues a new token for <paramref name="userId"/>. Earlier tokens of the same purpose stay
    /// valid until they expire or are used.
    /// </summary>
    Task<IssuedAccountToken> IssueAsync(string userId, AccountTokenPurpose purpose, CancellationToken cancellationToken = default);

    /// <summary>
    /// Redeems <paramref name="token"/> for <paramref name="purpose"/>: marks it used if it is
    /// known, unused and not expired. Two concurrent calls with the same token never both succeed.
    /// </summary>
    Task<AccountTokenRedemption> RedeemAsync(string token, AccountTokenPurpose purpose, CancellationToken cancellationToken = default);
}

/// <summary>What an account token is for; each purpose has its own 🔧 lifetime.</summary>
public enum AccountTokenPurpose
{
    /// <summary>The link of the address confirmation (24 h 🔧, R-AUTH-02).</summary>
    EmailConfirmation = 1,
}

/// <summary>A new token: the value for the link (never stored) and its lifetime.</summary>
/// <param name="Token">The value for the link; URL-safe.</param>
/// <param name="ExpiresAt">When it stops being valid.</param>
/// <param name="Lifetime">The 🔧 lifetime of its purpose, for the text of the e-mail.</param>
public sealed record IssuedAccountToken(string Token, DateTimeOffset ExpiresAt, TimeSpan Lifetime)
{
    /// <summary>The lifetime in whole hours, rounded up, as the e-mails write it (<c>{{hours}}</c>).</summary>
    public int ValidHours => Math.Max(1, (int)Math.Ceiling(Lifetime.TotalHours));

    /// <summary>Only the type: the value must never reach a log.</summary>
    public override string ToString() => nameof(IssuedAccountToken);
}

/// <summary>The outcome of <see cref="IAccountTokens.RedeemAsync"/>.</summary>
/// <param name="Outcome">What happened.</param>
/// <param name="UserId">The user of the token for <see cref="AccountTokenOutcome.Redeemed"/> and <see cref="AccountTokenOutcome.AlreadyUsed"/>, else null.</param>
public sealed record AccountTokenRedemption(AccountTokenOutcome Outcome, string? UserId)
{
    public static AccountTokenRedemption Invalid { get; } = new(AccountTokenOutcome.Invalid, null);
}

/// <summary>Outcomes of a redemption.</summary>
public enum AccountTokenOutcome
{
    /// <summary>The token was valid and is now used.</summary>
    Redeemed = 1,

    /// <summary>
    /// The token was used before. The caller decides what that means: the address confirmation
    /// shows „bestätigt“ when the account is confirmed (owner decision S5-12 4 (b)).
    /// </summary>
    AlreadyUsed = 2,

    /// <summary>Unknown, malformed, of another purpose, or expired before it was used.</summary>
    Invalid = 3,
}
