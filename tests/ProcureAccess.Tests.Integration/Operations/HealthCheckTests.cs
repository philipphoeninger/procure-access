// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;

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

    private static async Task ShouldReportAsync(HttpClient client, string path, HttpStatusCode status, string body)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await client.GetAsync(path, cancellationToken);

        response.StatusCode.ShouldBe(status, path);
        (await response.Content.ReadAsStringAsync(cancellationToken)).ShouldBe(body, path);
    }
}
