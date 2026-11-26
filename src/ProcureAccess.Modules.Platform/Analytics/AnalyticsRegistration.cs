// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Contracts.Analytics;
using ProcureAccess.Modules.Platform.Contracts.Configuration;

namespace ProcureAccess.Modules.Platform.Analytics;

/// <summary>Usage events in the <c>analytics</c> schema (ARCHITECTURE §5.7, SPEC R-MET-01).</summary>
internal static class AnalyticsRegistration
{
    private const string RetryDelaySetting = $"{UsageEventSettings.SectionName}:{nameof(UsageEventSettings.RetryDelay)}";

    /// <summary>
    /// Registers the fire-and-forget writer (#115): <see cref="IUsageEventWriter"/> queues,
    /// <see cref="UsageEventBackgroundWriter"/> stores through <see cref="UsageEventStore"/>.
    /// </summary>
    public static IServiceCollection AddUsageEvents(this IServiceCollection services, string connectionString)
    {
        // The binder reads a bare number as days, so the delay is read again through the reader (#456).
        services.AddOptions<UsageEventSettings>()
            .BindConfiguration(UsageEventSettings.SectionName)
            .Configure<IConfiguration>((settings, configuration) =>
                settings.RetryDelay = DurationSetting.Read(configuration[RetryDelaySetting], RetryDelaySetting))
            .Validate(s => s.QueueCapacity > 0, "UsageEvents:QueueCapacity must be greater than 0.")
            .ValidateOnStart();

        services.AddSingleton<UsageEventQueue>();
        services.AddSingleton<IUsageEventStore>(new UsageEventStore(UsageEventStore.OptionsFor(connectionString)));
        services.AddSingleton<IUsageEventWriter, UsageEventWriter>();
        services.AddHostedService<UsageEventBackgroundWriter>();

        return services;
    }
}
