// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Api;

/// <summary>
/// API-01: every validation failure is an RFC 9457 Problem Details document of type
/// <c>validation</c> with an <c>errors</c> map keyed by field path (SPEC §7.1, §8.6;
/// ARCHITECTURE §7.1). Also the other errors Platform turns into Problem Details on the shared host:
/// unknown routes and the former ad-hoc error object of the UI customisation (TECH_DEBT_v1 §5).
/// </summary>
public sealed class ProblemDetailsTests(ApiFixture api) : IntegrationTest(api)
{
    private const string MalformedJson = "{";

    /// <summary>Value for every route parameter except the API version: today all of them are numbers.</summary>
    private const string InvalidRouteValue = "not-a-number";

    private static readonly HashSet<string> WriteMethods = ["POST", "PUT", "PATCH", "DELETE"];

    /// <summary>Parameters of minimal-API handlers that the framework supplies, not the request.</summary>
    private static readonly HashSet<Type> FrameworkParameterTypes =
        [typeof(HttpContext), typeof(HttpRequest), typeof(HttpResponse), typeof(CancellationToken), typeof(ClaimsPrincipal)];

    private string TypeUri(string name) =>
        $"{Api.Factory.Services.GetRequiredService<IConfiguration>()["ProblemTypes:BaseUri"]}/problems/{name}";

    [Fact(DisplayName = "INV:API-01 " + nameof(WriteEndpoints_InvalidPayload_ReturnValidationProblemDetails))]
    [Trait("Invariant", "API-01")]
    [Trait("Kind", "Contract")]
    public async Task WriteEndpoints_InvalidPayload_ReturnValidationProblemDetails()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var requests = WriteRequestsWithInvalidInput();
        requests.ShouldNotBeEmpty("endpoint discovery found no write endpoint");

        // The moderator passes every authorisation check of today's write endpoints, so each request
        // reaches model binding.
        using var client = CreateClient(TestUsers.Moderator);
        var failures = new List<string>();
        foreach (var (method, path) in requests)
        {
            using var request = new HttpRequestMessage(method, path)
            {
                Content = new StringContent(MalformedJson, Encoding.UTF8, "application/json"),
            };
            using var response = await client.SendAsync(request, cancellationToken);
            var problem = await ProblemResponse.ReadAsync(response, cancellationToken);

            var deviations = problem.Deviations(HttpStatusCode.BadRequest, TypeUri(ProblemTypes.Validation), requireErrors: true);
            if (deviations.Count > 0)
            {
                failures.Add($"{method} {path}: {string.Join("; ", deviations)}");
            }
        }

        failures.ShouldBeEmpty();
    }

    [Fact(DisplayName = "INV:API-01 " + nameof(ValidationErrors_FieldErrors_KeyedByJsonFieldPath))]
    [Trait("Invariant", "API-01")]
    [Trait("Kind", "Contract")]
    public async Task ValidationErrors_FieldErrors_KeyedByJsonFieldPath()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = CreateClient(TestUsers.Member);

        // A missing required property: the key is the property's JSON name, not the C# name "NewEmail".
        using var missing = await client.PostAsync(
            "/api/Users/change-email", Json("{}"), cancellationToken);
        var missingProblem = await ProblemResponse.ReadAsync(missing, cancellationToken);
        missingProblem.Deviations(HttpStatusCode.BadRequest, TypeUri(ProblemTypes.Validation), requireErrors: true).ShouldBeEmpty();
        missingProblem.ErrorKeys.ShouldContain("newEmail");

        // A value of the wrong type: the key is the path in the payload, without the JSON-path root "$.".
        using var wrongType = await client.PutAsync(
            "/api/UICustomization", Json("""{"darkModeOn":"x"}"""), cancellationToken);
        var wrongTypeProblem = await ProblemResponse.ReadAsync(wrongType, cancellationToken);
        wrongTypeProblem.Deviations(HttpStatusCode.BadRequest, TypeUri(ProblemTypes.Validation), requireErrors: true).ShouldBeEmpty();
        wrongTypeProblem.ErrorKeys.ShouldContain("darkModeOn");
        wrongTypeProblem.ErrorKeys.ShouldNotContain(key => key.StartsWith("$.", StringComparison.Ordinal));
    }

    [Fact]
    public async Task UnknownRoute_Requested_Returns404ProblemDetailsOfTypeNotFound()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = CreateClient();

        using var response = await client.GetAsync("/api/no-such-resource", cancellationToken);

        var problem = await ProblemResponse.ReadAsync(response, cancellationToken);
        problem.Deviations(HttpStatusCode.NotFound, TypeUri(ProblemTypes.NotFound), requireErrors: false).ShouldBeEmpty();
    }

    [Fact]
    public async Task UICustomizationUpdate_UserHasNoRecord_Returns400ProblemDetailsWithoutExceptionText()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        // The test user exists only as a principal, so the repository finds no user row.
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.PutAsync(
            "/api/UICustomization", Json("""{"darkModeOn":true}"""), cancellationToken);

        ShouldBeLegacyFailure(await ProblemResponse.ReadAsync(response, cancellationToken));
    }

    /// <summary>
    /// A failure of the legacy UI customisation endpoints: still 400 as before S2-10 (owner,
    /// 2026-11-01), now as Problem Details, and neither the exception text nor the former members
    /// <c>code</c> and <c>message</c> reach the client.
    /// </summary>
    private static void ShouldBeLegacyFailure(ProblemResponse problem)
    {
        problem.StatusCode.ShouldBe(HttpStatusCode.BadRequest, problem.Body);
        ProblemResponse.AcceptedMediaTypes.ShouldContain(problem.MediaTypeOfResponse!);
        problem.Property("title").ShouldNotBeNullOrWhiteSpace();
        problem.Property("code").ShouldBeNull();
        problem.Property("message").ShouldBeNull();
        problem.Body.ShouldNotContain("Sequence contains");
    }

    private static StringContent Json(string body) => new(body, Encoding.UTF8, "application/json");

    /// <summary>
    /// One request per write endpoint that takes input (TESTING §5.2: discovered from
    /// <see cref="EndpointDataSource"/>, so new endpoints are covered without editing this test).
    /// Route parameters get <see cref="InvalidRouteValue"/>; the body is always malformed JSON.
    /// Endpoints without any input (e.g. "revoke my sessions") have nothing to validate.
    /// </summary>
    private List<(HttpMethod Method, string Path)> WriteRequestsWithInvalidInput()
    {
        var services = Api.Factory.Services;
        var isService = services.GetRequiredService<IServiceProviderIsService>();

        return services.GetServices<EndpointDataSource>()
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(endpoint => TakesInput(endpoint, isService))
            .SelectMany(endpoint => (endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? [])
                .Where(WriteMethods.Contains)
                .Select(method => (new HttpMethod(method), PathWithInvalidRouteValues(endpoint.RoutePattern))))
            .Distinct()
            .ToList();
    }

    private static bool TakesInput(Endpoint endpoint, IServiceProviderIsService isService)
    {
        if (endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is { } action)
        {
            return action.Parameters.Any(parameter =>
                parameter.BindingInfo?.BindingSource is not { } source
                || (source != BindingSource.Services && source != BindingSource.Special));
        }

        if (endpoint.Metadata.GetMetadata<MethodInfo>() is { } handler)
        {
            return handler.GetParameters().Any(parameter =>
                !FrameworkParameterTypes.Contains(parameter.ParameterType)
                && !isService.IsService(parameter.ParameterType));
        }

        // An endpoint of another kind is checked rather than skipped.
        return true;
    }

    private static string PathWithInvalidRouteValues(RoutePattern pattern) =>
        "/" + string.Join('/', pattern.PathSegments.Select(segment => string.Concat(segment.Parts.Select(part => part switch
        {
            RoutePatternLiteralPart literal => literal.Content,
            RoutePatternSeparatorPart separator => separator.Content,
            RoutePatternParameterPart parameter when IsApiVersion(pattern, parameter) => "1",
            RoutePatternParameterPart => InvalidRouteValue,
            _ => throw new NotSupportedException($"Unknown route pattern part in '{pattern.RawText}'."),
        }))));

    private static bool IsApiVersion(RoutePattern pattern, RoutePatternParameterPart parameter) =>
        pattern.ParameterPolicies.TryGetValue(parameter.Name, out var policies)
        && policies.Any(policy => policy.Content == "apiVersion");
}
