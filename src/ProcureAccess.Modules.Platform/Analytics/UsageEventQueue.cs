// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace ProcureAccess.Modules.Platform.Analytics;

/// <summary>
/// Bounded in-memory queue between <see cref="UsageEventWriter"/> (request path) and
/// <see cref="UsageEventBackgroundWriter"/> (#115, owner decision D1 of S3-23). Events in it are
/// lost when the process ends without having written them; the background writer logs that.
/// </summary>
internal sealed class UsageEventQueue
{
    private readonly Channel<UsageEvent> _channel;
    private long _dropped;

    public UsageEventQueue(IOptions<UsageEventSettings> options)
    {
        // Wait mode: TryWrite answers false when the queue is full instead of dropping an older event.
        _channel = Channel.CreateBounded<UsageEvent>(new BoundedChannelOptions(options.Value.QueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });
    }

    public ChannelReader<UsageEvent> Reader => _channel.Reader;

    /// <summary>Number of events waiting in the queue.</summary>
    public int Count => _channel.Reader.Count;

    /// <summary>
    /// Queues the event without waiting. When the queue is full or completed, the event is dropped
    /// and counted.
    /// </summary>
    /// <param name="firstDrop">
    /// True for the first event dropped since <see cref="TakeDroppedCount"/> was last called, so that
    /// a full queue is logged once and not once per event.
    /// </param>
    /// <returns>True when the event was queued.</returns>
    public bool TryEnqueue(UsageEvent usageEvent, out bool firstDrop)
    {
        if (_channel.Writer.TryWrite(usageEvent))
        {
            firstDrop = false;
            return true;
        }

        firstDrop = Interlocked.Increment(ref _dropped) == 1;
        return false;
    }

    /// <summary>Returns the number of events dropped since the last call and resets it.</summary>
    public long TakeDroppedCount() => Interlocked.Exchange(ref _dropped, 0);

    /// <summary>Accepts no further events; the reader ends once the queued ones are read.</summary>
    public void Complete() => _channel.Writer.TryComplete();
}
