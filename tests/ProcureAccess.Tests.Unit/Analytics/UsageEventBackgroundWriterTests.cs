// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Logging;
using ProcureAccess.Modules.Platform.Analytics;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Unit.Analytics.UsageEventTestDoubles;

namespace ProcureAccess.Tests.Unit.Analytics;

/// <summary>
/// The background service that stores queued usage events (#115, S3-23 D1–D3), on the fake clock:
/// retry after <see cref="UsageEventSettings.RetryDelay"/> only when nothing was sent, never a
/// second insert of the same events, bounded memory, and the queue emptied on shutdown.
/// </summary>
public class UsageEventBackgroundWriterTests
{
    private static readonly TimeSpan OneTick = TimeSpan.FromTicks(1);

    [Fact]
    public async Task Write_DatabaseNotReached_TriesTheSameBatchAgainAfterRetryDelay()
    {
        await using var harness = new BackgroundWriterHarness([UsageEventWriteOutcome.NotConnected]);
        harness.Enqueue(AnEvent("first"));
        await harness.StartAsync();

        var first = await harness.Store.NextCallAsync();
        (await harness.Clock.NextTimerAsync()).ShouldBe(RetryDelay);
        harness.Clock.Advance(RetryDelay - OneTick);
        harness.Store.CallCount.ShouldBe(1);
        harness.Clock.Advance(OneTick);
        var second = await harness.Store.NextCallAsync();

        second.Events.ShouldBe(first.Events);
        (second.At - first.At).ShouldBe(RetryDelay);
        harness.Store.Stored.Select(e => e.Dimension1).ShouldBe(["first"]);
    }

    [Fact]
    public async Task Write_DatabaseNotReachedThreeTimes_LogsTheOutageOnceAndTheRecoveryOnce()
    {
        await using var harness = new BackgroundWriterHarness(
            [UsageEventWriteOutcome.NotConnected, UsageEventWriteOutcome.NotConnected, UsageEventWriteOutcome.NotConnected]);
        harness.Enqueue(AnEvent());
        await harness.StartAsync();

        for (var attempt = 0; attempt < 3; attempt++)
        {
            await harness.Store.NextCallAsync();
            await harness.Clock.NextTimerAsync();
            harness.Clock.Advance(RetryDelay);
        }

        await harness.Store.NextCallAsync();
        await WaitUntilAsync(() => LogCount(harness.ServiceLogger, LogLevel.Warning) == 1);

        harness.Store.CallCount.ShouldBe(4);
        harness.Store.Stored.Count.ShouldBe(1);
        LogCount(harness.ServiceLogger, LogLevel.Error).ShouldBe(1);
    }

    [Fact]
    public async Task Write_InsertSentAndFailed_DoesNotStoreTheBatchAgain()
    {
        await using var harness = new BackgroundWriterHarness([UsageEventWriteOutcome.Failed]);
        harness.Enqueue(AnEvent("lost"));
        await harness.StartAsync();
        await harness.Store.NextCallAsync();

        // The database may have committed the failed insert (#115): the next call carries only
        // the next event, and no retry timer was started.
        harness.Enqueue(AnEvent("next"));
        var next = await harness.Store.NextCallAsync();

        next.Events.Select(e => e.Dimension1).ShouldBe(["next"]);
        harness.Store.Stored.Select(e => e.Dimension1).ShouldBe(["next"]);
        harness.Clock.Advance(RetryDelay);
        harness.Store.CallCount.ShouldBe(2);
        LogCount(harness.ServiceLogger, LogLevel.Error).ShouldBe(1);
    }

    [Fact]
    public async Task Write_QueueFullDuringOutage_KeepsCapacityAndLogsTheDroppedCountAfterRecovery()
    {
        await using var harness = new BackgroundWriterHarness([UsageEventWriteOutcome.NotConnected], capacity: 1);
        harness.Enqueue(AnEvent("in batch"));
        await harness.StartAsync();
        await harness.Store.NextCallAsync();
        await harness.Clock.NextTimerAsync();

        // The batch waits for the retry; the queue takes one more event and drops the rest.
        for (var i = 0; i < 3; i++)
        {
            harness.Writer.CriteriaListShown(signedIn: false);
        }

        harness.Queue.Count.ShouldBe(1);
        harness.Clock.Advance(RetryDelay);
        await harness.Store.NextCallAsync();
        await harness.Store.NextCallAsync();

        harness.Store.Stored.Count.ShouldBe(2);
        await WaitUntilAsync(() => LoggedValues(harness.ServiceLogger, "DroppedCount").Any());
        LoggedValues(harness.ServiceLogger, "DroppedCount").ShouldBe([2L]);
        LogCount(harness.WriterLogger, LogLevel.Warning).ShouldBe(1);
    }

    [Fact]
    public async Task Write_StoreThrowsUnexpectedly_DropsThatBatchAndKeepsRunning()
    {
        // A cancellation that is not the host's (e.g. an internal timeout) must not end the loop:
        // an unhandled exception in a BackgroundService stops the whole API.
        await using var harness = new BackgroundWriterHarness(storeFirstCallThrows: new TaskCanceledException("not ours"));
        harness.Enqueue(AnEvent("lost"));
        await harness.StartAsync();
        await harness.Store.NextCallAsync();

        harness.Enqueue(AnEvent("next"));
        await harness.Store.NextCallAsync();

        harness.Store.Stored.Select(e => e.Dimension1).ShouldBe(["next"]);
        harness.Service.ExecuteTask.ShouldNotBeNull().IsCompleted.ShouldBeFalse();
        LogCount(harness.ServiceLogger, LogLevel.Error).ShouldBe(1);
    }

    [Fact]
    public async Task Write_MoreEventsThanOneBatch_StoresThemInBatchesOfMaxBatchSize()
    {
        await using var harness = new BackgroundWriterHarness();
        for (var i = 0; i < UsageEventBackgroundWriter.MaxBatchSize + 1; i++)
        {
            harness.Enqueue(AnEvent());
        }

        await harness.StartAsync();

        (await harness.Store.NextCallAsync()).Events.Count.ShouldBe(UsageEventBackgroundWriter.MaxBatchSize);
        (await harness.Store.NextCallAsync()).Events.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Stop_EventsQueued_StoresThemAndTakesNoFurtherEvents()
    {
        var harness = new BackgroundWriterHarness();
        for (var i = 0; i < 3; i++)
        {
            harness.Enqueue(AnEvent());
        }

        await harness.StartAsync();
        await harness.Service.StopAsync(CancellationToken.None);

        harness.Store.Stored.Count.ShouldBe(3);
        harness.Writer.CriteriaListShown(signedIn: false);
        harness.Queue.Count.ShouldBe(0);
        LogCount(harness.ServiceLogger, LogLevel.Warning).ShouldBe(0);
        harness.Service.Dispose();
    }

    [Fact]
    public async Task Stop_DatabaseUnreachableUntilShutdownTimeout_LogsTheLostEvents()
    {
        await using var harness = new BackgroundWriterHarness([UsageEventWriteOutcome.NotConnected]);
        harness.Enqueue(AnEvent());
        harness.Enqueue(AnEvent());
        await harness.StartAsync();
        await harness.Store.NextCallAsync();
        await harness.Clock.NextTimerAsync();

        await harness.StopNowAsync();

        harness.Store.Stored.ShouldBeEmpty();
        LoggedValues(harness.ServiceLogger, "LostCount").ShouldBe([2]);
    }

    /// <summary>Waits in real time for something the background service does after a store call.</summary>
    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var patience = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        patience.CancelAfter(Patience);
        while (!condition())
        {
            await Task.Delay(10, patience.Token);
        }
    }
}
