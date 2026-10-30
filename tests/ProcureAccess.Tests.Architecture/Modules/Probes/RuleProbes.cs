// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Mvc;
using ProcureAccess.Modules.Platform.Analytics;
using ProcureAccess.Modules.Platform.Contracts.Analytics;

// Probe types for RuleSelfTests: deliberate violations the module rules must report. They live in
// the test assembly only and are never registered anywhere.
namespace ProcureAccess.Tests.Architecture.Modules.Probes.Dependencies
{
    /// <summary>Uses an internal type of the Platform module: a violation for any other module.</summary>
    internal sealed class UsesPlatformInternals
    {
        public UsageEvent? Event { get; init; }
    }

    /// <summary>Uses only Platform.Contracts: allowed, and must not be reported (S2-05 L2).</summary>
    internal sealed class UsesPlatformContracts
    {
        public IUsageEventWriter? Writer { get; init; }
    }
}

namespace ProcureAccess.Tests.Architecture.Modules.Probes.Visibility
{
    /// <summary>Plays the registration class: allowed.</summary>
    public static class ProbeModule;

    /// <summary>A controller: allowed.</summary>
    [ApiController]
    public sealed class ProbeController : ControllerBase;

    /// <summary>Any other public type: a violation, also when named as the registration class (not static).</summary>
    public sealed class StrayPublicType;

    internal static class InternalOuter
    {
        /// <summary>Declared public, but not visible outside the assembly: allowed.</summary>
        public sealed class NestedPublic;
    }
}
