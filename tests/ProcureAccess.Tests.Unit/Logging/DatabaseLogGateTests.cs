// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Logging;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Logging;

/// <summary>
/// The PostgreSQL log sink holds its events until the host has run the migrations (#437): nothing
/// reaches the sink before the gate opens, nothing held is lost or reordered when it opens.
/// </summary>
public sealed class DatabaseLogGateTests
{
    [Fact]
    public void Emit_BeforeOpen_ReachesTheSinkOnlyWhenTheGateOpensInItsOrder()
    {
        var sink = new CollectingSink();
        var gate = new DatabaseLogGate(limit: 10);
        var gated = gate.Wrap(sink);

        gated.Emit(Event("first"));
        gated.Emit(Event("second"));

        sink.Messages.ShouldBeEmpty();
        gate.IsOpen.ShouldBeFalse();

        gate.Open();
        gated.Emit(Event("third"));

        gate.IsOpen.ShouldBeTrue();
        sink.Messages.ShouldBe(["first", "second", "third"]);
    }

    [Fact]
    public void Emit_BeyondTheLimitBeforeOpen_KeepsTheOldestEvents()
    {
        var sink = new CollectingSink();
        var gate = new DatabaseLogGate(limit: 2);
        var gated = gate.Wrap(sink);

        gated.Emit(Event("first"));
        gated.Emit(Event("second"));
        gated.Emit(Event("third"));
        gate.Open();

        sink.Messages.ShouldBe(["first", "second"]);
    }

    [Fact]
    public void Open_Twice_PassesTheHeldEventsOnce()
    {
        var sink = new CollectingSink();
        var gate = new DatabaseLogGate(limit: 10);
        var gated = gate.Wrap(sink);
        gated.Emit(Event("held"));

        gate.Open();
        gate.Open();

        sink.Messages.ShouldBe(["held"]);
    }

    [Fact]
    public void Wrap_SecondSink_IsRefused()
    {
        var gate = new DatabaseLogGate(limit: 10);
        gate.Wrap(new CollectingSink());

        Should.Throw<InvalidOperationException>(() => gate.Wrap(new CollectingSink()));
    }

    [Fact]
    public void AddPlatformLogging_WithAConnectionString_RegistersAClosedGate()
    {
        using var services = new ServiceCollection().AddPlatformLogging(Configuration("Host=localhost;Database=unused")).BuildServiceProvider();

        services.GetRequiredService<DatabaseLogGate>().IsOpen.ShouldBeFalse();
    }

    [Fact]
    public void AddPlatformLogging_WithoutAConnectionString_HasNoGate()
    {
        using var services = new ServiceCollection().AddPlatformLogging(Configuration(connectionString: null)).BuildServiceProvider();

        services.GetService<DatabaseLogGate>().ShouldBeNull();
    }

    private static LogEvent Event(string text) =>
        new(DateTimeOffset.UnixEpoch, LogEventLevel.Warning, null, new MessageTemplateParser().Parse(text), []);

    private static IConfiguration Configuration(string? connectionString) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppLoggingSettings:General:RestrictedToMinimumLevel"] = "Warning",
                ["AppLoggingSettings:PostgreSQL:ConnectionStringName"] = "ProcureAccess",
                ["AppLoggingSettings:PostgreSQL:QueueLimit"] = "100",
                ["ConnectionStrings:ProcureAccess"] = connectionString,
            })
            .Build();

    private sealed class CollectingSink : ILogEventSink
    {
        public List<string> Messages { get; } = [];

        public void Emit(LogEvent logEvent) => Messages.Add(logEvent.RenderMessage());
    }
}
