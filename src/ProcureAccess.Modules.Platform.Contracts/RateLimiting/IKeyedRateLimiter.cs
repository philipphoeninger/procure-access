// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.RateLimiting;

/// <summary>
/// The rate limits of SPEC §7.3 whose partition only the endpoint knows, because it is in the
/// request body (S5-13a: the e-mail address of the password reset). Same fixed window and the same
/// 429 answer as the policies the middleware applies (API-05).
/// </summary>
public interface IKeyedRateLimiter
{
    /// <summary>
    /// Counts one request of <paramref name="policy"/> for <paramref name="key"/>. Throws
    /// <see cref="RateLimitExceededException"/> when the window holds no permit any more; Platform
    /// answers that with 429 Problem Details <c>rate-limit</c>.
    /// </summary>
    /// <param name="policy">A keyed policy of <see cref="RateLimitPolicies"/>.</param>
    /// <param name="key">The partition, normalised by the caller (e.g. an upper-case address). Kept
    /// in memory only as its SHA-256 hash, never logged.</param>
    void Acquire(string policy, string key);
}
