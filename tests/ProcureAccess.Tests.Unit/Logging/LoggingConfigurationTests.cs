// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Logging;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Logging;

/// <summary>
/// <see cref="LoggingRegistration.AddPlatformLogging"/> rejects settings that would crash later or
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
    public void AddPlatformLogging_ValidSettings_Succeeds()
    {
        var configuration = CreateConfiguration(ValidSettings);

        Should.NotThrow(() => new ServiceCollection().AddPlatformLogging(configuration));
    }

    [Fact]
    public void AddPlatformLogging_SectionMissing_ThrowsNamingTheSection()
    {
        var configuration = CreateConfiguration(new Dictionary<string, string?>());

        Should.Throw<InvalidOperationException>(() => new ServiceCollection().AddPlatformLogging(configuration))
            .Message.ShouldContain("AppLoggingSettings");
    }

    [Theory]
    [InlineData("Verbos")]
    [InlineData("")]
    [InlineData("42")]
    public void AddPlatformLogging_UnknownMinimumLevel_Throws(string level)
    {
        var configuration = CreateConfiguration(With("AppLoggingSettings:General:RestrictedToMinimumLevel", level));

        Should.Throw<InvalidOperationException>(() => new ServiceCollection().AddPlatformLogging(configuration))
            .Message.ShouldContain("RestrictedToMinimumLevel");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("-1")]
    public void AddPlatformLogging_QueueLimitMissingOrNotPositive_Throws(string? queueLimit)
    {
        var configuration = CreateConfiguration(With("AppLoggingSettings:PostgreSQL:QueueLimit", queueLimit));

        Should.Throw<InvalidOperationException>(() => new ServiceCollection().AddPlatformLogging(configuration))
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

    // No appsettings files and no environment variables: only the given settings count.
    private static IConfiguration CreateConfiguration(Dictionary<string, string?> settings) =>
        new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
}
