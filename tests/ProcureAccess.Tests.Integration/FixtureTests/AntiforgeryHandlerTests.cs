// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net.Http.Json;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.FixtureTests;

/// <summary>
/// The antiforgery hook of the test client against a stub server. The real session probe and
/// antiforgery validation only arrive in migration step 3; these tests keep the hook from
/// breaking unnoticed until then.
/// </summary>
public sealed class AntiforgeryHandlerTests
{
    private const string SessionProbe = AntiforgeryHandler.SessionProbePath;

    [Fact]
    public async Task StateChangingRequest_SessionProbeIssuesToken_SendsTokenAsHeader()
    {
        var server = new StubServer(request => request.RequestUri!.AbsolutePath == SessionProbe
            ? WithTokenCookie(HttpStatusCode.OK, "token-1")
            : new HttpResponseMessage(HttpStatusCode.OK));
        using var client = server.CreateClient();

        await client.PostAsync("/api/something", content: null, TestContext.Current.CancellationToken);

        server.Requests.Select(r => (r.Method, r.Path, r.TokenHeader)).ShouldBe([
            (HttpMethod.Get, SessionProbe, null),
            (HttpMethod.Post, "/api/something", "token-1"),
        ]);
    }

    [Fact]
    public async Task StateChangingRequests_SessionProbeMissing_ProbeOnceAndSendWithoutHeader()
    {
        var server = new StubServer(request => request.RequestUri!.AbsolutePath == SessionProbe
            ? new HttpResponseMessage(HttpStatusCode.NotFound)
            : new HttpResponseMessage(HttpStatusCode.OK));
        using var client = server.CreateClient();

        await client.PostAsync("/api/first", content: null, TestContext.Current.CancellationToken);
        await client.DeleteAsync("/api/second", TestContext.Current.CancellationToken);

        server.Requests.Select(r => (r.Method, r.Path, r.TokenHeader)).ShouldBe([
            (HttpMethod.Get, SessionProbe, null),
            (HttpMethod.Post, "/api/first", null),
            (HttpMethod.Delete, "/api/second", null),
        ]);
    }

    [Fact]
    public async Task GetRequest_Always_DoesNotCallSessionProbe()
    {
        var server = new StubServer(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = server.CreateClient();

        await client.GetAsync("/api/something", TestContext.Current.CancellationToken);

        server.Requests.Select(r => r.Path).ShouldBe(["/api/something"]);
    }

    [Fact]
    public async Task StateChangingRequest_TokenRotatedBySignIn_SendsNewToken()
    {
        var server = new StubServer(request => request.RequestUri!.AbsolutePath switch
        {
            SessionProbe => WithTokenCookie(HttpStatusCode.OK, "token-1"),
            "/api/signIn" => WithTokenCookie(HttpStatusCode.OK, "token-2"),
            _ => new HttpResponseMessage(HttpStatusCode.OK),
        });
        using var client = server.CreateClient();

        await client.PostAsync("/api/signIn", content: null, TestContext.Current.CancellationToken);
        await client.PostAsync("/api/something", content: null, TestContext.Current.CancellationToken);

        server.Requests.Select(r => (r.Path, r.TokenHeader)).ShouldBe([
            (SessionProbe, null),
            ("/api/signIn", "token-1"),
            ("/api/something", "token-2"),
        ]);
    }

    [Fact]
    public async Task StateChangingRequest_SessionProbeFails_Throws()
    {
        var server = new StubServer(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        using var client = server.CreateClient();

        await Should.ThrowAsync<InvalidOperationException>(
            () => client.PostAsync("/api/something", content: null, TestContext.Current.CancellationToken));
    }

    private static HttpResponseMessage WithTokenCookie(HttpStatusCode status, string token)
    {
        var response = new HttpResponseMessage(status);
        response.Headers.Add("Set-Cookie", $"{AntiforgeryHandler.TokenCookieName}={token}; path=/; secure; samesite=strict");
        return response;
    }

    private sealed record SentRequest(HttpMethod Method, string Path, string? TokenHeader);

    /// <summary>Answers every request with <paramref name="respond"/> and records what was sent.</summary>
    private sealed class StubServer(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<SentRequest> Requests { get; } = [];

        public HttpClient CreateClient()
        {
            var cookies = new CookieContainer();
            var handler = new AntiforgeryHandler(cookies)
            {
                InnerHandler = new CookieContainerHandler(cookies) { InnerHandler = this },
            };
            return new HttpClient(handler) { BaseAddress = IntegrationTest.BaseAddress };
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = request.Headers.TryGetValues(AntiforgeryHandler.TokenHeaderName, out var values)
                ? values.Single()
                : null;
            Requests.Add(new SentRequest(request.Method, request.RequestUri!.AbsolutePath, token));

            // A real server sets RequestMessage; CookieContainerHandler reads its URI to store cookies.
            var response = respond(request);
            response.RequestMessage = request;
            return Task.FromResult(response);
        }
    }
}

/// <summary>
/// The hook against the real API: before migration step 3 the session probe answers 404, and a
/// state-changing request still reaches its endpoint.
/// </summary>
public sealed class AntiforgeryHookApiTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact]
    public async Task SignIn_BeforeSessionProbeExists_ReachesEndpoint()
    {
        using var client = CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/signIn",
            new { email = "nobody@example.test", password = "Wrong-password-1" },
            TestContext.Current.CancellationToken);

        // 400 "InvalidLogin" comes from the endpoint itself, so the request got through.
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldContain("InvalidLogin");
    }
}
