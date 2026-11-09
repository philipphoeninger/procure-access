// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using ProcureAccess.Modules.Platform.Analytics;
using Shouldly;

namespace ProcureAccess.Tests.Unit.Analytics;

/// <summary>Test doubles for the usage-event writer and its background service (#115, S3-23).</summary>
internal static class UsageEventTestDoubles
{
    public static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    /// <summary>Guard in real time, so a broken test fails instead of hanging.</summary>
    public static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    public static UsageEventQueue Queue(int capacity = 1000) =>
        new(Settings(capacity));

    public static IOptions<UsageEventSettings> Settings(int capacity = 1000) =>
        Options.Create(new UsageEventSettings { QueueCapacity = capacity, RetryDelay = RetryDelay });

    public static UsageEvent AnEvent(string dimension1 = "anonymous") => new()
    {
        EventType = UsageEventType.CriteriaListShown,
        OccurredOn = new DateOnly(2026, 11, 9),
        Dimension1 = dimension1,
    };

    // ILogger.Log<TState> is generic over an internal state type, so count the calls instead of
    // matching them with Received().
    public static int LogCount(ILogger logger, LogLevel level) => LogCalls(logger, level).Count();

    /// <summary>Values of the structured property <paramref name="name"/> in the logged messages.</summary>
    public static IEnumerable<object?> LoggedValues(ILogger logger, string name) => logger.ReceivedCalls()
        .Where(call => call.GetMethodInfo().Name == nameof(ILogger.Log))
        .Select(call => call.GetArguments()[2])
        .OfType<IEnumerable<KeyValuePair<string, object?>>>()
        .SelectMany(state => state)
        .Where(pair => pair.Key == name)
        .Select(pair => pair.Value);

    private static IEnumerable<NSubstitute.Core.ICall> LogCalls(ILogger logger, LogLevel level) => logger.ReceivedCalls()
        .Where(call => call.GetMethodInfo().Name == nameof(ILogger.Log) && call.GetArguments()[0] is LogLevel l && l == level);
}

/// <summary>A <see cref="FakeTimeProvider"/> that reports every timer, so a test knows a wait has begun.</summary>
internal sealed class TimerAwareClock() : FakeTimeProvider(new DateTimeOffset(2026, 11, 9, 12, 0, 0, TimeSpan.Zero))
{
    private readonly Channel<TimeSpan> _timers = Channel.CreateUnbounded<TimeSpan>();

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = base.CreateTimer(callback, state, dueTime, period);
        _timers.Writer.TryWrite(dueTime);
        return timer;
    }

    /// <summary>Waits until the next timer is created and returns its due time.</summary>
    public async Task<TimeSpan> NextTimerAsync()
    {
        using var patience = new CancellationTokenSource(UsageEventTestDoubles.Patience);
        return await _timers.Reader.ReadAsync(patience.Token);
    }
}

/// <summary>
/// An <see cref="IUsageEventStore"/> that answers with the given outcomes in order, then
/// <see cref="UsageEventWriteOutcome.Written"/>, and records every call with the clock's time.
/// </summary>
internal sealed class FakeUsageEventStore(TimeProvider clock, params UsageEventWriteOutcome[] outcomes) : IUsageEventStore
{
    private readonly ConcurrentQueue<UsageEventWriteOutcome> _outcomes = new(outcomes);
    private readonly Channel<StoreCall> _calls = Channel.CreateUnbounded<StoreCall>();
    private int _callCount;

    /// <summary>When set, every call waits until it is cancelled (a database that never answers).</summary>
    public bool NeverAnswers { get; init; }

    /// <summary>When set, the first call throws it (a bug or an exception the store did not expect).</summary>
    public Exception? FirstCallThrows { get; init; }

    public int CallCount => Volatile.Read(ref _callCount);

    public ConcurrentQueue<UsageEvent> Stored { get; } = new();

    public async Task<UsageEventWriteResult> WriteAsync(IReadOnlyCollection<UsageEvent> events, CancellationToken cancellationToken)
    {
        var call = Interlocked.Increment(ref _callCount);
        _calls.Writer.TryWrite(new StoreCall([.. events], clock.GetUtcNow()));

        if (call == 1 && FirstCallThrows is { } exception)
        {
            throw exception;
        }

        if (NeverAnswers)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }

        var outcome = _outcomes.TryDequeue(out var next) ? next : UsageEventWriteOutcome.Written;
        if (outcome == UsageEventWriteOutcome.Written)
        {
            foreach (var usageEvent in events)
            {
                Stored.Enqueue(usageEvent);
            }

            return new UsageEventWriteResult(outcome);
        }

        return new UsageEventWriteResult(outcome, new InvalidOperationException($"store answered {outcome}"));
    }

    /// <summary>Waits for the next call of <see cref="WriteAsync"/>.</summary>
    public async Task<StoreCall> NextCallAsync()
    {
        using var patience = new CancellationTokenSource(UsageEventTestDoubles.Patience);
        return await _calls.Reader.ReadAsync(patience.Token);
    }

    public sealed record StoreCall(IReadOnlyList<UsageEvent> Events, DateTimeOffset At);
}

/// <summary>The writer, its queue and the background service on one fake clock and one fake store.</summary>
internal sealed class BackgroundWriterHarness : IAsyncDisposable
{
    public BackgroundWriterHarness(
        UsageEventWriteOutcome[]? outcomes = null,
        int capacity = 1000,
        bool storeNeverAnswers = false,
        Exception? storeFirstCallThrows = null)
    {
        Queue = UsageEventTestDoubles.Queue(capacity);
        Store = new FakeUsageEventStore(Clock, outcomes ?? [])
        {
            NeverAnswers = storeNeverAnswers,
            FirstCallThrows = storeFirstCallThrows,
        };
        Writer = new UsageEventWriter(Queue, Clock, WriterLogger);
        Service = new UsageEventBackgroundWriter(Queue, Store, Clock, UsageEventTestDoubles.Settings(capacity), ServiceLogger);
    }

    public TimerAwareClock Clock { get; } = new();

    public UsageEventQueue Queue { get; }

    public FakeUsageEventStore Store { get; }

    public UsageEventWriter Writer { get; }

    public UsageEventBackgroundWriter Service { get; }

    public ILogger<UsageEventWriter> WriterLogger { get; } = Substitute.For<ILogger<UsageEventWriter>>();

    public ILogger<UsageEventBackgroundWriter> ServiceLogger { get; } = Substitute.For<ILogger<UsageEventBackgroundWriter>>();

    public Task StartAsync() => Service.StartAsync(CancellationToken.None);

    public void Enqueue(UsageEvent usageEvent) => Queue.TryEnqueue(usageEvent, out _).ShouldBeTrue();

    /// <summary>Stops at once (shutdown timeout already over) and waits until the loop has ended.</summary>
    public async Task StopNowAsync()
    {
        await Service.StopAsync(new CancellationToken(canceled: true));
        if (Service.ExecuteTask is { } running)
        {
            await running.WaitAsync(UsageEventTestDoubles.Patience);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopNowAsync();
        Service.Dispose();
    }
}
