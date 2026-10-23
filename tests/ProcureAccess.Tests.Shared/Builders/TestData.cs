// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Tests.Shared.Builders;

/// <summary>
/// Entry point for all builders. Import it with
/// <c>using static ProcureAccess.Tests.Shared.Builders.TestData;</c> and write
/// <c>ACriterion().WithName("…").Build()</c>.
/// </summary>
/// <remarks>
/// Method names start with an article (<c>ACatalogRelease()</c>, <c>AnOrganization()</c>),
/// as in TESTING §6.1.
/// </remarks>
public static class TestData
{
    /// <summary>
    /// Fixed timestamp used as the default for every <c>CreatedAt</c>. Deterministic values keep
    /// failures readable; tests never read the real clock.
    /// </summary>
    public static readonly DateTimeOffset DefaultCreatedAt = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>A criterion of the existing data model (before catalog versioning, migration step 2).</summary>
    public static CriterionBuilder ACriterion() => new();
}
