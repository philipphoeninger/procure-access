// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.Platform.Contracts.Concurrency;

namespace ProcureAccess.Modules.Platform.Concurrency;

/// <summary>
/// 🔧 <c>Concurrency:ETagEpoch</c> (ARCHITECTURE §7.1, §9.2; API-04; S6-20): the number every
/// <c>ETag</c> of a shared resource carries. Raise it after a database restore; every tag issued
/// before is stale then. Read once at the start; a missing value or one below 1 stops it.
/// </summary>
internal static class ConcurrencySettings
{
    public const string SectionName = "Concurrency";

    public const string ETagEpoch = "ETagEpoch";

    public static EntityTags EntityTagsFrom(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var epoch = configuration.GetSection(SectionName).GetValue<int?>(ETagEpoch);
        if (epoch is null or < 1)
        {
            throw new InvalidOperationException($"{SectionName}:{ETagEpoch} is required and at least 1 (default 1).");
        }

        return new EntityTags(epoch.Value);
    }
}
