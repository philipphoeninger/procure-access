// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ProcureAccess.Tests.Integration.Api;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;
using PlatformHealth = ProcureAccess.Modules.Platform.Health.HealthChecks;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// OPS-01: <c>/health/live</c> depends on nothing, <c>/health/ready</c> on the database
/// (ARCHITECTURE §9.5).
/// </summary>
public sealed class HealthCheckTests(ApiFixture api) : IntegrationTest(api)
{
    private const string LivePath = "/health/live";
    private const string ReadyPath = "/health/ready";

    [Fact(DisplayName = "INV:OPS-01 " + nameof(HealthEndpoints_DatabaseReachable_ReportHealthy))]
    [Trait("Invariant", "OPS-01")]
    public async Task HealthEndpoints_DatabaseReachable_ReportHealthy()
    {
        using var client = CreateClient();

        await ShouldReportAsync(client, LivePath, HttpStatusCode.OK, "Healthy");
        await ShouldReportAsync(client, ReadyPath, HttpStatusCode.OK, "Healthy");
    }

    [Fact(DisplayName = "INV:OPS-01 " + nameof(HealthEndpoints_DatabaseStopped_LiveHealthyReadyUnhealthy))]
    [Trait("Invariant", "OPS-01")]
    public async Task HealthEndpoints_DatabaseStopped_LiveHealthyReadyUnhealthy()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        // A container of its own, so stopping it does not affect the other tests.
        await using var database = new PostgreSqlBuilder(ApiFixture.PostgresImage).Build();
        await database.StartAsync(cancellationToken);
        await using var factory = new ProcureAccessApiFactory(database.GetConnectionString(), Clock);
        using var client = factory.CreateClient();
        await ShouldReportAsync(client, ReadyPath, HttpStatusCode.OK, "Healthy");

        await database.StopAsync(cancellationToken);

        await ShouldReportAsync(client, LivePath, HttpStatusCode.OK, "Healthy");
        await ShouldReportAsync(client, ReadyPath, HttpStatusCode.ServiceUnavailable, "Unhealthy");
    }

    [Fact]
    public async Task Ready_MoreProbesAtOnceThanAllowed_Returns429AtOnce()
    {
        // #112, owner decision O-S4-70 (a): each ready check holds a pooled connection, so at most
        // Health:ReadyConcurrentChecks run at a time; a further probe does not wait for them.
        var cancellationToken = TestContext.Current.CancellationToken;
        var gate = new GateCheck(entriesToReport: 2);
        await using var factory = new ProcureAccessApiFactory(
            Api.ConnectionString,
            Clock,
            settings: new Dictionary<string, string?> { ["Health:ReadyConcurrentChecks"] = "2" },
            configureServices: services => services.AddHealthChecks()
                .AddCheck("gate", gate, tags: [PlatformHealth.ReadyTag]));
        using var client = factory.CreateClient();

        var first = client.GetAsync(ReadyPath, cancellationToken);
        var second = client.GetAsync(ReadyPath, cancellationToken);
        await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
        using var third = await client.GetAsync(ReadyPath, cancellationToken);
        gate.Release.SetResult();

        var problem = await ProblemResponse.ReadAsync(third, cancellationToken);
        problem.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        problem.Property("type").ShouldNotBeNull().ShouldEndWith("/problems/rate-limit");
        using var firstResponse = await first;
        using var secondResponse = await second;
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        await ShouldReportAsync(client, ReadyPath, HttpStatusCode.OK, "Healthy");
    }

    [Fact]
    public async Task Ready_ConfiguredTimeout_IsTheTimeoutOfTheDatabaseCheck()
    {
        // #112: the timeout was a code constant; it is the 🔧 setting Health:ReadyTimeout.
        await using var factory = new ProcureAccessApiFactory(
            Api.ConnectionString,
            Clock,
            settings: new Dictionary<string, string?> { ["Health:ReadyTimeout"] = "00:00:03" });

        var registrations = factory.Services.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations;

        registrations.Single(r => r.Name == "database").Timeout.ShouldBe(TimeSpan.FromSeconds(3));
    }

    /// <summary>A ready check that waits until the test releases it, and reports when enough checks wait.</summary>
    private sealed class GateCheck(int entriesToReport) : IHealthCheck
    {
        private int _entries;

        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref _entries) == entriesToReport)
            {
                Entered.SetResult();
            }

            await Release.Task.WaitAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
    }

    private static async Task ShouldReportAsync(HttpClient client, string path, HttpStatusCode status, string body)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await client.GetAsync(path, cancellationToken);

        response.StatusCode.ShouldBe(status, path);
        (await response.Content.ReadAsStringAsync(cancellationToken)).ShouldBe(body, path);
    }
}
