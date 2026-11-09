// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Logging;
using ProcureAccess.Modules.Platform.Contracts.Analytics;

namespace ProcureAccess.Modules.Platform.Analytics;

/// <inheritdoc cref="IUsageEventWriter"/>
/// <remarks>
/// Builds the event and puts it into <see cref="UsageEventQueue"/>; it never touches the database
/// (#115). <see cref="UsageEventBackgroundWriter"/> stores it.
/// </remarks>
internal sealed class UsageEventWriter(
    UsageEventQueue queue,
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

    public void CriteriaListShown(bool signedIn) =>
        Enqueue(UsageEventType.CriteriaListShown, () => (Audience(signedIn), null));

    public void ExportCreated(UsageExportFormat format, bool signedIn) =>
        Enqueue(UsageEventType.ExportCreated, () => (Format(format), Audience(signedIn)));

    private void Enqueue(UsageEventType eventType, Func<(string Dimension1, string? Dimension2)> dimensions)
    {
        try
        {
            var (dimension1, dimension2) = dimensions();
            var usageEvent = new UsageEvent
            {
                EventType = eventType,
                // Date only (PRV-05): the UTC day of the call, not of the later insert.
                OccurredOn = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime),
                Dimension1 = dimension1,
                Dimension2 = dimension2,
            };

            if (!queue.TryEnqueue(usageEvent, out var firstDrop) && firstDrop)
            {
                // Once per outage; UsageEventBackgroundWriter logs the number dropped when it
                // stores events again.
                logger.LogWarning(
                    "The usage-event queue is full or the API is stopping; usage events are dropped until it has room again");
            }
        }
        catch (Exception ex)
        {
            // Counting must never fail the user's request (R-MET-01). The event has no personal
            // data, so the type is all the log needs.
            logger.LogError(ex, "Recording the usage event {EventType} failed; the event is lost", eventType);
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
