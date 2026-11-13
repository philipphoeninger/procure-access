// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Api;

/// <summary>
/// The scheme behind the reverse proxies (S5-07, owner decision 1a; ARCHITECTURE §9.2): Traefik
/// terminates TLS, so the API's connection is plain HTTP. The Secure session and antiforgery
/// cookies refuse a non-HTTPS request (ASP.NET throws, the API answers 500), so the API takes the
/// client's scheme from <c>X-Forwarded-Proto</c> — from the trusted proxies only, like the address.
/// Runs on a host of its own that lets the test set the connection's remote address.
/// </summary>
public sealed class ForwardedProtoTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Probe = AntiforgeryHandler.SessionProbePath;

    /// <summary>Address of the client's nginx in the Docker network: a trusted proxy.</summary>
    private const string Nginx = "172.18.0.3";

    /// <summary>Address of Traefik in the Docker network: a trusted proxy.</summary>
    private const string Traefik = "172.18.0.2";

    private const string Client = "203.0.113.10";

    private static readonly Uri PlainHttp = new("http://localhost");

    private ProcureAccessApiFactory? _host;

    private ProcureAccessApiFactory Host => _host ?? throw new InvalidOperationException("The host is not started.");

    public ValueTask InitializeAsync()
    {
        _host = new ProcureAccessApiFactory(Api.ConnectionString, Clock, configureServices: TestRemoteAddress.AddTo);
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }
    }

    [Fact]
    public async Task Probe_OverHttpFromATrustedProxyForwardingHttps_IssuesTheSecureToken()
    {
        using var client = Host.CreateDefaultClient(PlainHttp);
        using var request = new HttpRequestMessage(HttpMethod.Get, Probe);
        request.Headers.Add(TestRemoteAddress.Header, Nginx);
        request.Headers.Add("X-Forwarded-For", $"{Client}, {Traefik}");
        request.Headers.Add("X-Forwarded-Proto", "https");

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.GetValues("Set-Cookie")
            .ShouldContain(c => c.StartsWith(AntiforgeryHandler.TokenCookieName + "=", StringComparison.Ordinal)
                                && c.Contains("secure", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Probe_OverHttpWithoutAForwardedScheme_Fails()
    {
        // The failure mode a proxy without X-Forwarded-Proto produces: the Secure cookies cannot be
        // issued on a plain HTTP request, and the API answers 500 instead of a token.
        using var client = Host.CreateDefaultClient(PlainHttp);
        using var request = new HttpRequestMessage(HttpMethod.Get, Probe);
        request.Headers.Add(TestRemoteAddress.Header, Nginx);
        request.Headers.Add("X-Forwarded-For", $"{Client}, {Traefik}");

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Probe_OverHttpWithAForwardedSchemeFromAnUntrustedAddress_IgnoresIt()
    {
        // A caller outside the proxy networks cannot make the API believe it runs behind TLS.
        using var client = Host.CreateDefaultClient(PlainHttp);
        using var request = new HttpRequestMessage(HttpMethod.Get, Probe);
        request.Headers.Add(TestRemoteAddress.Header, Client);
        request.Headers.Add("X-Forwarded-Proto", "https");

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }
}
