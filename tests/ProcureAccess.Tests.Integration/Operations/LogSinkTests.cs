// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MODELS.ProcureAccess.Entities;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// The Serilog PostgreSQL sink writes application log events into <c>logging."SeriLogs"</c>
/// (ARCHITECTURE §9.5), the table the migrations create and the retention job cleans up.
/// </summary>
public sealed class LogSinkTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact]
    public async Task LogWarning_ThroughApplicationLogger_IsStoredInLoggingSchema()
    {
        var marker = $"sink-test-{Guid.NewGuid():N}";
        var logger = Api.Factory.Services.GetRequiredService<ILogger<LogSinkTests>>();

        logger.LogWarning("Log sink test {Marker}", marker);

        var entry = await WaitForEntryAsync(Api.Factory.Services, marker);
        entry.Level.ShouldBe("Warning");
        entry.MessageTemplate.ShouldBe("Log sink test {Marker}");
        entry.SourceContext.ShouldBe(typeof(LogSinkTests).FullName);
        entry.TimeStamp.ShouldBe(DateTime.UtcNow, TimeSpan.FromMinutes(5));
        using var properties = JsonDocument.Parse(entry.Properties!);
        properties.RootElement.GetProperty("Marker").GetString().ShouldBe(marker);

        // appsettings.json sets the default application name for every environment (issue #121).
        var appName = Api.Factory.Services.GetRequiredService<IConfiguration>()["AppName"];
        appName.ShouldNotBeNullOrWhiteSpace();
        entry.ApplicationName.ShouldBe(appName);
    }

    [Fact]
    public async Task LogWarning_AppNameConfigured_StoresAppNameAsApplicationName()
    {
        // A name that differs from the assembly name: WebApplicationFactory sets the host setting
        // "applicationName" to the assembly name, which once hid the wrong key (issue #121).
        const string appName = "log-sink-test-app";
        await using var factory = new ProcureAccessApiFactory(
            Api.ConnectionStringForNewDatabase(),
            Api.Clock,
            settings: new Dictionary<string, string?> { ["AppName"] = appName });
        var marker = $"sink-test-{Guid.NewGuid():N}";
        var logger = factory.Services.GetRequiredService<ILogger<LogSinkTests>>();

        logger.LogWarning("Log sink test {Marker}", marker);

        var entry = await WaitForEntryAsync(factory.Services, marker);
        entry.ApplicationName.ShouldBe(appName);
    }

    /// <summary>The sink writes in batches on a background thread, so the row arrives later.</summary>
    private static async Task<SeriLogEntry> WaitForEntryAsync(IServiceProvider services, string marker)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var deadline = DateTime.UtcNow.AddSeconds(30);
        var entry = await db.SeriLogEntries.AsNoTracking()
            .SingleOrDefaultAsync(e => e.Message.Contains(marker), cancellationToken);
        while (entry is null && DateTime.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
            entry = await db.SeriLogEntries.AsNoTracking()
                .SingleOrDefaultAsync(e => e.Message.Contains(marker), cancellationToken);
        }

        entry.ShouldNotBeNull("the log event did not arrive in logging.\"SeriLogs\"");
        return entry;
    }
}
