// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.ProcureAccess.Extensions;

/// <summary>
/// The movable clock of the E2E stack (TESTING §3.3, J5; S5-30, owner decisions D4 (a), Q2 (a)).
/// It exists only in <c>Development</c> and <c>E2E</c>, whatever the configuration says, and only
/// when <see cref="EnabledKey"/> is true, which only <c>docker-compose.e2e.yml</c> sets — the same
/// guard as the E2E users (owner decision S5-14 1 (a)). Staging and Production never register it,
/// so there the cookie <see cref="CookieName"/> is an unknown cookie and changes nothing.
/// </summary>
public static class TestClockExtensions
{
    /// <summary>The switch (compose <c>TestClock__Enabled</c>).</summary>
    public const string EnabledKey = "TestClock:Enabled";

    /// <summary>
    /// Cookie with the offset in whole seconds (0 to <see cref="MaxOffset"/>) that a request asks
    /// for; the session journey sets it on its own browser context (<c>e2e/support/clock.ts</c>).
    /// </summary>
    public const string CookieName = "pa-test-clock";

    /// <summary>The largest offset a request may ask for: longer than the absolute session limit.</summary>
    public static readonly TimeSpan MaxOffset = TimeSpan.FromDays(2);

    /// <summary>True only in <c>Development</c> and <c>E2E</c>, like the E2E seeders.</summary>
    public static bool IsTestClockAllowed(IHostEnvironment environment) =>
        TestCatalogSeedExtensions.IsTestCatalogSeedAllowed(environment);

    /// <summary>
    /// Replaces the application's <see cref="TimeProvider"/> with a <see cref="TestClock"/> over the
    /// real clock where allowed and switched on; does nothing anywhere else.
    /// </summary>
    public static IServiceCollection AddTestClockIfAllowed(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (!IsTestClockAllowed(environment) || !configuration.GetValue<bool>(EnabledKey))
        {
            return services;
        }

        services.AddHttpContextAccessor();
        services.AddSingleton(provider => new TestClock(TimeProvider.System, provider.GetRequiredService<IHttpContextAccessor>()));
        services.Replace(ServiceDescriptor.Singleton<TimeProvider>(provider => provider.GetRequiredService<TestClock>()));
        return services;
    }

    /// <summary>
    /// First in the pipeline, before authentication reads the session: takes the offset of the
    /// request's cookie into the clock for the rest of the request. Without a registered
    /// <see cref="TestClock"/> it adds nothing.
    /// </summary>
    public static IApplicationBuilder UseTestClockIfRegistered(this IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetService<TestClock>() is null)
        {
            return app;
        }

        return app.Use((context, next) =>
        {
            TestClock.SetOffset(context, OffsetOf(context.Request));
            return next(context);
        });
    }

    private static TimeSpan OffsetOf(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue(CookieName, out var value)
            || !long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds)
            || seconds > MaxOffset.TotalSeconds)
        {
            return TimeSpan.Zero;
        }

        return TimeSpan.FromSeconds(seconds);
    }
}
