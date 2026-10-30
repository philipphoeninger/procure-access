// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.Logging.Configuration;

/// <summary>
/// The legacy logging wrapper <see cref="IAppLogging{T}"/> of the legacy controllers. Serilog itself,
/// its sinks and the log retention are set up by the Platform module (S2-07).
/// </summary>
public static class LoggingConfiguration
{
    public static IServiceCollection RegisterLoggingInterfaces(
        this IServiceCollection services)
    {
        services.AddScoped(typeof(IAppLogging<>), typeof(AppLogging<>));

        return services;
    }
}
