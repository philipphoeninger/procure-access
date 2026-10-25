// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SERVICES.ProcureAccess.Analytics;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Analytics;

/// <summary>
/// Writing a usage event must never fail the user's request (SPEC R-MET-01): a failure is logged
/// as an error and swallowed.
/// </summary>
public class UsageEventWriterTests
{
    [Fact]
    public async Task CriteriaListShown_DatabaseUnavailable_LogsErrorAndDoesNotThrow()
    {
        var (writer, logger) = WriterWithFailingDatabase();

        await Should.NotThrowAsync(() => writer.CriteriaListShownAsync(signedIn: true));

        ErrorLogCount(logger).ShouldBe(1);
    }

    [Fact]
    public async Task ExportCreated_DatabaseUnavailable_LogsErrorAndDoesNotThrow()
    {
        var (writer, logger) = WriterWithFailingDatabase();

        await Should.NotThrowAsync(() => writer.ExportCreatedAsync(UsageExportFormat.Csv, signedIn: false));

        ErrorLogCount(logger).ShouldBe(1);
    }

    private static (UsageEventWriter Writer, ILogger<UsageEventWriter> Logger) WriterWithFailingDatabase()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Throws(new InvalidOperationException("database unavailable"));
        var logger = Substitute.For<ILogger<UsageEventWriter>>();
        return (new UsageEventWriter(scopeFactory, TimeProvider.System, logger), logger);
    }

    // ILogger.Log<TState> is generic over an internal state type, so count the calls instead of
    // matching them with Received().
    private static int ErrorLogCount(ILogger logger) => logger.ReceivedCalls()
        .Count(call => call.GetMethodInfo().Name == nameof(ILogger.Log)
            && call.GetArguments()[0] is LogLevel.Error
            && call.GetArguments()[3] is InvalidOperationException);
}
