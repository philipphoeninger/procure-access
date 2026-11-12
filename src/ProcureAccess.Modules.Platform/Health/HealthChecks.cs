// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProcureAccess.Modules.Platform.Persistence;

namespace ProcureAccess.Modules.Platform.Health;

/// <summary>
/// Health endpoints for the reverse proxy and uptime monitoring (ARCHITECTURE §9.5, OPS-01):
/// <c>/health/live</c> answers as long as the process serves requests and checks nothing else;
/// <c>/health/ready</c> runs the checks tagged <see cref="ReadyTag"/>. Healthy is 200, unhealthy 503.
/// </summary>
/// <remarks>
/// <para>The file store check joins the ready checks once the file store exists (step 7).</para>
/// <para><b>Limit (#112, SPEC §7.3, owner decision O-S4-70 (a)).</b> Each ready check takes a
/// database connection from the pool the requests use. So at most
/// <see cref="HealthSettings.ReadyConcurrentChecks"/> (🔧, default 2) ready checks run at a time,
/// over all callers — monitors and healthchecks share few addresses, so a limit per IP would not
/// protect the pool —, and a further probe gets 429 at once instead of waiting. A check that
/// takes longer than <see cref="HealthSettings.ReadyTimeout"/> (🔧, default 5 s) counts as
/// unhealthy, so a probe never holds its connection longer.</para>
/// </remarks>
internal static class HealthChecks
{
    public const string LivePath = "/health/live";
    public const string ReadyPath = "/health/ready";
    public const string ReadyTag = "ready";

    /// <summary>Rate-limit policy of <see cref="ReadyPath"/>; only Platform uses it.</summary>
    public const string ReadyRateLimitPolicy = "HealthReady";

    public static IServiceCollection AddPlatformHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = HealthSettings.From(configuration);

        services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database", tags: [ReadyTag], timeout: settings.ReadyTimeout);

        services.AddRateLimiter(options => options.AddConcurrencyLimiter(ReadyRateLimitPolicy, limiter =>
        {
            limiter.PermitLimit = settings.ReadyConcurrentChecks;
            limiter.QueueLimit = 0;
        }));

        return services;
    }

    public static IEndpointRouteBuilder MapPlatformHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks(LivePath, new HealthCheckOptions { Predicate = _ => false })
                 .AllowAnonymous();

        endpoints.MapHealthChecks(ReadyPath, new HealthCheckOptions { Predicate = check => check.Tags.Contains(ReadyTag) })
                 .AllowAnonymous()
                 .RequireRateLimiting(ReadyRateLimitPolicy);

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
