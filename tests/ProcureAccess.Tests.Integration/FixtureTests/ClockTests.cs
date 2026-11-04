// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.FixtureTests;

/// <summary>
/// The API reads time through <see cref="TimeProvider"/>, which the fixture replaces with a
/// fake clock that tests move forward instead of waiting (TESTING §5.2).
/// </summary>
public sealed class ClockTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact]
    public void TimeProvider_AfterAdvancingFakeClock_ReturnsFakeClockTime()
    {
        Clock.Advance(TimeSpan.FromMinutes(31));
        var expected = Clock.GetUtcNow();
        using var scope = Api.Factory.Services.CreateScope();

        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        timeProvider.ShouldBeSameAs(Clock);
        timeProvider.GetUtcNow().ShouldBe(expected);
    }
}
