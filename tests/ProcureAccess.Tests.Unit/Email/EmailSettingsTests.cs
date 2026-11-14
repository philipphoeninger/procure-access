// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using ProcureAccess.Modules.Platform.Email;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Email;

/// <summary>
/// The SMTP settings (S5-11, SPEC §7.6, owner decision 4a): only Development and E2E may send without
/// credentials or without TLS; every other environment stops at startup without them.
/// </summary>
public class EmailSettingsTests
{
    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    [InlineData("Testing")]
    public void From_NoCredentialsOutsideDevelopmentAndE2E_StopsTheStart(string environment) =>
        Should.Throw<InvalidOperationException>(() => EmailSettings.From(Settings(), In(environment)))
            .Message.ShouldContain("Email:Smtp:Username");

    [Theory]
    [InlineData("Development")]
    [InlineData("E2E")]
    public void From_NoCredentialsInDevelopmentOrE2E_SendsWithoutSignIn(string environment)
    {
        var settings = EmailSettings.From(Settings(("Email:Smtp:Host", "mailpit"), ("Email:Smtp:Port", "1025"), ("Email:Smtp:Security", "None")), In(environment));

        settings.Host.ShouldBe("mailpit");
        settings.Port.ShouldBe(1025);
        settings.Security.ShouldBe(SmtpSecurity.None);
        settings.Username.ShouldBeNull();
        settings.Password.ShouldBeNull();
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public void From_NoTlsOutsideDevelopmentAndE2E_StopsTheStart(string environment) =>
        Should.Throw<InvalidOperationException>(() => EmailSettings.From(Settings(Credentials, ("Email:Smtp:Security", "None")), In(environment)))
            .Message.ShouldContain("Security None");

    [Fact]
    public void From_CredentialsAndDefaults_StartTlsWithSenderName()
    {
        var settings = EmailSettings.From(Settings(Credentials), In("Production"));

        settings.Host.ShouldBe("smtp.example.test");
        settings.Port.ShouldBe(587);
        settings.Security.ShouldBe(SmtpSecurity.StartTls);
        settings.Username.ShouldBe("api-token");
        settings.Password.ShouldBe("secret-password");
        settings.Sender.Address.ShouldBe("noreply@example.test");
        settings.Sender.Name.ShouldBe("ProcureAccess");
    }

    [Theory]
    [InlineData("Email:Smtp:Host", "")]
    [InlineData("Email:Smtp:Port", "0")]
    [InlineData("Email:Smtp:Port", "smtp")]
    [InlineData("Email:Smtp:Security", "Opportunistic")]
    [InlineData("Email:From", "")]
    [InlineData("Email:From", "not an address")]
    public void From_ImpossibleValue_StopsTheStartInEveryEnvironment(string key, string value)
    {
        Should.Throw<InvalidOperationException>(() => EmailSettings.From(Settings(Credentials, (key, value)), In("Development")));
        Should.Throw<InvalidOperationException>(() => EmailSettings.From(Settings(Credentials, (key, value)), In("Production")));
    }

    [Theory]
    [InlineData("Email:Smtp:Username")]
    [InlineData("Email:Smtp:Password")]
    public void From_OnlyOneOfUsernameAndPassword_StopsTheStart(string missing) =>
        Should.Throw<InvalidOperationException>(() => EmailSettings.From(Settings(Credentials, (missing, "")), In("Development")));

    [Fact]
    public void From_RefusedSettings_NeverQuoteTheValues()
    {
        var exception = Should.Throw<InvalidOperationException>(() =>
            EmailSettings.From(Settings(("Email:Smtp:Username", "api-token"), ("Email:Smtp:Password", "secret-password"), ("Email:Smtp:Security", "None")), In("Production")));

        exception.Message.ShouldNotContain("secret-password");
        exception.Message.ShouldNotContain("api-token");
    }

    private static readonly (string, string)[] Credentials =
    [
        ("Email:Smtp:Username", "api-token"),
        ("Email:Smtp:Password", "secret-password"),
    ];

    private static IHostEnvironment In(string environmentName) => new HostingEnvironment { EnvironmentName = environmentName };

    private static IConfiguration Settings(params (string Key, string Value)[] values) => Settings([], values);

    private static IConfiguration Settings((string Key, string Value)[] first, params (string Key, string Value)[] more)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Email:Smtp:Host"] = "smtp.example.test",
            ["Email:Smtp:Port"] = "587",
            ["Email:From"] = "noreply@example.test",
            ["Email:DisplayName"] = "ProcureAccess",
        };
        foreach (var (key, value) in first.Concat(more))
        {
            settings[key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }
}
