// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProcureAccess.Modules.Platform.Logging;

/// <summary>
/// Runs <see cref="LogRetentionJob"/> once at startup and then every
/// <see cref="AppLoggingSettings.RetentionSettings.RunInterval"/>. The timer runs on
/// <see cref="TimeProvider"/>, so tests control it with <c>FakeTimeProvider</c>.
/// </summary>
/// <remarks>
/// The deployment runs a single API instance (ARCHITECTURE §10), so there is no coordination
/// between instances. A failed run is logged and retried at the next interval; it never stops
/// the host.
/// </remarks>
internal sealed class LogRetentionService(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    IOptions<AppLoggingSettings.RetentionSettings> options,
    ILogger<LogRetentionService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.RunInterval, timeProvider);

        do
        {
            await RunOnceAsync(stoppingToken);
        }
        while (await WaitForNextRunAsync(timer, stoppingToken));
    }

    private static async Task<bool> WaitForNextRunAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<LogRetentionJob>().RunAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is stopping.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Log retention failed; next attempt in {RunInterval}", options.Value.RunInterval);
        }
    }
}
