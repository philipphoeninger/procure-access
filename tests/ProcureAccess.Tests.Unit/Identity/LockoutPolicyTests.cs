// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using API.ProcureAccess.Auth;
using API.ProcureAccess.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Identity;

/// <summary>
/// The 🔧 lockout of SPEC R-AUTH-03 (S5-13a, ARCHITECTURE §9.1): 5 failed sign-ins, 15 minutes,
/// read from configuration into Identity's options. The lock itself runs on the fake clock in
/// <c>PasswordResetTests</c> (SEC-11).
/// </summary>
public sealed class LockoutPolicyTests
{
    [Fact]
    public void IdentityOptions_ConfiguredLockout_IsSet()
    {
        var services = new ServiceCollection().ConfigureIdentityOptions(Configuration("4", "00:20:00"));
        var lockout = services.BuildServiceProvider().GetRequiredService<IOptions<IdentityOptions>>().Value.Lockout;

        lockout.MaxFailedAccessAttempts.ShouldBe(4);
        lockout.DefaultLockoutTimeSpan.ShouldBe(TimeSpan.FromMinutes(20));
        lockout.AllowedForNewUsers.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null, "00:15:00", LockoutPolicy.MaxFailedAttemptsSetting)]
    [InlineData("0", "00:15:00", LockoutPolicy.MaxFailedAttemptsSetting)]
    [InlineData("five", "00:15:00", LockoutPolicy.MaxFailedAttemptsSetting)]
    [InlineData("5", null, LockoutPolicy.DurationSetting)]
    [InlineData("5", "00:00:00", LockoutPolicy.DurationSetting)]
    [InlineData("5", "a while", LockoutPolicy.DurationSetting)]
    public void From_MissingOrImpossibleValue_StopsTheStart(string? attempts, string? duration, string setting) =>
        Should.Throw<InvalidOperationException>(() => LockoutPolicy.From(Configuration(attempts, duration)))
              .Message.ShouldContain(setting);

    [Fact]
    public void Defaults_OfAppsettings_AreFiveAttemptsAndFifteenMinutes()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false)
            .Build();

        LockoutPolicy.From(configuration).ShouldBe(new LockoutPolicy(5, TimeSpan.FromMinutes(15)));
    }

    private static IConfiguration Configuration(string? attempts, string? duration) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [PasswordPolicy.MinLengthSetting] = "12",
                [LockoutPolicy.MaxFailedAttemptsSetting] = attempts,
                [LockoutPolicy.DurationSetting] = duration,
            })
            .Build();
}
