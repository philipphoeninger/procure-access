// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Contracts.Analytics;

namespace ProcureAccess.Modules.Platform.Analytics;

/// <summary>Usage events in the <c>analytics</c> schema (ARCHITECTURE §5.7, SPEC R-MET-01).</summary>
internal static class AnalyticsRegistration
{
    /// <summary>
    /// Registers the fire-and-forget writer (#115): <see cref="IUsageEventWriter"/> queues,
    /// <see cref="UsageEventBackgroundWriter"/> stores through <see cref="UsageEventStore"/>.
    /// </summary>
    public static IServiceCollection AddUsageEvents(this IServiceCollection services, string connectionString)
    {
        services.AddOptions<UsageEventSettings>()
            .BindConfiguration(UsageEventSettings.SectionName)
            .Validate(s => s.QueueCapacity > 0, "UsageEvents:QueueCapacity must be greater than 0.")
            .Validate(s => s.RetryDelay > TimeSpan.Zero, "UsageEvents:RetryDelay must be greater than 0.")
            .ValidateOnStart();

        services.AddSingleton<UsageEventQueue>();
        services.AddSingleton<IUsageEventStore>(new UsageEventStore(UsageEventStore.OptionsFor(connectionString)));
        services.AddSingleton<IUsageEventWriter, UsageEventWriter>();
        services.AddHostedService<UsageEventBackgroundWriter>();

        return services;
    }
}
