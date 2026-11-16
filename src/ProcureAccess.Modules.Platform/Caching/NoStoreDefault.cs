// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace ProcureAccess.Modules.Platform.Caching;

/// <summary>
/// TEN-04 workspace part (ARCHITECTURE §9.6; S5-22, owner decision S5-22 3b): every answer below
/// <c>/api/v1</c> that sets no <c>Cache-Control</c> of its own leaves with <c>no-store</c>, so no
/// browser or proxy keeps a saved list, a member list or a session answer. An endpoint that may be
/// cached sets its own header (the catalog, ARCHITECTURE §9.6), and this default leaves it alone.
/// </summary>
/// <remarks>
/// A middleware, not an endpoint filter: it also covers the answers no endpoint writes — 401 and 403
/// of authentication, 404 of routing, 429 of the rate limiter and 400 of antiforgery. The header is
/// set when the answer starts, after everything later in the pipeline had its say. The legacy
/// controllers at <c>/api/&lt;controller&gt;</c> keep their behaviour until their step replaces them.
/// </remarks>
internal static class NoStoreDefault
{
    /// <summary>The versioned API whose answers get the default.</summary>
    public static readonly PathString VersionedApi = new("/api/v1");

    public const string Value = "no-store";

    public static IApplicationBuilder UseNoStoreDefault(this IApplicationBuilder app) =>
        app.Use(static (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments(VersionedApi))
            {
                context.Response.OnStarting(static state =>
                {
                    var headers = ((HttpResponse)state).Headers;
                    if (string.IsNullOrEmpty(headers.CacheControl))
                    {
                        headers.CacheControl = Value;
                    }

                    return Task.CompletedTask;
                }, context.Response);
            }

            return next(context);
        });
}
