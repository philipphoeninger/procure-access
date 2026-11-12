// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Api;

/// <summary>
/// The rate limits of SPEC §7.3 on the anonymous endpoints (API-05; S4-21a, owner decisions
/// O-S4-56 (b), O-S4-69 (a)): limit + 1 → 429 as Problem Details of type <c>rate-limit</c> with
/// <c>retryAfterMinutes</c>, counted per client IP as the trusted proxies forward it. Runs on an API
/// host of its own with a limit of <see cref="PermitLimit"/>, on the shared database.
/// </summary>
public sealed class RateLimitTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const int PermitLimit = 2;
    private const string WindowSetting = "00:10:00";
    private const string ExportPath = "/api/v1/exports";
    private const string CriteriaListShownPath = "/api/v1/configurator/criteria-list-shown";

    /// <summary>Address of the client's nginx in the Docker network: a trusted proxy.</summary>
    private const string Nginx = "172.18.0.3";

    /// <summary>Address of Traefik in the Docker network: a trusted proxy.</summary>
    private const string Traefik = "172.18.0.2";

    private ProcureAccessApiFactory? _host;

    private ProcureAccessApiFactory Host => _host ?? throw new InvalidOperationException("The host is not started.");

    public ValueTask InitializeAsync()
    {
        _host = new ProcureAccessApiFactory(
            Api.ConnectionString,
            Clock,
            settings: new Dictionary<string, string?>
            {
                ["RateLimits:AnonymousExport:PermitLimit"] = $"{PermitLimit}",
                ["RateLimits:AnonymousExport:Window"] = WindowSetting,
                ["RateLimits:CriteriaListShown:PermitLimit"] = $"{PermitLimit}",
                ["RateLimits:CriteriaListShown:Window"] = WindowSetting,
            },
            configureServices: TestRemoteAddress.AddTo);
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }
    }

    public static TheoryData<string> LimitedPaths => [ExportPath, CriteriaListShownPath];

    [Theory(DisplayName = "INV:API-05 " + nameof(Post_OneMoreThanTheLimit_Returns429WithRetryAfterMinutes))]
    [Trait("Invariant", "API-05")]
    [MemberData(nameof(LimitedPaths))]
    public async Task Post_OneMoreThanTheLimit_Returns429WithRetryAfterMinutes(string path)
    {
        using var client = Host.CreateDefaultClient(BaseAddress);
        const string clientAddress = "203.0.113.10";

        // Refused requests count too: the export bodies here are empty and answer 400.
        for (var i = 0; i < PermitLimit; i++)
        {
            using var allowed = await PostAsync(client, path, ForwardedBy(Nginx, Traefik, clientAddress));
            allowed.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests, $"request {i + 1}");
        }

        using var limited = await PostAsync(client, path, ForwardedBy(Nginx, Traefik, clientAddress));

        var problem = await ProblemResponse.ReadAsync(limited, Cancellation);
        problem.Deviations(HttpStatusCode.TooManyRequests, TypeUri(ProblemTypes.RateLimit), requireErrors: false).ShouldBeEmpty();
        problem.MediaTypeOfResponse.ShouldBe(ProblemResponse.MediaType);
        problem.Json!.Value.GetProperty("retryAfterMinutes").GetInt32().ShouldBe(10);
        limited.Headers.RetryAfter.ShouldNotBeNull().Delta.ShouldBe(TimeSpan.FromMinutes(10));
    }

    [Theory(DisplayName = "INV:API-05 " + nameof(Post_TwoClientsBehindTheProxies_AreCountedSeparately))]
    [Trait("Invariant", "API-05")]
    [MemberData(nameof(LimitedPaths))]
    public async Task Post_TwoClientsBehindTheProxies_AreCountedSeparately(string path)
    {
        using var client = Host.CreateDefaultClient(BaseAddress);
        await UseUpTheLimitAsync(client, path, ForwardedBy(Nginx, Traefik, "203.0.113.20"));

        using var otherClient = await PostAsync(client, path, ForwardedBy(Nginx, Traefik, "203.0.113.21"));

        otherClient.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
    }

    [Fact(DisplayName = "INV:API-05 " + nameof(Post_IPv6ClientsOfOneNetwork_ShareOneCounter))]
    [Trait("Invariant", "API-05")]
    public async Task Post_IPv6ClientsOfOneNetwork_ShareOneCounter()
    {
        using var client = Host.CreateDefaultClient(BaseAddress);
        await UseUpTheLimitAsync(client, CriteriaListShownPath, ForwardedBy(Nginx, Traefik, "2001:db8:1:2::10"));

        using var sameNetwork = await PostAsync(client, CriteriaListShownPath, ForwardedBy(Nginx, Traefik, "2001:db8:1:2::99"));
        using var otherNetwork = await PostAsync(client, CriteriaListShownPath, ForwardedBy(Nginx, Traefik, "2001:db8:1:3::10"));

        sameNetwork.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        otherNetwork.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
    }

    [Fact(DisplayName = "INV:API-05 " + nameof(Post_ForwardedHeaderFromAnUntrustedAddress_DoesNotChooseTheCounter))]
    [Trait("Invariant", "API-05")]
    public async Task Post_ForwardedHeaderFromAnUntrustedAddress_DoesNotChooseTheCounter()
    {
        // A caller on the internet that reaches the API directly sends a new X-Forwarded-For each
        // time: it still has one counter, its own address.
        using var client = Host.CreateDefaultClient(BaseAddress);
        const string caller = "198.51.100.7";
        await UseUpTheLimitAsync(client, CriteriaListShownPath, new Request(caller, "203.0.113.30"));

        using var spoofed = await PostAsync(client, CriteriaListShownPath, new Request(caller, "203.0.113.31"));

        spoofed.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact(DisplayName = "INV:API-05 " + nameof(Post_ProxyAsIPv4MappedAddress_IsTrusted))]
    [Trait("Invariant", "API-05")]
    public async Task Post_ProxyAsIPv4MappedAddress_IsTrusted()
    {
        // Kestrel listening on [::] reports an IPv4 peer as ::ffff:a.b.c.d.
        using var client = Host.CreateDefaultClient(BaseAddress);
        await UseUpTheLimitAsync(client, CriteriaListShownPath, ForwardedBy($"::ffff:{Nginx}", Traefik, "203.0.113.40"));

        using var otherClient = await PostAsync(client, CriteriaListShownPath, ForwardedBy($"::ffff:{Nginx}", Traefik, "203.0.113.41"));

        otherClient.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests);
    }

    [Fact(DisplayName = "INV:API-05 " + nameof(Post_MoreProxiesThanTheForwardLimit_CountsTheLastTrustedHop))]
    [Trait("Invariant", "API-05")]
    public async Task Post_MoreProxiesThanTheForwardLimit_CountsTheLastTrustedHop()
    {
        // ForwardLimit 2 (Traefik → nginx → API): an address a client put in front of the chain is
        // never read, so two such clients behind the same Traefik entry share one counter.
        using var client = Host.CreateDefaultClient(BaseAddress);
        await UseUpTheLimitAsync(client, CriteriaListShownPath, new Request(Nginx, $"203.0.113.50, 203.0.113.51, {Traefik}"));

        using var invented = await PostAsync(client, CriteriaListShownPath, new Request(Nginx, $"203.0.113.52, 203.0.113.51, {Traefik}"));

        invented.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public void Configuration_Defaults_AreTheValuesOfSpec73()
    {
        // appsettings.json carries the 🔧 defaults; the factory raises the limits for the shared host.
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false)
            .Build();

        foreach (var policy in new[] { "AnonymousExport", "CriteriaListShown" })
        {
            configuration[$"RateLimits:{policy}:PermitLimit"].ShouldBe("30", policy);
            configuration[$"RateLimits:{policy}:Window"].ShouldBe("00:10:00", policy);
        }

        configuration["ForwardedHeaders:ForwardLimit"].ShouldBe("2");
        configuration["Health:ReadyConcurrentChecks"].ShouldBe("2");
        configuration["Health:ReadyTimeout"].ShouldBe("00:00:05");
    }

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    /// <summary>A request from <paramref name="remote"/> with an <c>X-Forwarded-For</c> of <paramref name="forwardedFor"/>.</summary>
    private sealed record Request(string Remote, string? ForwardedFor);

    /// <summary>
    /// The chain client → Traefik → nginx → API: nginx connects, and the header holds the client
    /// (added by Traefik) and Traefik (added by nginx).
    /// </summary>
    private static Request ForwardedBy(string nginx, string traefik, string clientAddress) =>
        new(nginx, $"{clientAddress}, {traefik}");

    private static async Task UseUpTheLimitAsync(HttpClient client, string path, Request request)
    {
        for (var i = 0; i < PermitLimit; i++)
        {
            using var allowed = await PostAsync(client, path, request);
            allowed.StatusCode.ShouldNotBe(HttpStatusCode.TooManyRequests, $"request {i + 1}");
        }
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string path, Request request)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, path);
        message.Headers.Add(TestRemoteAddress.Header, request.Remote);
        if (request.ForwardedFor is { } forwardedFor)
        {
            message.Headers.Add("X-Forwarded-For", forwardedFor);
        }

        return SendAndDisposeRequestAsync(client, message);
    }

    private static async Task<HttpResponseMessage> SendAndDisposeRequestAsync(HttpClient client, HttpRequestMessage message)
    {
        using (message)
        {
            return await client.SendAsync(message, Cancellation);
        }
    }

    private string TypeUri(string name) =>
        $"{Host.Services.GetRequiredService<IConfiguration>()["ProblemTypes:BaseUri"]}/problems/{name}";
}
