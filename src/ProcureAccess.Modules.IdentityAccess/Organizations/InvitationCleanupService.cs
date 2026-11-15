// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProcureAccess.Modules.IdentityAccess.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Organizations;

/// <summary>
/// Deletes expired invitations, open or accepted, once at startup and then every
/// <see cref="InvitationSettings.CleanupInterval"/> (owner decision S5-17 D7 (a)): a row holds the
/// address of someone who may never have become a member, so it stays no longer than its link is
/// valid. An accepted row stays until then, so a second click on the link leads the new member to
/// the organisation. The timer runs on <see cref="TimeProvider"/>, so tests drive it with
/// <c>FakeTimeProvider</c>.
/// </summary>
/// <remarks>
/// Single API instance (ARCHITECTURE §10), like the log retention. A failed run is logged and
/// retried at the next interval; it never stops the host. The command hosts never start their host,
/// so this runs only in the web host.
/// </remarks>
internal sealed class InvitationCleanupService(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    InvitationSettings settings,
    ILogger<InvitationCleanupService> logger) : BackgroundService
{
    /// <summary>
    /// Deletes every invitation that expired at <paramref name="now"/>. Past the organisation filter:
    /// the job runs outside a request, where the filter shows nothing (TEN-02 allow-list).
    /// </summary>
    internal static Task<int> DeleteExpiredAsync(IdentityAccessDbContext db, DateTimeOffset now, CancellationToken cancellationToken) =>
        db.OrganizationInvitations.IgnoreQueryFilters()
          .Where(i => i.ExpiresAt <= now)
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
                logger.LogInformation("Invitation clean-up removed {DeletedCount} expired invitations", deleted);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is stopping.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invitation clean-up failed; next attempt in {CleanupInterval}", settings.CleanupInterval);
        }
    }
}
