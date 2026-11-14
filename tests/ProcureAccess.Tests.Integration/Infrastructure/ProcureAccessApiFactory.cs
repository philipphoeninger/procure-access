// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using API.ProcureAccess.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using ProcureAccess.Modules.IdentityAccess.DataProtection;
using ProcureAccess.Modules.Platform.Contracts.Email;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.DataProtection;

namespace ProcureAccess.Tests.Integration.Infrastructure;

/// <summary>
/// Hosts the real API in the <c>Testing</c> environment against the test container
/// (TESTING §3.1, §5.2). Created once per test run by <see cref="ApiFixture"/>.
/// </summary>
/// <remarks>
/// Tests of startup behaviour create their own instance with another
/// <paramref name="environmentName"/> or extra host <paramref name="settings"/>
/// (e.g. SEC-06) and dispose it themselves. <paramref name="configureServices"/> adds test-only
/// services to such an instance, e.g. a controller of the test assembly.
/// With <paramref name="realAuthentication"/> the host keeps the API's own authentication — the
/// Identity cookie — instead of the test scheme, for the tests of the real
/// sign-in flow (SEC-01, SEC-03, SEC-09, OPS-04; #88). Such a host runs on a database of its own,
/// because the test scheme is a host-wide switch and the shared host keeps it.
/// No host sends e-mail: <see cref="Emails"/> replaces Platform's SMTP sender (S5-11).
/// </remarks>
public sealed class ProcureAccessApiFactory(
    string connectionString,
    FakeTimeProvider clock,
    string environmentName = ProcureAccessApiFactory.EnvironmentName,
    IReadOnlyDictionary<string, string?>? settings = null,
    Action<IServiceCollection>? configureServices = null,
    bool realAuthentication = false)
    : WebApplicationFactory<Program>
{
    public const string EnvironmentName = "Testing";

    /// <summary>The e-mails the API sent through this host (S5-11).</summary>
    public CapturingEmailSender Emails { get; } = new();

    private const string RelaxedPermitLimit = "1000000";

    /// <summary>One key-ring certificate per test run, so hosts on the same database read each other's keys.</summary>
    private static readonly Lazy<string> TestRunCertificate = new(() => TestCertificates.NewRsaPfxBase64());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environmentName);

        // Program.cs reads these values while the builder is still being set up, so they must be
        // host settings (UseSetting); values added through ConfigureAppConfiguration arrive too late.
        builder.UseSetting("ConnectionStrings:ProcureAccess", connectionString);
        builder.UseSetting("RebuildDatabase", "false");

        // No test catalog release (S4-08): appsettings.Development.json names one, and a host in
        // Development would seed it. Tests of the seeder set the path themselves.
        builder.UseSetting(TestCatalogSeedExtensions.PathKey, "");

        // The key ring is encrypted outside Development and E2E (S4-26), so every host gets the
        // throwaway certificate of this test run; tests of the certificate settings override it.
        builder.UseSetting(KeyRingCertificates.CertificateSettingName, TestRunCertificate.Value);

        // Outside Development and E2E the API needs SMTP credentials to start (S5-11, owner decision
        // 4a); the sender is replaced below, so these never reach a server.
        builder.UseSetting("Email:Smtp:Username", "integration-test");
        builder.UseSetting("Email:Smtp:Password", "integration-test");

        // Every request of the test server comes from no address, so all tests share one counter
        // per rate-limit policy: limits far above what a test run sends. Tests of the limits pass
        // small ones in settings (S4-21a, API-05).
        builder.UseSetting("RateLimits:AnonymousExport:PermitLimit", RelaxedPermitLimit);
        builder.UseSetting("RateLimits:CriteriaListShown:PermitLimit", RelaxedPermitLimit);
        builder.UseSetting("RateLimits:Registration:PermitLimit", RelaxedPermitLimit);
        builder.UseSetting("RateLimits:ConfirmationResend:PermitLimit", RelaxedPermitLimit);

        foreach (var (key, value) in settings ?? new Dictionary<string, string?>())
        {
            builder.UseSetting(key, value);
        }

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(clock);

            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);

            if (!realAuthentication)
            {
                // The test scheme replaces the API's default scheme (the Identity cookie), also
                // in the default policy. The cookie schemes stay registered but unused.
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
            }

            configureServices?.Invoke(services);
        });
    }
}
