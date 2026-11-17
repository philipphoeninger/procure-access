// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Extensions;

/// <summary>
/// The API's clock in the E2E stack (TESTING §3.3, J5; S5-30, owner decisions D4 (a) and Q2 (a)):
/// the time of <see cref="Inner"/> plus the offset the current request asked for with the cookie
/// <see cref="TestClockExtensions.CookieName"/>. Only the request that carries the cookie sees the
/// moved time — the cookie handler, the session limits, the security-stamp check and every
/// endpoint read it through <see cref="TimeProvider"/> — so the session journey can move its own
/// browser context hours ahead while the other E2E tests on the same stack keep the real time.
/// Background work outside a request (the log retention, the mailer) always reads the real time.
/// The offset lives in the request's <see cref="HttpContext.Items"/>, read through
/// <see cref="IHttpContextAccessor"/>, which hosting keeps for the whole request — also for the
/// callbacks that run when the response starts, where the cookie handler writes a renewed cookie.
/// Registered only by <see cref="TestClockExtensions.AddTestClockIfAllowed"/>.
/// </summary>
public sealed class TestClock(TimeProvider inner, IHttpContextAccessor requests) : TimeProvider
{
    private static readonly object OffsetKey = new();

    /// <summary>The clock this one moves.</summary>
    public TimeProvider Inner { get; } = inner;

    /// <summary>Sets the offset of <paramref name="context"/>'s request.</summary>
    public static void SetOffset(HttpContext context, TimeSpan offset) => context.Items[OffsetKey] = offset;

    /// <summary>The offset of the current request; zero outside a request or without the cookie.</summary>
    public TimeSpan Offset => requests.HttpContext?.Items[OffsetKey] is TimeSpan offset ? offset : TimeSpan.Zero;

    public override DateTimeOffset GetUtcNow() => Inner.GetUtcNow() + Offset;

    public override TimeZoneInfo LocalTimeZone => Inner.LocalTimeZone;

    public override long TimestampFrequency => Inner.TimestampFrequency;

    public override long GetTimestamp() => Inner.GetTimestamp();
}
