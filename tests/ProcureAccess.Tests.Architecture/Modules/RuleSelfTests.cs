// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Tests.Architecture.Modules.Probes.Dependencies;
using ProcureAccess.Tests.Architecture.Modules.Probes.Visibility;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Architecture.Modules;

/// <summary>
/// Shows that the checks behind ARC-01 – ARC-03 can fail. Most modules are still empty, so their
/// rules pass without having anything to find; these tests run each check on a deliberate
/// violation (probe types in this assembly, a made-up project graph). If NetArchTest ever stops
/// reading the assemblies of a new SDK, these fail instead of the rules passing silently (S2-05 P3).
/// </summary>
public class RuleSelfTests
{
    private const string DependencyProbes = "ProcureAccess.Tests.Architecture.Modules.Probes.Dependencies";
    private const string VisibilityProbes = "ProcureAccess.Tests.Architecture.Modules.Probes.Visibility";
    private const string Platform = ModuleMap.Prefix + "Platform";

    [Fact]
    public void TypesUsingAny_TypeUsingForbiddenInternals_IsReportedAndContractsUserIsNot()
    {
        var failing = ModuleRules.TypesUsingAny(
            typeof(RuleSelfTests).Assembly, DependencyProbes, [ModuleRules.Load(Platform)]);

        failing.ShouldBe([typeof(UsesPlatformInternals).FullName!]);
    }

    [Fact]
    public void TypesUsingAny_ForbiddenContracts_ReportsTheirUser()
    {
        var failing = ModuleRules.TypesUsingAny(
            typeof(RuleSelfTests).Assembly, DependencyProbes, [ModuleRules.Load(ModuleMap.PlatformContracts)]);

        failing.ShouldBe([typeof(UsesPlatformContracts).FullName!]);
    }

    [Fact]
    public void UnexpectedPublicTypes_ProbeTypes_ReportsOnlyTheStrayType()
    {
        var exported = typeof(RuleSelfTests).Assembly.GetExportedTypes().Where(type => type.Namespace == VisibilityProbes);

        ModuleRules.UnexpectedPublicTypes(exported, typeof(ProbeModule).FullName!)
            .ShouldBe([typeof(StrayPublicType).FullName!]);
    }

    [Fact]
    public void UnexpectedPublicTypes_RegistrationNameOnNonStaticClass_IsReported()
    {
        ModuleRules.UnexpectedPublicTypes([typeof(StrayPublicType)], typeof(StrayPublicType).FullName!)
            .ShouldBe([typeof(StrayPublicType).FullName!]);
    }

    [Fact]
    public void ProjectGraph_WrongReferenceThroughContracts_ShowsInAllReferences()
    {
        var catalog = ModuleMap.Row("Catalog");
        var workspaceContracts = ModuleMap.Row("Workspace").ContractsProject;
        var graph = new ProjectGraph(new Dictionary<string, IReadOnlySet<string>>
        {
            [catalog.Project] = new HashSet<string> { catalog.ContractsProject },
            [catalog.ContractsProject] = new HashSet<string> { workspaceContracts },
            [workspaceContracts] = new HashSet<string>(),
            ["DAL.ProcureAccess"] = new HashSet<string> { catalog.Project },
        });

        graph.AllReferences(catalog.Project).Except(catalog.AllowedReferences).ShouldBe([workspaceContracts]);
        graph.ReferencedBy(catalog.Project).ShouldBe(["DAL.ProcureAccess"]);
    }
}
