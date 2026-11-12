// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Analytics;
using ProcureAccess.Modules.Platform.Persistence;
using Xunit;

namespace ProcureAccess.Tests.Integration.Infrastructure;

/// <summary>
/// The rows of <c>analytics."UsageEvents"</c> for tests of the usage events (R-MET-01). The writer
/// only queues and a background service stores the events (#115), so a test waits for the rows.
/// </summary>
internal static class UsageEventRows
{
    /// <summary>Deletes every stored event, so a test counts only its own.</summary>
    public static async Task DeleteAllAsync(ApiFixture api)
    {
        using var scope = api.Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PlatformDbContext>().UsageEvents
            .ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Waits (in real time, at most ten seconds) until the background service has stored
    /// <paramref name="count"/> events, then returns all stored events.
    /// </summary>
    public static async Task<List<UsageEvent>> WaitForAsync(ApiFixture api, int count)
    {
        using var patience = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        patience.CancelAfter(TimeSpan.FromSeconds(10));
        while (true)
        {
            using var scope = api.Factory.Services.CreateScope();
            var stored = await scope.ServiceProvider.GetRequiredService<PlatformDbContext>().UsageEvents
                .AsNoTracking()
                .ToListAsync(patience.Token);
            if (stored.Count >= count)
            {
                return stored;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50), patience.Token);
        }
    }
}
