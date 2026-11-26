// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;
using ProcureAccess.Modules.IdentityAccess.Accounts;
using ProcureAccess.Modules.IdentityAccess.Auth;
using ProcureAccess.Modules.IdentityAccess.Organizations;
using ProcureAccess.Modules.IdentityAccess.Session;
using ProcureAccess.Modules.Platform.Analytics;
using ProcureAccess.Modules.Platform.Contracts.Configuration;
using ProcureAccess.Modules.Platform.Email;
using ProcureAccess.Modules.Platform.Health;
using ProcureAccess.Modules.Platform.Logging;
using ProcureAccess.Modules.Platform.RateLimiting;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Configuration;

/// <summary>
/// The one reader of the duration settings (#456, TECH_DEBT TD-94): only <c>hh:mm:ss</c> or
/// <c>d.hh:mm:ss</c> count, a bare number fails the start instead of meaning days, and every 🔧
/// <see cref="TimeSpan"/> setting of the API goes through it.
/// </summary>
public sealed class DurationSettingTests
{
    [Theory]
    [InlineData("00:00:15", 0, 0, 0, 15, 0)]
    [InlineData("00:30:00", 0, 0, 30, 0, 0)]
    [InlineData("08:00:00", 0, 8, 0, 0, 0)]
    [InlineData("1.00:00:00", 1, 0, 0, 0, 0)]
    [InlineData("14.00:00:00", 14, 0, 0, 0, 0)]
    [InlineData("00:00:00.5", 0, 0, 0, 0, 500)]
    [InlineData(" 00:15:00 ", 0, 0, 15, 0, 0)]
    public void Read_HoursMinutesSecondsWithOrWithoutDays_IsTheDuration(
        string value, int days, int hours, int minutes, int seconds, int milliseconds) =>
        DurationSetting.Read(value, "Email:Smtp:Timeout").ShouldBe(new TimeSpan(days, hours, minutes, seconds, milliseconds));

    [Theory]
    [InlineData("15")] // the binder's 15 days
    [InlineData("1")]
    [InlineData("00:15")] // hours and minutes only
    [InlineData("1.00:00")]
    [InlineData("24:00:00")]
    [InlineData("00:60:00")]
    [InlineData("-00:00:15")]
    [InlineData("15s")]
    [InlineData("PT15S")]
    [InlineData("")]
    [InlineData(null)]
    public void Read_ABareNumberOrAnotherForm_FailsTheStartNamingTheSetting(string? value) =>
        Should.Throw<InvalidOperationException>(() => DurationSetting.Read(value, "Email:Smtp:Timeout"))
            .Message.ShouldSatisfyAllConditions(
                m => m.ShouldContain("Email:Smtp:Timeout"),
                m => m.ShouldContain("hh:mm:ss"));

    [Fact]
    public void Read_Zero_FailsUnlessTheSettingAllowsIt()
    {
        Should.Throw<InvalidOperationException>(() => DurationSetting.Read("00:00:00", "Health:ReadyTimeout"));
        DurationSetting.Read("00:00:00", "Session:SecurityStampValidationInterval", zeroAllowed: true).ShouldBe(TimeSpan.Zero);
    }

    /// <summary>Each 🔧 duration setting of the API, read with <c>"15"</c> where the shipped value stands.</summary>
    public static TheoryData<string> Settings =>
    [
        "Session:IdleTimeout",
        "Session:AbsoluteTimeout",
        "Session:WarningLead",
        "Session:SecurityStampValidationInterval",
        "Account:Tokens:EmailConfirmationLifetime",
        "Account:Tokens:PasswordResetLifetime",
        "Account:Tokens:CleanupInterval",
        "Account:Lockout:Duration",
        "Organizations:Invitations:Lifetime",
        "Organizations:Invitations:CleanupInterval",
        "Email:Smtp:Timeout",
        "Health:ReadyTimeout",
        "RateLimits:SignIn:Window",
        "AppLoggingSettings:Retention:RunInterval",
        "UsageEvents:RetryDelay",
    ];

    [Fact]
    public void Settings_ShippedValues_AreRead() =>
        Should.NotThrow(() => ReadAll(AppSettings()));

    [Theory]
    [MemberData(nameof(Settings))]
    public void Settings_ABareNumber_FailsTheStartNamingTheSetting(string setting)
    {
        var configuration = AppSettings((setting, "15"));

        Should.Throw<InvalidOperationException>(() => ReadAll(configuration)).Message.ShouldContain(setting);
    }

    /// <summary>Reads every settings class with a duration as the start does.</summary>
    private static void ReadAll(IConfiguration configuration)
    {
        SessionSettings.From(configuration);
        AccountTokenSettings.From(configuration);
        LockoutPolicy.From(configuration);
        InvitationSettings.From(configuration);
        EmailSettings.From(configuration, new HostingEnvironment { EnvironmentName = "Development" });
        HealthSettings.From(configuration);
        RateLimitingRegistration.FixedWindowLimit.From(configuration, "SignIn");

        var services = new ServiceCollection()
            .AddSingleton(configuration)
            .AddLogRetention()
            .AddUsageEvents("Host=localhost;Database=unused")
            .BuildServiceProvider();
        _ = services.GetRequiredService<IOptions<AppLoggingSettings.RetentionSettings>>().Value;
        _ = services.GetRequiredService<IOptions<UsageEventSettings>>().Value;
    }

    /// <summary>The shipped <c>appsettings.json</c> of the API with <paramref name="overrides"/>.</summary>
    private static IConfiguration AppSettings(params (string Key, string? Value)[] overrides) =>
        new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false)
            .AddInMemoryCollection(overrides.ToDictionary(o => o.Key, o => o.Value))
            .Build();
}
