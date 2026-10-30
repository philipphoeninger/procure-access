// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Xunit;

namespace ProcureAccess.Tests.Architecture.Modules;

/// <summary>One row of the module table in ARCHITECTURE §4.3.</summary>
/// <param name="Name">Module name, as in the project name <c>ProcureAccess.Modules.&lt;Name&gt;</c>.</param>
/// <param name="AllowedContracts">The modules whose <c>Contracts</c> project this module may reference.</param>
internal sealed record ModuleRow(string Name, IReadOnlyList<string> AllowedContracts)
{
    public string Project => ModuleMap.Prefix + Name;

    public string ContractsProject => Project + ModuleMap.ContractsSuffix;

    /// <summary>
    /// Every project the module project may reference, directly or through another project: its own
    /// contracts, the listed contracts and <c>Platform.Contracts</c>, which every module may use.
    /// </summary>
    public IReadOnlySet<string> AllowedReferences =>
        new HashSet<string>(
            AllowedContracts.Select(module => ModuleMap.Prefix + module + ModuleMap.ContractsSuffix)
                .Append(ContractsProject)
                .Append(ModuleMap.PlatformContracts));

    public override string ToString() => Name;
}

/// <summary>
/// The module table of ARCHITECTURE §4.3 in one place; ARC-01 – ARC-03 read it. Change it only
/// together with the table.
/// </summary>
internal static class ModuleMap
{
    public const string Prefix = "ProcureAccess.Modules.";

    public const string ContractsSuffix = ".Contracts";

    public const string PlatformContracts = Prefix + "Platform" + ContractsSuffix;

    /// <summary>The only production project that may reference a module project (C94).</summary>
    public const string Host = "API.ProcureAccess";

    /// <summary>Test projects may reference module projects too.</summary>
    public const string TestProjectPrefix = "ProcureAccess.Tests.";

    /// <summary>The legacy projects; no module project references them (O-S2-1).</summary>
    public static readonly IReadOnlyList<string> LegacyProjects =
        [Host, "DAL.ProcureAccess", "MODELS.ProcureAccess", "SERVICES.ProcureAccess"];

    public static readonly IReadOnlyList<ModuleRow> Rows =
    [
        new("Catalog", []),
        new("Configurator", ["Catalog", "IdentityAccess"]),
        new("Workspace", ["Catalog", "Configurator", "Directory", "IdentityAccess"]),
        new("Directory", ["Catalog", "Configurator", "IdentityAccess"]),
        new("Moderation", ["Directory", "IdentityAccess"]),
        new("Export", ["Catalog", "Configurator"]),
        new("IdentityAccess", []),
        new("Platform", []),
    ];

    public static ModuleRow Row(string name) => Rows.Single(row => row.Name == name);

    /// <summary>All module and contracts projects of the table.</summary>
    public static IEnumerable<string> AllModuleProjects =>
        Rows.SelectMany(row => new[] { row.Project, row.ContractsProject });

    /// <summary>xUnit data: one case per row.</summary>
    public static TheoryData<string> RowNames => [.. Rows.Select(row => row.Name)];
}
