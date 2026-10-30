// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Reflection;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Architecture.Modules;

/// <summary>
/// ARC-03, one case per row of the module table (ARCHITECTURE §4.3, <see cref="ModuleMap"/>): a
/// module project references only its own contracts, the listed contracts and
/// <c>Platform.Contracts</c>, also through other projects; contracts projects reference nothing
/// (O-22 open); only the host and the tests reference a module project (C94). Checked on the
/// project files and, with NetArchTest, on the types.
/// </summary>
public class ModuleReferenceTests
{
    private static readonly ProjectGraph Graph = ProjectGraph.LoadSolution();

    [Fact(DisplayName = "INV:ARC-03 " + nameof(SolutionProjectsUnderSrc_Names_AreExactlyTheModuleTable))]
    [Trait("Invariant", "ARC-03")]
    public void SolutionProjectsUnderSrc_Names_AreExactlyTheModuleTable()
    {
        var projectsUnderSrc = Graph.Folders.Where(entry => entry.Value.StartsWith("src/", StringComparison.Ordinal))
            .Select(entry => entry.Key);

        projectsUnderSrc.ShouldBe(ModuleMap.AllModuleProjects, ignoreOrder: true,
            "Every project under src/ is a module or contracts project of the table in ARCHITECTURE §4.3 (ModuleMap).");
    }

    [Theory(DisplayName = "INV:ARC-03 " + nameof(ModuleProject_ProjectReferences_OnlyListedContracts))]
    [Trait("Invariant", "ARC-03")]
    [MemberData(nameof(ModuleMap.RowNames), MemberType = typeof(ModuleMap))]
    public void ModuleProject_ProjectReferences_OnlyListedContracts(string module)
    {
        var row = ModuleMap.Row(module);

        Graph.AllReferences(row.Project).Except(row.AllowedReferences).ShouldBeEmpty(
            $"{row.Project} may reference only {string.Join(", ", row.AllowedReferences.Order())} (ARCHITECTURE §4.3), also not through another project.");
    }

    [Theory(DisplayName = "INV:ARC-03 " + nameof(ContractsProject_ProjectReferences_None))]
    [Trait("Invariant", "ARC-03")]
    [MemberData(nameof(ModuleMap.RowNames), MemberType = typeof(ModuleMap))]
    public void ContractsProject_ProjectReferences_None(string module)
    {
        var row = ModuleMap.Row(module);

        Graph.DirectReferences(row.ContractsProject).ShouldBeEmpty(
            $"{row.ContractsProject} references no project until the owner decides O-22 (ARCHITECTURE §16.5).");
    }

    [Theory(DisplayName = "INV:ARC-03 " + nameof(ModuleProject_ReferencedBy_OnlyHostAndTests))]
    [Trait("Invariant", "ARC-03")]
    [MemberData(nameof(ModuleMap.RowNames), MemberType = typeof(ModuleMap))]
    public void ModuleProject_ReferencedBy_OnlyHostAndTests(string module)
    {
        var row = ModuleMap.Row(module);

        Graph.ReferencedBy(row.Project)
            .Where(project => project != ModuleMap.Host && !project.StartsWith(ModuleMap.TestProjectPrefix, StringComparison.Ordinal))
            .ShouldBeEmpty($"Only {ModuleMap.Host} and the test projects reference {row.Project} (ARCHITECTURE C94).");
    }

    [Theory(DisplayName = "INV:ARC-03 " + nameof(ModuleTypes_Dependencies_OnlyListedContracts))]
    [Trait("Invariant", "ARC-03")]
    [MemberData(nameof(ModuleMap.RowNames), MemberType = typeof(ModuleMap))]
    public void ModuleTypes_Dependencies_OnlyListedContracts(string module)
    {
        var row = ModuleMap.Row(module);
        var forbidden = ModuleMap.AllModuleProjects
            .Where(project => project != row.Project && !row.AllowedReferences.Contains(project))
            .Concat(ModuleMap.LegacyProjects);

        ModuleRules.TypesUsingAny(ModuleRules.Load(row.Project), row.Project, Assemblies(forbidden)).ShouldBeEmpty(
            $"Types of {row.Project} use only their own module, the listed contracts and Platform.Contracts.");
    }

    [Theory(DisplayName = "INV:ARC-03 " + nameof(ContractsTypes_Dependencies_NoOtherProject))]
    [Trait("Invariant", "ARC-03")]
    [MemberData(nameof(ModuleMap.RowNames), MemberType = typeof(ModuleMap))]
    public void ContractsTypes_Dependencies_NoOtherProject(string module)
    {
        var row = ModuleMap.Row(module);
        var forbidden = ModuleMap.AllModuleProjects
            .Where(project => project != row.ContractsProject)
            .Concat(ModuleMap.LegacyProjects);

        ModuleRules.TypesUsingAny(ModuleRules.Load(row.ContractsProject), row.ContractsProject, Assemblies(forbidden)).ShouldBeEmpty(
            $"Types of {row.ContractsProject} use no other project of the solution (O-22).");
    }

    private static IEnumerable<Assembly> Assemblies(IEnumerable<string> projects) => projects.Select(ModuleRules.Load);
}
