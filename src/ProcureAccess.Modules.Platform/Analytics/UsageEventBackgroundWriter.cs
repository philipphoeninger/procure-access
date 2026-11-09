// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProcureAccess.Modules.Platform.Analytics;

/// <summary>
/// Stores the events of <see cref="UsageEventQueue"/> in batches (#115, owner decisions D1–D3 of
/// S3-23). While the database cannot be reached it keeps the batch and tries again every
/// <see cref="UsageEventSettings.RetryDelay"/>; the queue meanwhile takes new events up to its
/// capacity. An insert that was sent and failed is never repeated, so no event is counted twice.
/// </summary>
/// <remarks>
/// The waits run on <see cref="TimeProvider"/>, so tests control them with <c>FakeTimeProvider</c>.
/// The deployment runs a single API instance (ARCHITECTURE §10), so the queue is per process.
/// When the host stops, the queue takes no further events and the ones in it are stored until the
/// host's shutdown timeout; what is left then is logged as lost.
/// </remarks>
internal sealed class UsageEventBackgroundWriter(
    UsageEventQueue queue,
    IUsageEventStore store,
    TimeProvider timeProvider,
    IOptions<UsageEventSettings> options,
    ILogger<UsageEventBackgroundWriter> logger) : BackgroundService
{
    /// <summary>Most events in one insert.</summary>
    internal const int MaxBatchSize = 100;

    private bool _unreachable;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<UsageEvent>(MaxBatchSize);
        try
        {
            // Ends when the queue is completed (host stopping) and empty.
            while (await queue.Reader.WaitToReadAsync(stoppingToken))
            {
                while (batch.Count < MaxBatchSize && queue.Reader.TryRead(out var usageEvent))
                {
                    batch.Add(usageEvent);
                }

                try
                {
                    await WriteAsync(batch, stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    // An unhandled exception would stop the whole host (BackgroundService
                    // default); counting must never do that (R-MET-01). Drop the batch, go on.
                    logger.LogError(ex, "Storing {EventCount} usage events failed unexpectedly; they are lost", batch.Count);
                }

                batch.Clear();
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            var lost = batch.Count + queue.Count;
            if (lost > 0)
            {
                logger.LogWarning(
                    "{LostCount} usage events were not stored before the API stopped; they are lost",
                    lost);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // Take no further events and let the loop store the queued ones until the shutdown
        // timeout; base.StopAsync then cancels whatever is still running.
        queue.Complete();
        if (ExecuteTask is { } running)
        {
            try
            {
                await running.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Shutdown timeout reached.
            }
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task WriteAsync(List<UsageEvent> batch, CancellationToken stoppingToken)
    {
        while (true)
        {
            var result = await store.WriteAsync(batch, stoppingToken);
            switch (result.Outcome)
            {
                case UsageEventWriteOutcome.Written:
                    if (_unreachable)
                    {
                        _unreachable = false;
                        logger.LogWarning("Usage events are stored again; the database is reachable");
                    }

                    LogDropped();
                    return;

                case UsageEventWriteOutcome.Failed:
                    // Not retried: the database may have committed the insert (#115).
                    logger.LogError(
                        result.Error,
                        "Storing {EventCount} usage events failed after the insert was sent; they are not retried and may be lost",
                        batch.Count);
                    LogDropped();
                    return;

                case UsageEventWriteOutcome.NotConnected:
                    if (!_unreachable)
                    {
                        // Once per outage, not once per attempt.
                        _unreachable = true;
                        logger.LogError(
                            result.Error,
                            "The database is unreachable; usage events wait in the queue and are tried again every {RetryDelay}",
                            options.Value.RetryDelay);
                    }

                    await Task.Delay(options.Value.RetryDelay, timeProvider, stoppingToken);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown outcome {result.Outcome}.");
            }
        }
    }

    private void LogDropped()
    {
        var dropped = queue.TakeDroppedCount();
        if (dropped > 0)
        {
            logger.LogWarning("{DroppedCount} usage events were dropped while the queue was full", dropped);
        }
    }
}
