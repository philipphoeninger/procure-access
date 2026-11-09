// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Platform.Persistence;

namespace ProcureAccess.Modules.Platform.Analytics;

/// <summary>What happened to a batch of usage events (see <see cref="IUsageEventStore"/>).</summary>
internal enum UsageEventWriteOutcome
{
    /// <summary>The events are stored.</summary>
    Written,

    /// <summary>
    /// The connection could not be opened, so nothing was sent to the database: the same batch can
    /// be tried again without storing an event twice.
    /// </summary>
    NotConnected,

    /// <summary>
    /// The insert was sent and failed. The database may still have committed it (a lost
    /// acknowledgement), so the batch must not be tried again (#115: no event counted twice).
    /// </summary>
    Failed,
}

/// <summary>Result of <see cref="IUsageEventStore.WriteAsync"/> with the exception, if any.</summary>
internal readonly record struct UsageEventWriteResult(UsageEventWriteOutcome Outcome, Exception? Error = null);

/// <summary>Stores a batch of usage events; used only by <see cref="UsageEventBackgroundWriter"/>.</summary>
internal interface IUsageEventStore
{
    /// <summary>
    /// Stores the events in one insert. Never throws, except <see cref="OperationCanceledException"/>
    /// when <paramref name="cancellationToken"/> is cancelled; any other failure, a timeout that
    /// surfaces as a cancellation included, is reported in the result.
    /// </summary>
    Task<UsageEventWriteResult> WriteAsync(IReadOnlyCollection<UsageEvent> events, CancellationToken cancellationToken);
}

/// <inheritdoc cref="IUsageEventStore"/>
/// <remarks>
/// Uses <see cref="PlatformDbContext"/> with options of its own <b>without</b> the retrying
/// execution strategy of the module's registration: a retry after a lost commit acknowledgement
/// would store the events twice (#115). The background writer retries only what this store
/// reports as <see cref="UsageEventWriteOutcome.NotConnected"/>.
/// </remarks>
internal sealed class UsageEventStore(DbContextOptions<PlatformDbContext> options) : IUsageEventStore
{
    /// <summary>
    /// Options for <see cref="UsageEventStore"/>: the application database, no retrying execution
    /// strategy, Npgsql's default connect and command timeouts.
    /// </summary>
    public static DbContextOptions<PlatformDbContext> OptionsFor(string connectionString) =>
        new DbContextOptionsBuilder<PlatformDbContext>().UseNpgsql(connectionString).Options;

    public async Task<UsageEventWriteResult> WriteAsync(
        IReadOnlyCollection<UsageEvent> events,
        CancellationToken cancellationToken)
    {
        await using var db = CreateContext();

        // Open the connection first: a failure here means the insert was never sent.
        try
        {
            await db.Database.OpenConnectionAsync(cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            return new UsageEventWriteResult(UsageEventWriteOutcome.NotConnected, ex);
        }

        try
        {
            db.UsageEvents.AddRange(events);
            await db.SaveChangesAsync(cancellationToken);
            return new UsageEventWriteResult(UsageEventWriteOutcome.Written);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            return new UsageEventWriteResult(UsageEventWriteOutcome.Failed, ex);
        }
    }

    /// <summary>A new context with the store's options; internal for the tests.</summary>
    internal PlatformDbContext CreateContext() => new(options);
}
