// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.FixtureTests;

/// <summary>
/// The test authentication handler signs requests in as a fixture user; without one the request
/// stays anonymous. <c>GET /api/Proposals</c> is an existing endpoint that requires a signed-in user.
/// </summary>
public sealed class TestAuthenticationTests(ApiFixture api) : IntegrationTest(api)
{
    private const string ProtectedEndpoint = "/api/Proposals";

    [Fact]
    public async Task ProtectedEndpoint_Anonymous_Returns401()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(ProtectedEndpoint, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_AsFixtureUser_Returns200()
    {
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.GetAsync(ProtectedEndpoint, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedEndpoint_UnknownTestUser_Returns401()
    {
        var unknown = new TestUser("not-a-fixture-user", "00000000-0000-0000-0000-00000000ffff", "x@example.test", [], []);
        using var client = CreateClient(unknown);

        using var response = await client.GetAsync(ProtectedEndpoint, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
