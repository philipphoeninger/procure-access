// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Logging;
using ProcureAccess.Modules.Platform.Analytics;
using ProcureAccess.Modules.Platform.Contracts.Analytics;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Unit.Analytics.UsageEventTestDoubles;

namespace ProcureAccess.Tests.Unit.Analytics;

/// <summary>
/// Recording a usage event must never fail or hold up the user's request (SPEC R-MET-01, #115):
/// the writer only queues the event and returns.
/// </summary>
public class UsageEventWriterTests
{
    [Fact]
    public void CriteriaListShown_Called_QueuesTheEventWithTheUtcDayOfTheCall()
    {
        var harness = new BackgroundWriterHarness();
        // 23:59:59 UTC is already the next day in Germany; the event counts for the UTC day.
        harness.Clock.SetUtcNow(new DateTimeOffset(2026, 11, 9, 23, 59, 59, TimeSpan.Zero));

        harness.Writer.CriteriaListShown(signedIn: true);
        harness.Clock.Advance(TimeSpan.FromSeconds(2));

        harness.Queue.Reader.TryRead(out var queued).ShouldBeTrue();
        queued.EventType.ShouldBe(UsageEventType.CriteriaListShown);
        queued.OccurredOn.ShouldBe(new DateOnly(2026, 11, 9));
        queued.Dimension1.ShouldBe(UsageEventWriter.Dimensions.SignedIn);
        queued.Dimension2.ShouldBeNull();
    }

    [Fact]
    public void ExportCreated_Called_QueuesFormatAndAudience()
    {
        var harness = new BackgroundWriterHarness();

        harness.Writer.ExportCreated(UsageExportFormat.Xlsx, signedIn: false);

        harness.Queue.Reader.TryRead(out var queued).ShouldBeTrue();
        queued.EventType.ShouldBe(UsageEventType.ExportCreated);
        queued.Dimension1.ShouldBe(UsageEventWriter.Dimensions.Xlsx);
        queued.Dimension2.ShouldBe(UsageEventWriter.Dimensions.Anonymous);
    }

    [Fact]
    public async Task CriteriaListShown_DatabaseNeverAnswers_ReturnsWithoutWaiting()
    {
        await using var harness = new BackgroundWriterHarness(storeNeverAnswers: true);
        await harness.StartAsync();
        harness.Writer.CriteriaListShown(signedIn: false);
        await harness.Store.NextCallAsync();

        // The background service hangs in the store; further calls still return at once.
        harness.Writer.CriteriaListShown(signedIn: false);
        harness.Writer.ExportCreated(UsageExportFormat.Csv, signedIn: true);

        harness.Queue.Count.ShouldBe(2);
        harness.Store.CallCount.ShouldBe(1);
    }

    [Fact]
    public void CriteriaListShown_QueueFull_DropsTheEventAndWarnsOnce()
    {
        var harness = new BackgroundWriterHarness(capacity: 2);

        for (var i = 0; i < 5; i++)
        {
            harness.Writer.CriteriaListShown(signedIn: false);
        }

        harness.Queue.Count.ShouldBe(2);
        harness.Queue.TakeDroppedCount().ShouldBe(3);
        LogCount(harness.WriterLogger, LogLevel.Warning).ShouldBe(1);
        LogCount(harness.WriterLogger, LogLevel.Error).ShouldBe(0);
    }

    [Fact]
    public void ExportCreated_UnknownFormat_LogsErrorAndQueuesNothing()
    {
        var harness = new BackgroundWriterHarness();

        Should.NotThrow(() => harness.Writer.ExportCreated((UsageExportFormat)99, signedIn: false));

        harness.Queue.Count.ShouldBe(0);
        LogCount(harness.WriterLogger, LogLevel.Error).ShouldBe(1);
    }
}
