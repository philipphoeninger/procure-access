// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using ProcureAccess.Modules.IdentityAccess.Session;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.IdentityAccess;

/// <summary>
/// <see cref="SessionCookieEvents.AbsoluteExpiry"/> (S5-29, S6-01): the absolute limit in whole
/// seconds, as the cookie ticket stores its idle limit and the expiry headers name both. With the
/// fraction of the sign-in second, the probe reported an absolute limit later than the capped idle
/// limit, so the client's session warning opened for the idle limit and opened again for the
/// absolute one with the next answer (#379).
/// </summary>
public class SessionCookieEventsTests
{
    private static readonly SessionSettings Settings = new()
    {
        IdleTimeout = TimeSpan.FromMinutes(30),
        AbsoluteTimeout = TimeSpan.FromHours(8),
        WarningLead = TimeSpan.FromMinutes(5),
    };

    [Fact(DisplayName = "INV:SEC-07 " + nameof(AbsoluteExpiry_SignInWithAFractionOfASecond_IsTheWholeSecondOfTheLimit))]
    [Trait("Invariant", "SEC-07")]
    public void AbsoluteExpiry_SignInWithAFractionOfASecond_IsTheWholeSecondOfTheLimit()
    {
        var signedIn = new DateTimeOffset(2026, 11, 17, 13, 22, 49, TimeSpan.Zero).AddTicks(8_209_408);

        var limit = SessionCookieEvents.AbsoluteExpiry(Ticket(signedIn), Settings);

        limit.ShouldBe(new DateTimeOffset(2026, 11, 17, 21, 22, 49, TimeSpan.Zero));
    }

    [Fact]
    public void AbsoluteExpiry_WithoutStoredSignInTime_IsNull() =>
        SessionCookieEvents.AbsoluteExpiry(new AuthenticationProperties(), Settings).ShouldBeNull();

    private static AuthenticationProperties Ticket(DateTimeOffset signedIn) =>
        new(new Dictionary<string, string?>
        {
            [SessionCookieEvents.SignedInUtcKey] = signedIn.ToString("O", CultureInfo.InvariantCulture),
        });
}
