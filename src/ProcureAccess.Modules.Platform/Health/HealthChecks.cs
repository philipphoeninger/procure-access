// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProcureAccess.Modules.Platform.Persistence;

namespace ProcureAccess.Modules.Platform.Health;

/// <summary>
/// Health endpoints for the reverse proxy and uptime monitoring (ARCHITECTURE §9.5, OPS-01):
/// <c>/health/live</c> answers as long as the process serves requests and checks nothing else;
/// <c>/health/ready</c> runs the checks tagged <see cref="ReadyTag"/>. Healthy is 200, unhealthy 503.
/// </summary>
/// <remarks>The file store check joins the ready checks once the file store exists (step 7).</remarks>
internal static class HealthChecks
{
    public const string LivePath = "/health/live";
    public const string ReadyPath = "/health/ready";
    public const string ReadyTag = "ready";

    /// <summary>A ready check that takes longer counts as unhealthy, so the probe never hangs.</summary>
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(5);

    public static IServiceCollection AddPlatformHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database", tags: [ReadyTag], timeout: CheckTimeout);

        return services;
    }

    public static IEndpointRouteBuilder MapPlatformHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks(LivePath, new HealthCheckOptions { Predicate = _ => false })
                 .AllowAnonymous();

        endpoints.MapHealthChecks(ReadyPath, new HealthCheckOptions { Predicate = check => check.Tags.Contains(ReadyTag) })
                 .AllowAnonymous();

        return endpoints;
    }

    /// <summary>Healthy when the application database accepts a connection.</summary>
    private sealed class DatabaseHealthCheck(PlatformDbContext db) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            // The connection error is logged by the health check service; the response body only
            // says "Unhealthy" and never exposes connection details.
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : new HealthCheckResult(context.Registration.FailureStatus, "The database is unreachable.");
        }
    }
}
