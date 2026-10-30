// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Architecture.Modules;

/// <summary>
/// ARC-03: outside <c>Contracts</c>, a module project makes public only its registration class
/// <c>&lt;Name&gt;Module</c> (for Platform it also maps the endpoints) and its controllers
/// (ARCHITECTURE §4.3). Everything else is <c>internal</c>, generated EF migrations included.
/// </summary>
public class ModuleVisibilityTests
{
    [Theory(DisplayName = "INV:ARC-03 " + nameof(ModuleProject_PublicTypes_OnlyRegistrationAndControllers))]
    [Trait("Invariant", "ARC-03")]
    [MemberData(nameof(ModuleMap.RowNames), MemberType = typeof(ModuleMap))]
    public void ModuleProject_PublicTypes_OnlyRegistrationAndControllers(string module)
    {
        var row = ModuleMap.Row(module);
        var assembly = ModuleRules.Load(row.Project);
        var registrationClass = ModuleRules.RegistrationClass(row);

        assembly.GetType(registrationClass).ShouldNotBeNull($"{row.Project} has its registration class {registrationClass}.");
        ModuleRules.UnexpectedPublicTypes(assembly.GetExportedTypes(), registrationClass).ShouldBeEmpty(
            $"Make these types of {row.Project} internal (EF generates migrations public: change them by hand, CLAUDE.md §5).");
    }
}
