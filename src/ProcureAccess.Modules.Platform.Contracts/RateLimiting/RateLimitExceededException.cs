// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.RateLimiting;

/// <summary>
/// Thrown by <see cref="IKeyedRateLimiter.Acquire"/> when a keyed policy refuses a request;
/// Platform's exception handler answers it with 429 Problem Details <c>rate-limit</c>,
/// <c>retryAfterMinutes</c> and <c>Retry-After</c>, as the middleware does for the other policies.
/// </summary>
public sealed class RateLimitExceededException : Exception
{
    public RateLimitExceededException()
        : this("A rate limit refused the request.")
    {
    }

    public RateLimitExceededException(string message)
        : base(message)
    {
    }

    public RateLimitExceededException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public RateLimitExceededException(string policy, TimeSpan? retryAfter)
        : base($"The rate limit '{policy}' refused the request.")
    {
        Policy = policy;
        RetryAfter = retryAfter;
    }

    /// <summary>The refusing policy.</summary>
    public string? Policy { get; }

    /// <summary>The wait the limiter reports (the whole window), if any.</summary>
    public TimeSpan? RetryAfter { get; }
}
