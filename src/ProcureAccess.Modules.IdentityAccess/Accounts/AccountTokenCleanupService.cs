// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProcureAccess.Modules.IdentityAccess.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Accounts;

/// <summary>
/// Deletes expired account tokens, used or not, once at startup and then every
/// <see cref="AccountTokenSettings.CleanupInterval"/> (S6-05, TD-59; task decision D1 (a), like the
/// invitations' owner decision S5-17 D7 (a)): a row stays no longer than its link is valid. A used
/// row stays until then, so a second click on a confirmation link still answers as confirmed (SPEC
/// §8.8, decision S5-12 4 (b)); after the expiry every link answers as invalid. The timer runs on
/// <see cref="TimeProvider"/>, so tests drive it with <c>FakeTimeProvider</c>.
/// </summary>
/// <remarks>
/// Single API instance (ARCHITECTURE §10), like the log retention and the invitations' clean-up. A
/// failed run is logged and retried at the next interval; it never stops the host. The command
/// hosts never start their host, so this runs only in the web host. The table has no index on
/// <c>ExpiresAt</c>: the clean-up keeps it to the tokens of one lifetime, so the hourly scan is
/// cheap (task decision, S6-05).
/// </remarks>
internal sealed class AccountTokenCleanupService(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    AccountTokenSettings settings,
    ILogger<AccountTokenCleanupService> logger) : BackgroundService
{
    /// <summary>Deletes every account token that expired at <paramref name="now"/>, used or not.</summary>
    internal static Task<int> DeleteExpiredAsync(IdentityAccessDbContext db, DateTimeOffset now, CancellationToken cancellationToken) =>
        db.AccountTokens
          .Where(t => t.ExpiresAt <= now)
          .ExecuteDeleteAsync(cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(settings.CleanupInterval, timeProvider);

        do
        {
            await RunOnceAsync(stoppingToken);
        }
        while (await WaitForNextRunAsync(timer, stoppingToken));
    }

    private static async Task<bool> WaitForNextRunAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
            var deleted = await DeleteExpiredAsync(db, timeProvider.GetUtcNow(), stoppingToken);
            if (deleted > 0)
            {
                logger.LogInformation("Account token clean-up removed {DeletedCount} expired tokens", deleted);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is stopping.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Account token clean-up failed; next attempt in {CleanupInterval}", settings.CleanupInterval);
        }
    }
}
