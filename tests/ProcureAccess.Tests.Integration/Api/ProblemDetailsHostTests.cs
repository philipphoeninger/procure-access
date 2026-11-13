// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Api;

/// <summary>
/// Problem types under a configured base URI (SPEC §8.6, ARCHITECTURE C78) and Problem Details for
/// unhandled exceptions, on a host of its own: another base URI and a test controller. The shared
/// host and the endpoint discovery of API-01 stay untouched.
/// </summary>
public sealed class ProblemDetailsHostTests(ProblemDetailsHostTests.Host host) : IClassFixture<ProblemDetailsHostTests.Host>
{
    private const string BaseUri = "https://problems.example.test";

    /// <summary>Message of every exception thrown on this host; it must never reach the client.</summary>
    internal const string SecretMessage = "internal detail that must not reach the client";

    [Fact]
    public async Task ProblemType_NameSetByEndpoint_WrittenUnderConfiguredBaseUri()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await host.Client.GetAsync(ProblemProbeController.ConflictPath, cancellationToken);

        var problem = await ProblemResponse.ReadAsync(response, cancellationToken);
        problem.Deviations(HttpStatusCode.Conflict, $"{BaseUri}/problems/{ProblemTypes.Conflict}", requireErrors: false).ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidationProblem_BaseUriConfigured_TypeUnderConfiguredBaseUri()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await host.Client.PostAsync(ProblemProbeController.ValidatePath, Json("{"), cancellationToken);

        var problem = await ProblemResponse.ReadAsync(response, cancellationToken);
        problem.Deviations(HttpStatusCode.BadRequest, $"{BaseUri}/problems/{ProblemTypes.Validation}", requireErrors: true).ShouldBeEmpty();
    }

    [Fact]
    public async Task UnhandledException_Thrown_Returns500ProblemDetailsWithoutExceptionText()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await host.Client.GetAsync(ProblemProbeController.ThrowPath, cancellationToken);

        var problem = await ProblemResponse.ReadAsync(response, cancellationToken);
        problem.StatusCode.ShouldBe(HttpStatusCode.InternalServerError, problem.Body);
        problem.MediaTypeOfResponse.ShouldBe(ProblemResponse.MediaType);
        problem.Property("title").ShouldNotBeNullOrWhiteSpace();
        problem.Body.ShouldNotContain(SecretMessage);
        problem.Body.ShouldNotContain(nameof(InvalidOperationException));
    }

    [Theory]
    [InlineData("")]
    [InlineData("procureaccess.de")]
    [InlineData("ftp://procureaccess.de")]
    public async Task Startup_BaseUriMissingOrInvalid_Fails(string baseUri)
    {
        // The check runs while services are registered, before any database access.
        await using var factory = new ProcureAccessApiFactory(
            host.Api.ConnectionStringForNewDatabase(),
            host.Api.Clock,
            settings: new Dictionary<string, string?> { ["ProblemTypes:BaseUri"] = baseUri });

        var exception = Should.Throw<InvalidOperationException>(() => factory.Services);

        exception.Message.ShouldContain("ProblemTypes:BaseUri");
    }

    private static StringContent Json(string body) => new(body, Encoding.UTF8, "application/json");

    /// <summary>
    /// The host of this class: base URI <see cref="BaseUri"/> and <see cref="ProblemProbeController"/>.
    /// </summary>
    public sealed class Host(ApiFixture api) : IAsyncLifetime
    {
        private ProcureAccessApiFactory? _factory;

        public ApiFixture Api { get; } = api;

        public HttpClient Client { get; private set; } = null!;

        public ValueTask InitializeAsync()
        {
            _factory = new ProcureAccessApiFactory(
                Api.ConnectionStringForNewDatabase(),
                Api.Clock,
                settings: new Dictionary<string, string?> { ["ProblemTypes:BaseUri"] = BaseUri },
                configureServices: services =>
                {
                    services.AddControllers().AddApplicationPart(typeof(ProblemProbeController).Assembly);
                });
            // With the antiforgery hook (S5-07): the POST of the validation probe needs the token.
            var cookies = new CookieContainer();
            Client = _factory.CreateDefaultClient(
                IntegrationTest.BaseAddress,
                new AntiforgeryHandler(cookies),
                new CookieContainerHandler(cookies));
            return ValueTask.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            if (_factory is not null)
            {
                await _factory.DisposeAsync();
            }
        }
    }
}

/// <summary>Endpoints that produce errors <see cref="ProblemDetailsHostTests"/> checks. Test host only.</summary>
[ApiController]
[Route("test/problems")]
public sealed class ProblemProbeController : ControllerBase
{
    public const string ConflictPath = "/test/problems/conflict";
    public const string ThrowPath = "/test/problems/throw";
    public const string ValidatePath = "/test/problems/validate";

    [HttpGet("conflict")]
    public IActionResult ConflictProblem() =>
        Problem(statusCode: StatusCodes.Status409Conflict, type: ProblemTypes.Conflict);

    [HttpGet("throw")]
    public IActionResult Throw() => throw new InvalidOperationException(ProblemDetailsHostTests.SecretMessage);

    [HttpPost("validate")]
    public IActionResult Validate(ProbeBody body) => Ok(body);

    public sealed record ProbeBody(string Name);
}
