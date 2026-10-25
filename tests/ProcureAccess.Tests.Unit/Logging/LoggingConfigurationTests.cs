// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using SERVICES.ProcureAccess.Logging.Configuration;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Logging;

/// <summary>
/// <see cref="LoggingConfiguration.ConfigureSerilog"/> rejects settings that would crash later or
/// fill the log table unnoticed (ARCHITECTURE §9.5). Building the logger does not connect to the
/// database, so these tests need none.
/// </summary>
public class LoggingConfigurationTests
{
    private static readonly Dictionary<string, string?> ValidSettings = new()
    {
        ["ConnectionStrings:ProcureAccess"] = "Host=localhost;Database=unit_test_never_connects",
        ["AppLoggingSettings:General:RestrictedToMinimumLevel"] = "Warning",
        ["AppLoggingSettings:PostgreSQL:ConnectionStringName"] = "ProcureAccess",
        ["AppLoggingSettings:PostgreSQL:QueueLimit"] = "10000",
    };

    [Fact]
    public void ConfigureSerilog_ValidSettings_Succeeds()
    {
        var builder = CreateBuilder(ValidSettings);

        Should.NotThrow(builder.ConfigureSerilog);
    }

    [Fact]
    public void ConfigureSerilog_SectionMissing_ThrowsNamingTheSection()
    {
        var builder = CreateBuilder(new Dictionary<string, string?>());

        Should.Throw<InvalidOperationException>(builder.ConfigureSerilog)
            .Message.ShouldContain("AppLoggingSettings");
    }

    [Theory]
    [InlineData("Verbos")]
    [InlineData("")]
    [InlineData("42")]
    public void ConfigureSerilog_UnknownMinimumLevel_Throws(string level)
    {
        var builder = CreateBuilder(With("AppLoggingSettings:General:RestrictedToMinimumLevel", level));

        Should.Throw<InvalidOperationException>(builder.ConfigureSerilog)
            .Message.ShouldContain("RestrictedToMinimumLevel");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("-1")]
    public void ConfigureSerilog_QueueLimitMissingOrNotPositive_Throws(string? queueLimit)
    {
        var builder = CreateBuilder(With("AppLoggingSettings:PostgreSQL:QueueLimit", queueLimit));

        Should.Throw<InvalidOperationException>(builder.ConfigureSerilog)
            .Message.ShouldContain("QueueLimit");
    }

    private static Dictionary<string, string?> With(string key, string? value)
    {
        var settings = new Dictionary<string, string?>(ValidSettings);
        if (value is null)
        {
            settings.Remove(key);
        }
        else
        {
            settings[key] = value;
        }

        return settings;
    }

    private static WebApplicationBuilder CreateBuilder(Dictionary<string, string?> settings)
    {
        // No appsettings files and no environment variables: only the given settings count.
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Configuration.AddInMemoryCollection(settings);
        return builder;
    }
}
