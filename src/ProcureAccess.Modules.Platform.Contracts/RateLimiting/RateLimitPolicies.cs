// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.RateLimiting;

/// <summary>
/// Names of the rate-limit policies of SPEC §7.3 that Platform registers (ARCHITECTURE §9.2). A
/// module attaches one to its endpoint with <c>RequireRateLimiting(&lt;name&gt;)</c>; a rejected
/// request gets 429 as Problem Details of type <c>rate-limit</c> with <c>retryAfterMinutes</c>
/// (API-05). The limits are 🔧 settings under <c>RateLimits:&lt;policy&gt;</c>.
/// </summary>
public static class RateLimitPolicies
{
    /// <summary><c>POST /api/v1/exports</c>: per client IP, 30 per 10 minutes 🔧.</summary>
    public const string AnonymousExport = "AnonymousExport";

    /// <summary><c>POST /api/v1/configurator/criteria-list-shown</c>: per client IP, 30 per 10 minutes 🔧 (O-S4-56 (b)).</summary>
    public const string CriteriaListShown = "CriteriaListShown";
}
