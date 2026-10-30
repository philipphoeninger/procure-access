// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Architecture.Modules;

/// <summary>
/// ARC-01 and ARC-02 (ARCHITECTURE §4.3, CLAUDE.md §7). They name their projects themselves
/// instead of reading <see cref="ModuleMap"/>, so a wrong edit of the table cannot weaken them.
/// </summary>
public class ModuleBoundaryTests
{
    private const string Catalog = ModuleMap.Prefix + "Catalog";
    private const string Workspace = ModuleMap.Prefix + "Workspace";
    private const string Directory = ModuleMap.Prefix + "Directory";
    private const string Contracts = ModuleMap.ContractsSuffix;

    private static readonly ProjectGraph Graph = ProjectGraph.LoadSolution();

    [Fact(DisplayName = "INV:ARC-01 " + nameof(CatalogModule_ProjectsAndTypes_DependOnNoOtherModule))]
    [Trait("Invariant", "ARC-01")]
    public void CatalogModule_ProjectsAndTypes_DependOnNoOtherModule()
    {
        // Platform.Contracts is not "another module": every module may use Platform (§4.3).
        Graph.AllReferences(Catalog).Except([Catalog + Contracts, ModuleMap.PlatformContracts]).ShouldBeEmpty();
        Graph.AllReferences(Catalog + Contracts).ShouldBeEmpty();

        var otherProjects = ModuleMap.AllModuleProjects
            .Where(project => project != Catalog && project != Catalog + Contracts)
            .Concat(ModuleMap.LegacyProjects)
            .ToList();
        var forbiddenForModule = otherProjects.Where(project => project != ModuleMap.PlatformContracts).Select(ModuleRules.Load);

        ModuleRules.TypesUsingAny(ModuleRules.Load(Catalog), Catalog, forbiddenForModule).ShouldBeEmpty();
        ModuleRules.TypesUsingAny(ModuleRules.Load(Catalog + Contracts), Catalog + Contracts, otherProjects.Select(ModuleRules.Load))
            .ShouldBeEmpty();
    }

    [Fact(DisplayName = "INV:ARC-02 " + nameof(WorkspaceAndDirectory_ProjectsAndTypes_DoNotReferenceEachOther))]
    [Trait("Invariant", "ARC-02")]
    public void WorkspaceAndDirectory_ProjectsAndTypes_DoNotReferenceEachOther()
    {
        // Workspace reaches Directory only through Directory.Contracts (IPublicationGateway, Flow B);
        // Directory never sees Workspace, not even its contracts (IEntrySourceAuthorizer is declared
        // in Directory.Contracts and implemented by Workspace).
        Graph.AllReferences(Workspace).ShouldNotContain(Directory);
        Graph.AllReferences(Workspace + Contracts).ShouldNotContain(Directory);
        Graph.AllReferences(Directory).ShouldNotContain(Workspace);
        Graph.AllReferences(Directory).ShouldNotContain(Workspace + Contracts);
        Graph.AllReferences(Directory + Contracts).ShouldNotContain(Workspace);
        Graph.AllReferences(Directory + Contracts).ShouldNotContain(Workspace + Contracts);

        var directoryInternals = new[] { ModuleRules.Load(Directory) };
        var workspaceAll = new[] { ModuleRules.Load(Workspace), ModuleRules.Load(Workspace + Contracts) };

        ModuleRules.TypesUsingAny(ModuleRules.Load(Workspace), Workspace, directoryInternals).ShouldBeEmpty();
        ModuleRules.TypesUsingAny(ModuleRules.Load(Workspace + Contracts), Workspace + Contracts, directoryInternals).ShouldBeEmpty();
        ModuleRules.TypesUsingAny(ModuleRules.Load(Directory), Directory, workspaceAll).ShouldBeEmpty();
        ModuleRules.TypesUsingAny(ModuleRules.Load(Directory + Contracts), Directory + Contracts, workspaceAll).ShouldBeEmpty();
    }
}
