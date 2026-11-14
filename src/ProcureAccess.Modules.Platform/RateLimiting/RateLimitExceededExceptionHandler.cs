// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using ProcureAccess.Modules.Platform.Contracts.RateLimiting;

namespace ProcureAccess.Modules.Platform.RateLimiting;

/// <summary>
/// Answers a <see cref="RateLimitExceededException"/> of a keyed policy with the same 429 as the
/// middleware's policies (API-05; S5-13a).
/// </summary>
internal sealed class RateLimitExceededExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not RateLimitExceededException refused)
        {
            return false;
        }

        await RateLimitingRegistration.WriteRejectionAsync(httpContext, refused.RetryAfter);
        return true;
    }
}
