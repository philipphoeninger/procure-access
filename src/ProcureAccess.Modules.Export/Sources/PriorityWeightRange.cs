// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;

namespace ProcureAccess.Modules.Export.Sources;

/// <summary>
/// 🔧 The weights a user may give a B criterion (R-LST-03, SPEC SD23: 1–10), from
/// <c>Configurator:PriorityWeight</c> of <c>appsettings.json</c> (owner decision O-S4-58 (a)). The
/// client has the same range as its setting <c>priorityWeight</c>; keep both in step. The server is
/// authoritative (SPEC §7.1).
/// </summary>
internal sealed record PriorityWeightRange(int Min, int Max)
{
    public const string SectionName = "Configurator:PriorityWeight";

    public bool Contains(int weight) => weight >= Min && weight <= Max;

    /// <summary>Reads the range; a missing or impossible range fails the start.</summary>
    public static PriorityWeightRange From(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var section = configuration.GetSection(SectionName);
        var min = section.GetValue<int?>(nameof(Min));
        var max = section.GetValue<int?>(nameof(Max));
        if (min is null || max is null || min < 1 || max < min)
        {
            throw new InvalidOperationException(
                $"{SectionName} needs {nameof(Min)} and {nameof(Max)} with 1 <= {nameof(Min)} <= {nameof(Max)} (SPEC SD23: 1 and 10).");
        }

        return new PriorityWeightRange(min.Value, max.Value);
    }
}
