// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Extensions;

/// <summary>
/// Configuration sections of the legacy code. No CORS policy since S5-08 (#317): every client —
/// the release build behind nginx, the E2E stack and the dev server through its proxy — calls the
/// API on its own origin, and the session cookie is <c>SameSite=Strict</c> (ARCHITECTURE §9.1).
/// </summary>
public static class AppConfigExtensions
{
    public static IServiceCollection AddEmailConfig(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<EmailSettings>(config.GetSection("Email"));
        return services;
    }
}
