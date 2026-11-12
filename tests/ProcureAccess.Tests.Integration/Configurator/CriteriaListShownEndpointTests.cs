// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using ProcureAccess.Modules.Platform.Analytics;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Configurator;

/// <summary>
/// <c>POST /api/v1/configurator/criteria-list-shown</c> (SPEC §8.7, R-MET-01, R-CON-10; owner
/// decision O-S4-48 (a)): one call writes one <c>CriteriaListShown</c> event with the dimension of
/// the request's sign-in. The background service stores the queued event, so the tests wait for
/// the rows.
/// </summary>
public sealed class CriteriaListShownEndpointTests(ApiFixture api) : IntegrationTest(api)
{
    private const string Path = "/api/v1/configurator/criteria-list-shown";

    public static TheoryData<bool, string> Audiences => new()
    {
        { false, UsageEventWriter.Dimensions.Anonymous },
        { true, UsageEventWriter.Dimensions.SignedIn },
    };

    [Theory]
    [MemberData(nameof(Audiences))]
    public async Task Post_OnceForAShownList_Answers204AndStoresOneEventWithTheAudience(
        bool signedIn, string expectedDimension)
    {
        await DeleteAllEventsAsync();
        using var client = CreateClient(signedIn ? TestUsers.Member : null);

        using var response = await client.PostAsync(Path, content: null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).ShouldBeEmpty();
        var stored = (await StoredEventsAsync(1)).ShouldHaveSingleItem();
        stored.EventType.ShouldBe(UsageEventType.CriteriaListShown);
        stored.OccurredOn.ShouldBe(DateOnly.FromDateTime(Clock.GetUtcNow().UtcDateTime));
        stored.Dimension1.ShouldBe(expectedDimension);
        stored.Dimension2.ShouldBeNull();
    }

    [Fact]
    public async Task Post_TwoCalls_StoresTwoEvents()
    {
        // The server counts every call; the client decides what a new selection is (O-S4-55 (a)).
        await DeleteAllEventsAsync();
        using var client = CreateClient();

        for (var i = 0; i < 2; i++)
        {
            using var response = await client.PostAsync(Path, content: null, TestContext.Current.CancellationToken);
            response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        (await StoredEventsAsync(2)).Count.ShouldBe(2);
    }

    [Fact]
    public async Task Get_NotAPost_IsRefused()
    {
        using var client = CreateClient();

        using var refused = await client.GetAsync(Path, TestContext.Current.CancellationToken);

        refused.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

    private Task DeleteAllEventsAsync() => UsageEventRows.DeleteAllAsync(Api);

    private Task<List<UsageEvent>> StoredEventsAsync(int count) => UsageEventRows.WaitForAsync(Api, count);
}
