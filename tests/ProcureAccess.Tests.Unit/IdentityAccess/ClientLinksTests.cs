// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using ProcureAccess.Modules.IdentityAccess.Auth;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.IdentityAccess;

/// <summary>
/// The address of the client in the e-mail links (S7-09, #447; owner decision O-S7-5): outside
/// Development and E2E the API does not start with the <c>App:BaseUrl</c> of <c>appsettings.json</c>,
/// like the key-ring certificate (<see cref="KeyRingCertificatesTests"/>).
/// </summary>
public class ClientLinksTests
{
    [Theory]
    [InlineData("Production", "http://procureaccess.tech:4200")]
    [InlineData("Staging", "http://procureaccess.tech:4200/")]
    [InlineData("Testing", "HTTP://ProcureAccess.tech:4200")]
    [InlineData("SomethingElse", "http://procureaccess.tech:4200")]
    public void EnsureDeploymentAddress_DefaultOutsideDevelopmentAndE2E_StopsTheStart(string environment, string baseUrl) =>
        Should.Throw<InvalidOperationException>(() => ClientLinks.EnsureDeploymentAddress(Settings(baseUrl), In(environment)))
            .Message.ShouldContain("APP_BASE_URL");

    [Theory]
    [InlineData("Development")]
    [InlineData("E2E")]
    public void EnsureDeploymentAddress_DefaultInDevelopmentOrE2E_Starts(string environment) =>
        Should.NotThrow(() => ClientLinks.EnsureDeploymentAddress(Settings(ClientLinks.DefaultBaseUrl), In(environment)));

    [Theory]
    [InlineData("https://staging.example.test")]
    [InlineData("https://procureaccess.tech")]
    [InlineData("https://example.test/procureaccess/")]
    public void EnsureDeploymentAddress_OwnAddressOutsideDevelopment_Starts(string baseUrl) =>
        Should.NotThrow(() => ClientLinks.EnsureDeploymentAddress(Settings(baseUrl), In("Production")));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("procureaccess.example")]
    [InlineData("/app")]
    [InlineData("ftp://procureaccess.example")]
    public void EnsureDeploymentAddress_NoAbsoluteAddress_StopsTheStartInEveryEnvironment(string? baseUrl)
    {
        Should.Throw<InvalidOperationException>(() => ClientLinks.EnsureDeploymentAddress(Settings(baseUrl), In("Development")))
            .Message.ShouldContain(ClientLinks.BaseUrlSetting);
        Should.Throw<InvalidOperationException>(() => ClientLinks.EnsureDeploymentAddress(Settings(baseUrl), In("Production")))
            .Message.ShouldContain(ClientLinks.BaseUrlSetting);
    }

    [Theory]
    [InlineData("https://staging.example.test", "https://staging.example.test/passwort/neu?token=abc")]
    [InlineData("https://example.test/procureaccess/", "https://example.test/procureaccess/passwort/neu?token=abc")]
    public void To_BaseUrlWithOrWithoutPath_BuildsTheLink(string baseUrl, string expected) =>
        ClientLinks.To(Settings(baseUrl), ClientLinks.PasswordResetPagePath + "?token=abc").AbsoluteUri.ShouldBe(expected);

    private static IHostEnvironment In(string environmentName) => new HostingEnvironment { EnvironmentName = environmentName };

    private static IConfiguration Settings(string? baseUrl) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [ClientLinks.BaseUrlSetting] = baseUrl })
            .Build();
}
