// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// Antiforgery validation on the shared host (SEC-02; ARCHITECTURE §9.1; S5-07): a state-changing
/// request needs the token of the probe as <c>X-XSRF-TOKEN</c>, else 400 Problem Details of type
/// <c>antiforgery</c>; the anonymous export is exempt, asserted explicitly. The exemption list is
/// checked whole, so it can only shrink knowingly: the export, and until S5-10 the anonymous legacy
/// identity endpoints. A Bearer header is no exemption any more since the JWT went (S5-08).
/// </summary>
public sealed class AntiforgeryValidationTests(ApiFixture api) : IntegrationTest(api)
{
    private const string CriteriaListShown = "/api/v1/configurator/criteria-list-shown";
    private const string Export = "/api/v1/exports";

    [Fact(DisplayName = "INV:SEC-02 " + nameof(StateChangingRequest_WithoutToken_Returns400Antiforgery))]
    [Trait("Invariant", "SEC-02")]
    public async Task StateChangingRequest_WithoutToken_Returns400Antiforgery()
    {
        using var client = Api.Factory.CreateDefaultClient(BaseAddress);

        using var response = await client.PostAsync(CriteriaListShown, content: null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemTypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.Antiforgery);
    }

    [Fact(DisplayName = "INV:SEC-02 " + nameof(StateChangingRequest_WithWrongToken_Returns400Antiforgery))]
    [Trait("Invariant", "SEC-02")]
    public async Task StateChangingRequest_WithWrongToken_Returns400Antiforgery()
    {
        using var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, CriteriaListShown);
        request.Headers.Add(AntiforgeryHandler.TokenHeaderName, "not-the-token");

        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemTypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.Antiforgery);
    }

    [Fact(DisplayName = "INV:SEC-02 " + nameof(StateChangingRequest_WithTokenOfTheProbe_ReachesEndpoint))]
    [Trait("Invariant", "SEC-02")]
    public async Task StateChangingRequest_WithTokenOfTheProbe_ReachesEndpoint()
    {
        using var client = CreateClient();

        using var response = await client.PostAsync(CriteriaListShown, content: null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "INV:SEC-02 " + nameof(AnonymousExport_WithoutToken_IsExemptAndReachesEndpoint))]
    [Trait("Invariant", "SEC-02")]
    public async Task AnonymousExport_WithoutToken_IsExemptAndReachesEndpoint()
    {
        using var client = Api.Factory.CreateDefaultClient(BaseAddress);
        using var body = new StringContent("{}", Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(Export, body, TestContext.Current.CancellationToken);

        // The endpoint itself refuses the empty body: a validation problem, not an antiforgery one.
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemTypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.Validation);
    }

    [Fact(DisplayName = "INV:SEC-02 " + nameof(ExemptEndpoints_AreExactlyTheExportAndTheLegacyIdentityPosts))]
    [Trait("Invariant", "SEC-02")]
    public void ExemptEndpoints_AreExactlyTheExportAndTheLegacyIdentityPosts()
    {
        var exempt = Api.Factory.Services.GetServices<EndpointDataSource>()
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAntiforgeryMetadata>() is { RequiresValidation: false })
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .Order(StringComparer.Ordinal)
            .ToList();

        exempt.ShouldBe(
        [
            // Legacy anonymous identity POSTs until S5-10 (S5-12, S5-13): no session to forge a request into.
            "/api/forgot-password",
            "/api/reset-password",
            "/api/signUp",
            // The anonymous export changes no server state (SEC-02, EXP-05).
            "/api/v{version:apiVersion}/exports",
        ]);
    }

    [Fact]
    public async Task StateChangingRequest_UnknownRoute_Returns404NotAntiforgery()
    {
        using var client = Api.Factory.CreateDefaultClient(BaseAddress);

        using var response = await client.PostAsync("/api/v1/no-such-endpoint", content: null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "INV:SEC-02 " + nameof(StateChangingRequest_WithBearerHeader_IsValidatedLikeAnyOther))]
    [Trait("Invariant", "SEC-02")]
    public async Task StateChangingRequest_WithBearerHeader_IsValidatedLikeAnyOther()
    {
        // The legacy JWT skipped validation until S5-08; a Bearer header exempts nothing any more.
        using var client = Api.Factory.CreateDefaultClient(BaseAddress);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "legacy-token");

        using var response = await client.PostAsync(CriteriaListShown, content: null, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemTypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.Antiforgery);
    }

    private static async Task<string> ProblemTypeOf(HttpResponseMessage response)
    {
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        return body.RootElement.GetProperty("type").GetString()!;
    }
}
