// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.Analytics;

/// <inheritdoc cref="IUsageEventWriter"/>
public sealed class UsageEventWriter(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<UsageEventWriter> logger) : IUsageEventWriter
{
    /// <summary>Dimension values of SPEC §9.1, exactly as stored.</summary>
    public static class Dimensions
    {
        public const string Anonymous = "anonymous";
        public const string SignedIn = "signedIn";
        public const string Xlsx = "xlsx";
        public const string Csv = "csv";
    }

    public Task CriteriaListShownAsync(bool signedIn) =>
        WriteAsync(UsageEventType.CriteriaListShown, () => (Audience(signedIn), null));

    public Task ExportCreatedAsync(UsageExportFormat format, bool signedIn) =>
        WriteAsync(UsageEventType.ExportCreated, () => (Format(format), Audience(signedIn)));

    private async Task WriteAsync(UsageEventType eventType, Func<(string Dimension1, string? Dimension2)> dimensions)
    {
        try
        {
            var (dimension1, dimension2) = dimensions();

            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

            db.UsageEvents.Add(new UsageEvent
            {
                EventType = eventType,
                // Date only (PRV-05): the UTC day, no time of day.
                OccurredOn = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime),
                Dimension1 = dimension1,
                Dimension2 = dimension2,
            });

            // No cancellation token: the event describes something that already happened.
            await db.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            // Counting must never fail the user's request (R-MET-01). The event has no personal
            // data, so the type is all the log needs.
            logger.LogError(ex, "Writing the usage event {EventType} failed; the event is lost", eventType);
        }
    }

    private static string Audience(bool signedIn) => signedIn ? Dimensions.SignedIn : Dimensions.Anonymous;

    private static string Format(UsageExportFormat format) => format switch
    {
        UsageExportFormat.Xlsx => Dimensions.Xlsx,
        UsageExportFormat.Csv => Dimensions.Csv,
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown export format."),
    };
}
