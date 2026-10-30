// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProcureAccess.Modules.Platform.Persistence;

namespace ProcureAccess.Modules.Platform.Logging;

/// <summary>
/// Deletes log rows older than the configured retention period from the <c>logging</c> schema
/// (ARCHITECTURE §9.5, invariant OPS-02). Unbounded growth of the log table is the most likely
/// operational failure of an under-maintained deployment (ARCHITECTURE §11.3, item 1).
/// </summary>
internal sealed class LogRetentionJob(
    PlatformDbContext db,
    TimeProvider timeProvider,
    IOptions<AppLoggingSettings.RetentionSettings> options,
    ILogger<LogRetentionJob> logger)
{
    /// <summary>Deletes every row with a time stamp before now minus the retention period.</summary>
    /// <returns>The number of deleted rows.</returns>
    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var cutoff = timeProvider.GetUtcNow().UtcDateTime.AddDays(-options.Value.RetentionDays);

        var deleted = await db.SeriLogEntries
            .Where(entry => entry.TimeStamp < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        logger.LogInformation(
            "Log retention removed {DeletedCount} log rows older than {Cutoff:O}",
            deleted,
            cutoff);

        return deleted;
    }
}
