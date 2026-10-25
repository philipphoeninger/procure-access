// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using ProcureAccess.Tests.Shared.Auth;

namespace ProcureAccess.Tests.Integration.Infrastructure;

/// <summary>
/// Hosts the real API in the <c>Testing</c> environment against the test container
/// (TESTING §3.1, §5.2). Created once per test run by <see cref="ApiFixture"/>.
/// </summary>
/// <remarks>
/// Tests of startup behaviour create their own instance with another
/// <paramref name="environmentName"/> or extra host <paramref name="settings"/>
/// (e.g. SEC-06) and dispose it themselves.
/// </remarks>
public sealed class ProcureAccessApiFactory(
    string connectionString,
    FakeTimeProvider clock,
    string environmentName = ProcureAccessApiFactory.EnvironmentName,
    IReadOnlyDictionary<string, string?>? settings = null)
    : WebApplicationFactory<Program>
{
    public const string EnvironmentName = "Testing";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environmentName);

        // Program.cs reads these values while the builder is still being set up, so they must be
        // host settings (UseSetting); values added through ConfigureAppConfiguration arrive too late.
        builder.UseSetting("ConnectionStrings:ProcureAccess", connectionString);
        builder.UseSetting("RebuildDatabase", "false");

        // Secrets never live in the repository (CLAUDE.md §10): a fresh random key per test run.
        builder.UseSetting("JWTSettings:Key", Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)));

        foreach (var (key, value) in settings ?? new Dictionary<string, string?>())
        {
            builder.UseSetting(key, value);
        }

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(clock);

            // The test scheme replaces JwtBearer as the default, also in the default policy,
            // which the API ties to the JwtBearer scheme.
            services.AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = TestAuthHandler.SchemeName;
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            });
            services.PostConfigure<AuthorizationOptions>(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(TestAuthHandler.SchemeName)
                    .RequireAuthenticatedUser()
                    .Build();
            });
        });
    }
}
