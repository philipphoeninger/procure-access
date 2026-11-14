// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Platform.Contracts.RateLimiting;
using ProcureAccess.Modules.Platform.RateLimiting;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.RateLimiting;

/// <summary>
/// The keyed policies of SPEC §7.3 whose partition is in the request body (S5-13a: the password
/// reset per address); the HTTP answer is tested in <c>PasswordResetTests</c>.
/// </summary>
public sealed class KeyedRateLimiterTests
{
    private const string Policy = RateLimitPolicies.PasswordResetAddress;

    [Fact]
    public void Acquire_OneMoreThanTheLimit_ThrowsWithTheWholeWindowAsTheWait()
    {
        using var limiter = Limiter(permitLimit: 3);
        for (var i = 0; i < 3; i++)
        {
            limiter.Acquire(Policy, "A@EXAMPLE.TEST");
        }

        var refused = Should.Throw<RateLimitExceededException>(() => limiter.Acquire(Policy, "A@EXAMPLE.TEST"));

        refused.Policy.ShouldBe(Policy);
        refused.RetryAfter.ShouldBe(TimeSpan.FromHours(1));
    }

    [Fact]
    public void Acquire_OtherKey_HasItsOwnCounter()
    {
        using var limiter = Limiter(permitLimit: 1);
        limiter.Acquire(Policy, "A@EXAMPLE.TEST");

        Should.NotThrow(() => limiter.Acquire(Policy, "B@EXAMPLE.TEST"));
    }

    [Fact]
    public void Acquire_UnknownPolicy_IsAProgrammingError()
    {
        using var limiter = Limiter(permitLimit: 1);

        Should.Throw<ArgumentOutOfRangeException>(() => limiter.Acquire(RateLimitPolicies.Registration, "A@EXAMPLE.TEST"));
    }

    private static KeyedRateLimiter Limiter(int permitLimit) =>
        new(new Dictionary<string, RateLimitingRegistration.FixedWindowLimit>
        {
            [Policy] = new(permitLimit, TimeSpan.FromHours(1)),
        });
}
