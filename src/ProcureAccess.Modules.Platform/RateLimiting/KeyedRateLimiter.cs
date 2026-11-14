// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using ProcureAccess.Modules.Platform.Contracts.RateLimiting;

namespace ProcureAccess.Modules.Platform.RateLimiting;

/// <summary>
/// <see cref="IKeyedRateLimiter"/>: one fixed-window limiter per keyed policy and key, in memory
/// only (S5-13a; SPEC §7.3 „per address“). The key is kept as its SHA-256 hash, so the process never
/// holds a plain e-mail address for the window's length. Like the middleware's policies it keeps
/// its own clock, not <see cref="TimeProvider"/>.
/// </summary>
internal sealed class KeyedRateLimiter : IKeyedRateLimiter, IDisposable
{
    private readonly Dictionary<string, PartitionedRateLimiter<string>> _limiters;

    public KeyedRateLimiter(IReadOnlyDictionary<string, RateLimitingRegistration.FixedWindowLimit> limits)
    {
        _limiters = limits.ToDictionary(
            entry => entry.Key,
            entry => PartitionedRateLimiter.Create<string, string>(key => RateLimitPartition.GetFixedWindowLimiter(
                key,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = entry.Value.PermitLimit,
                    Window = entry.Value.Window,
                    QueueLimit = 0,
                    AutoReplenishment = true,
                })),
            StringComparer.Ordinal);
    }

    public void Acquire(string policy, string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (!_limiters.TryGetValue(policy, out var limiter))
        {
            throw new ArgumentOutOfRangeException(nameof(policy), policy, "Not a keyed rate-limit policy.");
        }

        using var lease = limiter.AttemptAcquire(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))));
        if (!lease.IsAcquired)
        {
            throw new RateLimitExceededException(
                policy,
                lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) ? retryAfter : null);
        }
    }

    public void Dispose()
    {
        foreach (var limiter in _limiters.Values)
        {
            limiter.Dispose();
        }
    }
}
