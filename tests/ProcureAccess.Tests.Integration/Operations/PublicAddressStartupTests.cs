// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// The public address of a deployment (S7-09, #447; owner decision O-S7-5 of 2026-11-26): the real
/// host does not start outside Development and E2E while <c>App:BaseUrl</c> or <c>Email:From</c>
/// still holds the default of <c>appsettings.json</c> — the links of every e-mail and their sender
/// would name another site. The rules themselves are in the unit tests (<c>ClientLinksTests</c>,
/// <c>EmailSettingsTests</c>); here the wiring: <c>Program.cs</c> runs both checks while the
/// services are registered, before any database access, like the key-ring certificate.
/// </summary>
public sealed class PublicAddressStartupTests(ApiFixture api)
{
    [Theory]
    [InlineData("Production", "App:BaseUrl", "APP_BASE_URL")]
    [InlineData("Staging", "App:BaseUrl", "APP_BASE_URL")]
    [InlineData("Production", "Email:From", "EMAIL_FROM")]
    [InlineData("Staging", "Email:From", "EMAIL_FROM")]
    public async Task Startup_DefaultPublicAddressOutsideDevelopment_Fails(string environment, string setting, string variable)
    {
        await using var factory = new ProcureAccessApiFactory(
            api.ConnectionStringForNewDatabase(),
            api.Clock,
            environment,
            new Dictionary<string, string?> { [setting] = DefaultOf(setting) });

        var exception = Should.Throw<InvalidOperationException>(() => factory.Services);

        exception.Message.ShouldContain(setting);
        exception.Message.ShouldContain(variable);
    }

    [Fact]
    public async Task Startup_DefaultPublicAddressesInDevelopment_Starts()
    {
        // RebuildDatabase stays off (the factory sets it), so Development seeds nothing here.
        await using var factory = new ProcureAccessApiFactory(
            api.ConnectionStringForNewDatabase(),
            api.Clock,
            Environments.Development,
            new Dictionary<string, string?> { ["App:BaseUrl"] = DefaultOf("App:BaseUrl"), ["Email:From"] = DefaultOf("Email:From") });

        factory.Services.GetRequiredService<IConfiguration>()["App:BaseUrl"].ShouldBe(DefaultOf("App:BaseUrl"));
    }

    /// <summary>The value of the API's <c>appsettings.json</c>, as a deployment without the variable gets it.</summary>
    private static string DefaultOf(string setting) =>
        new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false)
            .Build()[setting]
        ?? throw new InvalidOperationException($"appsettings.json has no {setting}.");
}
