// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json;
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
    public async Task Health_AfterAdvancingFakeClock_ReturnsFakeClockTime()
    {
        Clock.Advance(TimeSpan.FromMinutes(31));
        var expected = Clock.GetUtcNow();
        using var client = CreateClient();

        using var response = await client.GetAsync("/api/Health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        json.RootElement.GetProperty("time").GetDateTimeOffset().ShouldBe(expected);
    }
}
