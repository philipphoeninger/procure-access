// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.RateLimiting;

namespace ProcureAccess.Modules.Platform.RateLimiting;

/// <summary>
/// The rate limits of SPEC §7.3 (ARCHITECTURE §9.2) on ASP.NET Core's rate limiter: one policy
/// per name of <see cref="RateLimitPolicies"/>, a fixed window per client IP
/// (<see cref="ClientAddress"/>) or, for the policies of signed-in actions, per user (S5-12a), held
/// in memory only. A rejected request gets 429 as Problem
/// Details of type <c>rate-limit</c> with <c>retryAfterMinutes</c> and a <c>Retry-After</c> header
/// (API-05); other modules and Platform itself add policies of their own kind (the ready check,
/// <c>Health/HealthChecks.cs</c>).
/// </summary>
/// <remarks>
/// The limiter keeps its own clock, not <see cref="TimeProvider"/>; tests therefore check the count,
/// not the passing of a window. A rejected window lease reports the whole window as the wait, so
/// <c>retryAfterMinutes</c> is an upper bound: the counter may be free earlier, never later.
/// </remarks>
internal static class RateLimitingRegistration
{
    public const string SectionName = "RateLimits";

    /// <summary>Extension member of the 429 problem that <c>errors.rateLimit</c> shows (SPEC §7.3).</summary>
    public const string RetryAfterMinutesExtension = "retryAfterMinutes";

    /// <summary>The fixed-window policies partitioned by client IP.</summary>
    private static readonly string[] ClientIpPolicies =
    [
        RateLimitPolicies.AnonymousExport,
        RateLimitPolicies.CriteriaListShown,
        RateLimitPolicies.Registration,
    ];

    /// <summary>
    /// The fixed-window policies partitioned by the signed-in user (SPEC §7.3 „user“). Their
    /// endpoints require a sign-in, and authorization runs before the limiter, so an anonymous
    /// request never reaches them; should one, it counts by its client IP.
    /// </summary>
    private static readonly string[] UserPolicies =
    [
        RateLimitPolicies.ConfirmationResend,
    ];

    public static IServiceCollection AddPlatformRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var limits = ClientIpPolicies.Concat(UserPolicies)
                                     .ToDictionary(name => name, name => FixedWindowLimit.From(configuration, name));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) => WriteRejectionAsync(context.HttpContext, context.Lease);

            foreach (var (name, limit) in limits)
            {
                var byUser = UserPolicies.Contains(name);
                options.AddPolicy(name, context => RateLimitPartition.GetFixedWindowLimiter(
                    byUser ? UserPartitionOf(context) : ClientAddress.PartitionOf(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limit.PermitLimit,
                        Window = limit.Window,
                        QueueLimit = 0,
                        AutoReplenishment = true,
                    }));
            }
        });

        return services;
    }

    /// <summary>
    /// The partition of a per-user policy: the user id of the sign-in (<c>NameIdentifier</c>), else
    /// the client IP. Prefixed, so a user id can never share a counter with an address.
    /// </summary>
    internal static string UserPartitionOf(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(userId)
            ? "ip:" + ClientAddress.PartitionOf(context)
            : "user:" + userId;
    }

    /// <summary>
    /// Whole minutes until the counter is free again, rounded up, at least 1, so the text never
    /// asks to wait less than needed.
    /// </summary>
    public static int RetryAfterMinutes(TimeSpan retryAfter) =>
        Math.Max(1, (int)Math.Ceiling(retryAfter.TotalMinutes));

    private static async ValueTask WriteRejectionAsync(HttpContext context, RateLimitLease lease)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Type = ProblemTypes.RateLimit,
            Title = "Too many requests",
            Detail = "This action was carried out too often in a short time. Try again later.",
        };

        if (lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            problem.Extensions[RetryAfterMinutesExtension] = RetryAfterMinutes(retryAfter);
            context.Response.Headers.RetryAfter =
                ((long)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        }

        var problemDetails = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetails.TryWriteAsync(new ProblemDetailsContext { HttpContext = context, ProblemDetails = problem });
    }

    /// <summary>A 🔧 limit of SPEC §7.3: <see cref="PermitLimit"/> requests per <see cref="Window"/>.</summary>
    internal sealed record FixedWindowLimit(int PermitLimit, TimeSpan Window)
    {
        /// <summary>Reads <c>RateLimits:&lt;policy&gt;</c>; a missing or impossible limit fails the start.</summary>
        public static FixedWindowLimit From(IConfiguration configuration, string policy)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            var section = configuration.GetSection($"{SectionName}:{policy}");
            var permitLimit = section.GetValue<int?>(nameof(PermitLimit));
            var window = section.GetValue<TimeSpan?>(nameof(Window));
            if (permitLimit is null or < 1 || window is null || window <= TimeSpan.Zero)
            {
                throw new InvalidOperationException(
                    $"{SectionName}:{policy} needs {nameof(PermitLimit)} >= 1 and a positive {nameof(Window)} (SPEC §7.3, e.g. 30 and 00:10:00).");
            }

            return new FixedWindowLimit(permitLimit.Value, window.Value);
        }
    }
}
