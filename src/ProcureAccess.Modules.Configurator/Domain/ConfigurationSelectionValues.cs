// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Configurator.Domain;

// The three multi-value layers of a list's selection (ARCHITECTURE §5.3), one row per chosen value.
// Each names a filter value by its key in the release the list is pinned to (owner decision
// S5-19 2a): the Catalog contract and the API address values by key, and no foreign key may cross
// the two contexts (ADR-014). Rows of their list, stored and read only with it.

/// <summary>An app type of the selection; a list has at least one.</summary>
internal sealed class ConfigurationAppType
{
    private ConfigurationAppType()
    {
    }

    internal ConfigurationAppType(string appTypeKey) => AppTypeKey = appTypeKey;

    public string AppTypeKey { get; private set; } = null!;
}

/// <summary>A product part of the selection.</summary>
internal sealed class ConfigurationProductPart
{
    private ConfigurationProductPart()
    {
    }

    internal ConfigurationProductPart(string productPartKey) => ProductPartKey = productPartKey;

    public string ProductPartKey { get; private set; } = null!;
}

/// <summary>An optional test procedure of the selection; each adds its „Geprüft nach …“ criterion (C30).</summary>
internal sealed class ConfigurationTestType
{
    private ConfigurationTestType()
    {
    }

    internal ConfigurationTestType(string testTypeKey) => TestTypeKey = testTypeKey;

    public string TestTypeKey { get; private set; } = null!;
}
