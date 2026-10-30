// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProcureAccess.Modules.Platform.Logging;
using ProcureAccess.Modules.Platform.Persistence;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// OPS-02: the retention job deletes log rows older than the configured period from the
/// <c>logging</c> schema and nothing newer (ARCHITECTURE §9.5). Time is the fake clock.
/// </summary>
/// <remarks>
/// The log sink writes into the same table while the tests run, so every test marks its own
/// rows (<see cref="SeriLogEntry.MessageTemplate"/>) and only looks at those.
/// </remarks>
public sealed class LogRetentionTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact(DisplayName = "INV:OPS-02 " + nameof(Run_RowsAroundRetentionLimit_RemovesOnlyOlderRows))]
    [Trait("Invariant", "OPS-02")]
    public async Task Run_RowsAroundRetentionLimit_RemovesOnlyOlderRows()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var marker = NewMarker();
        var now = Clock.GetUtcNow().UtcDateTime;
        var limit = now - Retention().RetentionPeriod;
        await StoreAsync(
            LogRow(marker, "older", limit.AddMinutes(-1)),
            LogRow(marker, "at-limit", limit),
            LogRow(marker, "newer", limit.AddMinutes(1)),
            LogRow(marker, "now", now));

        using var scope = Api.Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<LogRetentionJob>().RunAsync(cancellationToken);

        (await RemainingAsync(marker)).ShouldBe(["at-limit", "newer", "now"], ignoreOrder: true);
    }

    [Fact(DisplayName = "INV:OPS-02 " + nameof(HostedService_ClockAdvancedByRunInterval_RemovesRowsThatAgedOut))]
    [Trait("Invariant", "OPS-02")]
    public async Task HostedService_ClockAdvancedByRunInterval_RemovesRowsThatAgedOut()
    {
        var marker = NewMarker();
        var (period, interval) = Retention();
        var now = Clock.GetUtcNow().UtcDateTime;
        // Inside the period now, outside it once the clock has moved on by one run interval.
        var agesOut = now - period + interval / 2;
        await StoreAsync(
            LogRow(marker, "ages-out", agesOut),
            LogRow(marker, "stays", now));

        Clock.Advance(interval);

        // The hosted service runs the job on its own thread when its timer fires.
        var deadline = DateTime.UtcNow.AddSeconds(30);
        var remaining = await RemainingAsync(marker);
        while (remaining.Contains("ages-out") && DateTime.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken);
            remaining = await RemainingAsync(marker);
        }

        remaining.ShouldBe(["stays"]);
    }

    private (TimeSpan RetentionPeriod, TimeSpan RunInterval) Retention()
    {
        var settings = Api.Factory.Services
            .GetRequiredService<IOptions<AppLoggingSettings.RetentionSettings>>().Value;
        return (TimeSpan.FromDays(settings.RetentionDays), settings.RunInterval);
    }

    private static string NewMarker() => $"retention-test-{Guid.NewGuid():N}";

    private static SeriLogEntry LogRow(string marker, string message, DateTime timeStamp) => new()
    {
        TimeStamp = timeStamp,
        Level = "Warning",
        Message = message,
        MessageTemplate = marker,
    };

    private async Task<List<string>> RemainingAsync(string marker)
    {
        using var scope = Api.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        return await db.SeriLogEntries
            .Where(entry => entry.MessageTemplate == marker)
            .Select(entry => entry.Message)
            .ToListAsync(TestContext.Current.CancellationToken);
    }
}
