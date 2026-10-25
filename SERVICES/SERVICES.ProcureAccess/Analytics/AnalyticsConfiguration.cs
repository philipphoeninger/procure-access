// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.Analytics;

public static class AnalyticsConfiguration
{
    /// <summary>
    /// Registers <see cref="IUsageEventWriter"/> (ARCHITECTURE §5.7). Singleton: the writer keeps
    /// no state and opens a scope of its own for every event.
    /// </summary>
    public static IServiceCollection AddUsageEvents(this IServiceCollection services)
    {
        services.AddSingleton<IUsageEventWriter, UsageEventWriter>();

        return services;
    }
}
