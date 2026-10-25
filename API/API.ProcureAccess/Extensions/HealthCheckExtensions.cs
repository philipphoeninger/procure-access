// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.ProcureAccess.Extensions;

/// <summary>
/// Health endpoints for the reverse proxy and uptime monitoring (ARCHITECTURE §9.5, OPS-01):
/// <c>/health/live</c> answers as long as the process serves requests and checks nothing else;
/// <c>/health/ready</c> runs the checks tagged <see cref="ReadyTag"/>. Healthy is 200, unhealthy 503.
/// </summary>
/// <remarks>The file store check joins the ready checks once the file store exists (step 7).</remarks>
public static class HealthCheckExtensions
{
    public const string LivePath = "/health/live";
    public const string ReadyPath = "/health/ready";
    public const string ReadyTag = "ready";

    /// <summary>A ready check that takes longer counts as unhealthy, so the probe never hangs.</summary>
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(5);

    public static IServiceCollection AddProcureAccessHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database", tags: [ReadyTag], timeout: CheckTimeout);

        return services;
    }

    public static WebApplication MapProcureAccessHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks(LivePath, new HealthCheckOptions { Predicate = _ => false })
           .AllowAnonymous();

        app.MapHealthChecks(ReadyPath, new HealthCheckOptions { Predicate = check => check.Tags.Contains(ReadyTag) })
           .AllowAnonymous();

        return app;
    }

    /// <summary>Healthy when the application database accepts a connection.</summary>
    internal sealed class DatabaseHealthCheck(ApplicationDBContext db) : IHealthCheck
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
