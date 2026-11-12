// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace ProcureAccess.Tests.Integration.Infrastructure;

/// <summary>
/// A logger provider that keeps every log event of the API host at every level, as text: the
/// formatted message, each structured value and the exception with its inner exceptions. For the
/// privacy invariants on logs (EXP-05): a sentinel that is not in <see cref="Text"/> reached no log
/// sink, whatever level the sinks are set to. Hand-written instead of a sink package (owner
/// decision O-S4-59 (a)); add it with <c>AddLogging</c> and its own filter rule.
/// </summary>
internal sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<string> _events = new();

    /// <summary>Every event so far, one block of text each.</summary>
    public IReadOnlyCollection<string> Events => _events;

    /// <summary>All events as one text, for a substring search.</summary>
    public string Text => string.Join("\n", _events);

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, _events);

    public void Dispose()
    {
    }

    private sealed class CapturingLogger(string category, ConcurrentQueue<string> events) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            events.Enqueue($"{category} scope: {Values(state)}");
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);
            events.Enqueue($"{logLevel} {category}: {formatter(state, exception)} | {Values(state)} | {exception}");
        }

        private static string Values<TState>(TState state) =>
            state is IEnumerable<KeyValuePair<string, object?>> pairs
                ? string.Join("; ", pairs.Select(p => $"{p.Key}={p.Value}"))
                : state?.ToString() ?? "";
    }
}
