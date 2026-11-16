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

    /// <summary><c>POST /api/v1/auth/register</c>: per client IP, 5 per hour 🔧 (S5-12a).</summary>
    public const string Registration = "Registration";

    /// <summary>
    /// <c>POST /api/v1/auth/confirm-email/resend</c>: per signed-in user, 5 per hour 🔧 (S5-12a); the
    /// first policy partitioned by user, not by client IP.
    /// </summary>
    public const string ConfirmationResend = "ConfirmationResend";

    /// <summary><c>POST /api/v1/auth/password-reset</c>: per client IP, 5 per hour 🔧 (S5-13a).</summary>
    public const string PasswordReset = "PasswordReset";

    /// <summary>
    /// <c>POST /api/v1/auth/password-reset</c>: per e-mail address, 3 per hour 🔧 (S5-13a). The address
    /// is in the body, which the rate-limit middleware cannot read, so the endpoint asks
    /// <see cref="IKeyedRateLimiter"/> with it after binding.
    /// </summary>
    public const string PasswordResetAddress = "PasswordResetAddress";

    /// <summary>
    /// <c>POST /api/v1/organizations/{orgId}/invitations</c>: per signed-in user, 5 per hour 🔧
    /// (SPEC §7.3 „invitation“; S5-17a, owner decision S5-17 D8 (a): its own counter, not the one of
    /// <see cref="ConfirmationResend"/>).
    /// </summary>
    public const string Invitation = "Invitation";

    /// <summary>
    /// <c>POST /api/v1/configurations/{id}/exports</c>: per signed-in user, 60 per 10 minutes 🔧
    /// (SPEC §7.3 „Saved export“; S5-21a).
    /// </summary>
    public const string SavedExport = "SavedExport";
}
