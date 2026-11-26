// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.IdentityAccess.Auth;
using ProcureAccess.Modules.Platform.Email;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Operations;

/// <summary>
/// The public-address settings of a deployment (S7-09, #447; owner decision O-S7-5) have their
/// default in three places that must not drift apart: <c>appsettings.json</c>, the
/// <c>${VAR:-default}</c> of <c>docker-compose.yml</c>, and — for the two the API refuses outside
/// Development and E2E — the constant the start-up check compares with. If they differed, a
/// deployment that sets no variable would start with a default the check does not know.
/// </summary>
public partial class PublicAddressDefaultsTests
{
    [Theory]
    [InlineData("App:BaseUrl", "App__BaseUrl", "APP_BASE_URL")]
    [InlineData("Email:From", "Email__From", "EMAIL_FROM")]
    [InlineData("Email:DisplayName", "Email__DisplayName", "EMAIL_DISPLAY_NAME")]
    [InlineData("Export:ToolUrl", "Export__ToolUrl", "EXPORT_TOOL_URL")]
    [InlineData("ProblemTypes:BaseUri", "ProblemTypes__BaseUri", "PROBLEM_TYPES_BASE_URI")]
    public void Compose_PassesTheSettingWithTheDefaultOfAppSettings(string setting, string environmentKey, string variable)
    {
        var line = ComposeLine().Matches(File.ReadAllText(Path.Combine(RepositoryRoot(), "docker-compose.yml")))
            .SingleOrDefault(match => match.Groups["key"].Value == environmentKey);

        line.ShouldNotBeNull($"docker-compose.yml passes no {environmentKey}");
        line.Groups["variable"].Value.ShouldBe(variable);
        line.Groups["default"].Value.ShouldBe(AppSettings()[setting]);
        File.ReadAllText(Path.Combine(RepositoryRoot(), ".env.example")).ShouldContain(variable + "=");
    }

    [Fact]
    public void StartupChecks_CompareWithTheDefaultsOfAppSettings()
    {
        AppSettings()[ClientLinks.BaseUrlSetting].ShouldBe(ClientLinks.DefaultBaseUrl);
        AppSettings()["Email:From"].ShouldBe(EmailSettings.DefaultSenderAddress);
    }

    /// <summary>An entry of <c>api.environment</c>: <c>Key: ${VARIABLE:-default}</c>.</summary>
    [GeneratedRegex(@"^\s+(?<key>[A-Za-z_]+): \$\{(?<variable>[A-Z_]+):-(?<default>[^}]*)\}\s*$", RegexOptions.Multiline)]
    private static partial Regex ComposeLine();

    /// <summary>The API's <c>appsettings.json</c>, copied to the test output.</summary>
    private static IConfigurationRoot AppSettings() =>
        new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false)
            .Build();

    private static string RepositoryRoot()
    {
        for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder is not null; folder = folder.Parent)
        {
            if (File.Exists(Path.Combine(folder.FullName, "ProcureAccess.sln")))
            {
                return folder.FullName;
            }
        }

        throw new InvalidOperationException($"No ProcureAccess.sln above {AppContext.BaseDirectory}.");
    }
}
