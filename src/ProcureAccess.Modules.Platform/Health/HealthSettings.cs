// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.Platform.Contracts.Configuration;

namespace ProcureAccess.Modules.Platform.Health;

/// <summary>
/// 🔧 settings of <c>/health/ready</c> (#112, SPEC §7.3, owner decision O-S4-70 (a)): how long a
/// ready check may take before it counts as unhealthy, and how many ready checks run at a time.
/// </summary>
internal sealed record HealthSettings(TimeSpan ReadyTimeout, int ReadyConcurrentChecks)
{
    public const string SectionName = "Health";

    /// <summary>Reads the settings; a missing or impossible value fails the start.</summary>
    public static HealthSettings From(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var section = configuration.GetSection(SectionName);
        var timeout = DurationSetting.Read(section[nameof(ReadyTimeout)], $"{SectionName}:{nameof(ReadyTimeout)}");
        var concurrentChecks = section.GetValue<int?>(nameof(ReadyConcurrentChecks));
        if (concurrentChecks is null or < 1)
        {
            throw new InvalidOperationException(
                $"{SectionName} needs {nameof(ReadyConcurrentChecks)} >= 1 (SPEC §7.3: 2).");
        }

        return new HealthSettings(timeout, concurrentChecks.Value);
    }
}
