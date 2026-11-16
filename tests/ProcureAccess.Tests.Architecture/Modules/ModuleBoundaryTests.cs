// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Export.Sources;
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
    private const string Export = ModuleMap.Prefix + "Export";
    private const string Configurator = ModuleMap.Prefix + "Configurator";
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

    /// <summary>
    /// ARC-03, the part deferred to step 2 (TESTING V74): the ad-hoc export source reads the catalog
    /// only through <c>Catalog.Contracts</c> (ARCHITECTURE §4.4, C62) — no saved configuration, no
    /// other module, no legacy project. Its own module and Platform's contracts are allowed.
    /// </summary>
    [Fact(DisplayName = "INV:ARC-03 " + nameof(AdHocSelectionSource_Types_UseOfOtherModulesOnlyCatalogContracts))]
    [Trait("Invariant", "ARC-03")]
    public void AdHocSelectionSource_Types_UseOfOtherModulesOnlyCatalogContracts()
    {
        string[] allowed = [Export, Export + Contracts, Catalog + Contracts, ModuleMap.PlatformContracts];
        var forbidden = ModuleMap.AllModuleProjects
            .Except(allowed)
            .Concat(ModuleMap.LegacyProjects)
            .Select(ModuleRules.Load);

        ModuleRules.TypeUsingAny(typeof(AdHocSelectionSource), forbidden).ShouldBeEmpty();
    }

    /// <summary>
    /// ARC-03, the <c>IConfigurationReader</c> part (ARCHITECTURE §4.3, C62; S5-19): Export reads a
    /// saved list only through the reader of <c>Configurator.Contracts</c>. The Export project
    /// cannot see the Configurator module at all, the source of saved lists takes the reader, and
    /// no other type of Export uses a type of <c>Configurator.Contracts</c>.
    /// </summary>
    [Fact(DisplayName = "INV:ARC-03 " + nameof(Export_SavedLists_AreReadOnlyThroughIConfigurationReader))]
    [Trait("Invariant", "ARC-03")]
    public void Export_SavedLists_AreReadOnlyThroughIConfigurationReader()
    {
        Graph.AllReferences(Export).ShouldNotContain(Configurator);
        Graph.AllReferences(Export + Contracts).ShouldNotContain(Configurator);

        typeof(PersistedConfigurationSource).GetConstructors().Single().GetParameters()
            .ShouldContain(parameter => parameter.ParameterType == typeof(IConfigurationReader));

        string[] allowed = [Export, Export + Contracts, Catalog + Contracts, Configurator + Contracts, ModuleMap.PlatformContracts];
        var forbidden = ModuleMap.AllModuleProjects
            .Except(allowed)
            .Concat(ModuleMap.LegacyProjects)
            .Select(ModuleRules.Load);
        ModuleRules.TypeUsingAny(typeof(PersistedConfigurationSource), forbidden).ShouldBeEmpty();

        ModuleRules.TypesUsingAny(ModuleRules.Load(Export), Export, [ModuleRules.Load(Configurator + Contracts)])
            .ShouldAllBe(type => type.StartsWith(typeof(PersistedConfigurationSource).FullName!, StringComparison.Ordinal));
    }
}
