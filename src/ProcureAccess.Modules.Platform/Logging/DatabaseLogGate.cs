// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace ProcureAccess.Modules.Platform.Logging;

/// <summary>
/// Keeps the events of the PostgreSQL log sink in memory until the host has run the migrations
/// that create <c>logging."SeriLogs"</c> (#437), then passes them on in their order and every later
/// event at once. The console sink is not behind it.
/// </summary>
/// <remarks>
/// <para>On an empty database the sink's first batch reached PostgreSQL before the schema existed
/// (<c>schema "logging" does not exist</c>, or <c>database … does not exist</c> before the legacy
/// migration creates the database). Serilog kept the batch and wrote it again after a pause, so no
/// event was lost, but every first start left an error in the database server's log.</para>
/// <para>While closed, at most <c>QueueLimit</c> events are held, as many as the sink itself keeps
/// while the database is unreachable; newer ones are dropped and counted in Serilog's self log.
/// The host opens the gate (<see cref="PlatformModule.StartDatabaseLogSink"/>); a host that never
/// opens it writes those events to the console only.</para>
/// </remarks>
internal sealed class DatabaseLogGate(int limit)
{
    private readonly Lock _sync = new();
    private Queue<LogEvent>? _held = new();
    private ILogEventSink? _sink;
    private volatile bool _open;
    private int _dropped;

    /// <summary>Whether the events go to the sink.</summary>
    public bool IsOpen => _open;

    /// <summary>Puts the gate in front of <paramref name="sink"/>; for <c>WriteTo.Wrap</c>, once.</summary>
    public ILogEventSink Wrap(ILogEventSink sink)
    {
        ArgumentNullException.ThrowIfNull(sink);
        if (Interlocked.CompareExchange(ref _sink, sink, null) is not null)
        {
            throw new InvalidOperationException("The log gate already wraps a sink.");
        }

        return new GatedSink(this);
    }

    /// <summary>Passes the held events to the sink in their order, then every new one. Opening twice changes nothing.</summary>
    public void Open()
    {
        lock (_sync)
        {
            if (_held is null)
            {
                return;
            }

            while (_held.TryDequeue(out var held))
            {
                _sink?.Emit(held);
            }

            _held = null;
            _open = true;
        }

        if (_dropped > 0)
        {
            SelfLog.WriteLine("The log gate dropped {0} events beyond its limit of {1} before the database log opened.", _dropped, limit);
        }
    }

    private void Emit(LogEvent logEvent)
    {
        if (!_open)
        {
            lock (_sync)
            {
                if (_held is not null)
                {
                    if (_held.Count < limit)
                    {
                        _held.Enqueue(logEvent);
                    }
                    else
                    {
                        _dropped++;
                    }

                    return;
                }
            }
        }

        _sink?.Emit(logEvent);
    }

    /// <summary>
    /// The sink Serilog calls. Not disposable on purpose: <c>WriteTo.Wrap</c> then disposes the
    /// wrapped sink itself, which flushes its batches when the host stops.
    /// </summary>
    private sealed class GatedSink(DatabaseLogGate gate) : ILogEventSink
    {
        public void Emit(LogEvent logEvent) => gate.Emit(logEvent);
    }
}
