// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MODELS.ProcureAccess.Entities;
using ProcureAccess.Tests.Integration.Infrastructure;
using SERVICES.ProcureAccess.Analytics;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Analytics;

/// <summary>
/// The usage-event writer against the real table (SPEC R-MET-01, §9.1; ARCHITECTURE §5.7):
/// fixed dimension values, the UTC day of the fake clock, its own database context, and no
/// exception for the caller.
/// </summary>
public sealed class UsageEventWriterTests(ApiFixture api) : IntegrationTest(api)
{
    public static TheoryData<string, string, string?> Events => new()
    {
        { "list-anonymous", UsageEventWriter.Dimensions.Anonymous, null },
        { "list-signedIn", UsageEventWriter.Dimensions.SignedIn, null },
        { "xlsx-anonymous", UsageEventWriter.Dimensions.Xlsx, UsageEventWriter.Dimensions.Anonymous },
        { "csv-signedIn", UsageEventWriter.Dimensions.Csv, UsageEventWriter.Dimensions.SignedIn },
    };

    [Theory]
    [MemberData(nameof(Events))]
    public async Task Write_EachEventOfSpec_StoresFixedDimensionsAndTodaysDate(
        string call, string expectedDimension1, string? expectedDimension2)
    {
        await DeleteAllEventsAsync();
        var expectedType = call.StartsWith("list", StringComparison.Ordinal)
            ? UsageEventType.CriteriaListShown
            : UsageEventType.ExportCreated;

        await (call switch
        {
            "list-anonymous" => Writer.CriteriaListShownAsync(signedIn: false),
            "list-signedIn" => Writer.CriteriaListShownAsync(signedIn: true),
            "xlsx-anonymous" => Writer.ExportCreatedAsync(UsageExportFormat.Xlsx, signedIn: false),
            "csv-signedIn" => Writer.ExportCreatedAsync(UsageExportFormat.Csv, signedIn: true),
            _ => throw new ArgumentOutOfRangeException(nameof(call)),
        });

        var stored = (await AllEventsAsync()).ShouldHaveSingleItem();
        stored.EventType.ShouldBe(expectedType);
        stored.OccurredOn.ShouldBe(DateOnly.FromDateTime(Clock.GetUtcNow().UtcDateTime));
        stored.Dimension1.ShouldBe(expectedDimension1);
        stored.Dimension2.ShouldBe(expectedDimension2);
    }

    [Fact]
    public async Task Write_ShortlyBeforeAndAfterMidnightUtc_StoresTheUtcDay()
    {
        await DeleteAllEventsAsync();
        var now = Clock.GetUtcNow();
        var midnight = new DateTimeOffset(now.UtcDateTime.Date.AddDays(1), TimeSpan.Zero);

        // 23:59:59 UTC is already the next day in Germany; the event still counts for the UTC day.
        Clock.Advance(midnight - now - TimeSpan.FromSeconds(1));
        await Writer.CriteriaListShownAsync(signedIn: false);
        Clock.Advance(TimeSpan.FromSeconds(2));
        await Writer.CriteriaListShownAsync(signedIn: false);

        (await AllEventsAsync()).Select(stored => stored.OccurredOn).ShouldBe(
            [DateOnly.FromDateTime(midnight.UtcDateTime.AddDays(-1)), DateOnly.FromDateTime(midnight.UtcDateTime)],
            ignoreOrder: true);
    }

    [Fact]
    public async Task Write_CallerHasUnsavedChanges_LeavesThemUnsavedAndTracked()
    {
        await DeleteAllEventsAsync();
        var marker = $"usage-event-test-{Guid.NewGuid():N}";
        using var requestScope = Api.Factory.Services.CreateScope();
        var requestDb = requestScope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var unsaved = new SeriLogEntry
        {
            TimeStamp = Clock.GetUtcNow().UtcDateTime,
            Level = "Warning",
            Message = marker,
            MessageTemplate = marker,
        };
        requestDb.SeriLogEntries.Add(unsaved);

        await Writer.CriteriaListShownAsync(signedIn: true);

        (await AllEventsAsync()).ShouldHaveSingleItem();
        requestDb.Entry(unsaved).State.ShouldBe(EntityState.Added);
        using var checkScope = Api.Factory.Services.CreateScope();
        (await checkScope.ServiceProvider.GetRequiredService<ApplicationDBContext>().SeriLogEntries
            .AnyAsync(entry => entry.MessageTemplate == marker, TestContext.Current.CancellationToken))
            .ShouldBeFalse();
    }

    [Fact]
    public async Task Write_UnknownExportFormat_DoesNotThrowAndStoresNothing()
    {
        await DeleteAllEventsAsync();

        await Should.NotThrowAsync(() => Writer.ExportCreatedAsync((UsageExportFormat)99, signedIn: false));

        (await AllEventsAsync()).ShouldBeEmpty();
    }

    private IUsageEventWriter Writer => Api.Factory.Services.GetRequiredService<IUsageEventWriter>();

    private async Task DeleteAllEventsAsync()
    {
        using var scope = Api.Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDBContext>().UsageEvents
            .ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    private async Task<List<UsageEvent>> AllEventsAsync()
    {
        using var scope = Api.Factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDBContext>().UsageEvents
            .AsNoTracking()
            .ToListAsync(TestContext.Current.CancellationToken);
    }
}
