// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.Platform.RateLimiting;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.RateLimiting;

/// <summary>The 🔧 limits of SPEC §7.3 and the wait the 429 names (S4-21a, API-05).</summary>
public sealed class RateLimitingRegistrationTests
{
    [Theory]
    [InlineData(600, 10)]
    [InlineData(61, 2)]
    [InlineData(60, 1)]
    [InlineData(1, 1)]
    [InlineData(0, 1)]
    public void RetryAfterMinutes_Wait_IsRoundedUpToWholeMinutesAndAtLeastOne(int seconds, int minutes) =>
        RateLimitingRegistration.RetryAfterMinutes(TimeSpan.FromSeconds(seconds)).ShouldBe(minutes);

    [Fact]
    public void FixedWindowLease_Rejected_ReportsTheWholeWindowAsTheWait()
    {
        // The 429 says "wait 10 minutes" also when the window ends sooner: an upper bound, never
        // too short (documented in RateLimitingRegistration).
        using var limiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0,
            AutoReplenishment = true,
        });
        using var allowed = limiter.AttemptAcquire();

        using var rejected = limiter.AttemptAcquire();

        allowed.IsAcquired.ShouldBeTrue();
        rejected.IsAcquired.ShouldBeFalse();
        rejected.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter).ShouldBeTrue();
        retryAfter.ShouldBe(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public void UserPartition_SignedIn_IsTheUserIdWhateverTheAddress()
    {
        var first = Context("203.0.113.1", userId: "user-1");
        var second = Context("203.0.113.2", userId: "user-1");

        RateLimitingRegistration.UserPartitionOf(first).ShouldBe("user:user-1");
        RateLimitingRegistration.UserPartitionOf(second).ShouldBe("user:user-1");
    }

    [Fact]
    public void UserPartition_Anonymous_IsTheClientAddressWithItsOwnPrefix() =>
        RateLimitingRegistration.UserPartitionOf(Context("203.0.113.1", userId: null)).ShouldBe("ip:203.0.113.1");

    [Fact]
    public void FixedWindowLimit_ConfiguredPolicy_IsRead()
    {
        var limit = RateLimitingRegistration.FixedWindowLimit.From(Configuration(("PermitLimit", "30"), ("Window", "00:10:00")), "AnonymousExport");

        limit.ShouldBe(new RateLimitingRegistration.FixedWindowLimit(30, TimeSpan.FromMinutes(10)));
    }

    [Theory]
    [InlineData(null, "00:10:00")]
    [InlineData("0", "00:10:00")]
    [InlineData("30", null)]
    [InlineData("30", "00:00:00")]
    public void FixedWindowLimit_MissingOrImpossibleValue_FailsTheStart(string? permitLimit, string? window)
    {
        var configuration = Configuration(("PermitLimit", permitLimit), ("Window", window));

        Should.Throw<InvalidOperationException>(() => RateLimitingRegistration.FixedWindowLimit.From(configuration, "AnonymousExport"))
            .Message.ShouldContain("RateLimits:AnonymousExport");
    }

    private static IConfiguration Configuration(params (string Key, string? Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => KeyValuePair.Create($"RateLimits:AnonymousExport:{v.Key}", v.Value)))
            .Build();

    private static DefaultHttpContext Context(string address, string? userId)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(address);
        if (userId is not null)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "Test"));
        }

        return context;
    }
}
