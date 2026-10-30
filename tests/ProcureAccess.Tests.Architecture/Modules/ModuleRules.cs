// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using NetArchTest.Rules;

namespace ProcureAccess.Tests.Architecture.Modules;

/// <summary>
/// The type-level checks of ARC-01 – ARC-03 on the built assemblies: NetArchTest for dependencies,
/// reflection for visibility. They complement the project-reference checks (<see cref="ProjectGraph"/>);
/// <see cref="RuleSelfTests"/> shows that each of them can fail.
/// </summary>
internal static class ModuleRules
{
    /// <summary>Loads the assembly of a project; the project name is the assembly name.</summary>
    public static Assembly Load(string project) => Assembly.Load(new AssemblyName(project));

    /// <summary>
    /// The types in <paramref name="subject"/> (selected by namespace) that use a type of one of the
    /// <paramref name="forbidden"/> assemblies; empty when the rule holds.
    /// </summary>
    /// <remarks>
    /// The rule names the forbidden types one by one. A namespace would not do: NetArchTest matches
    /// dependencies by name prefix, so <c>ProcureAccess.Modules.Directory</c> would also forbid the
    /// allowed <c>ProcureAccess.Modules.Directory.Contracts</c> (S2-05 L2).
    /// </remarks>
    public static IReadOnlyList<string> TypesUsingAny(Assembly subject, string subjectNamespace, IEnumerable<Assembly> forbidden)
    {
        var forbiddenTypes = forbidden.SelectMany(OwnTopLevelTypeNames).ToArray();
        if (forbiddenTypes.Length == 0)
        {
            return [];
        }

        var result = Types.InAssembly(subject)
            .That().ResideInNamespaceStartingWith(subjectNamespace)
            .ShouldNot().HaveDependencyOnAny(forbiddenTypes)
            .GetResult();

        return result.IsSuccessful ? [] : [.. result.FailingTypeNames];
    }

    /// <summary>
    /// The public types among <paramref name="exportedTypes"/> that are neither the module
    /// registration class nor a controller (ARCHITECTURE §4.3); empty when the rule holds. A
    /// generated EF migration left <c>public</c> shows up here (S2-07 R3).
    /// </summary>
    public static IReadOnlyList<string> UnexpectedPublicTypes(IEnumerable<Type> exportedTypes, string registrationClass) =>
    [
        .. exportedTypes
            .Where(type => !IsRegistrationClass(type, registrationClass) && !typeof(ControllerBase).IsAssignableFrom(type))
            .Select(type => type.FullName!),
    ];

    /// <summary>The registration class of a module project, e.g. <c>ProcureAccess.Modules.Catalog.CatalogModule</c>.</summary>
    public static string RegistrationClass(ModuleRow row) => $"{row.Project}.{row.Name}Module";

    private static bool IsRegistrationClass(Type type, string registrationClass) =>
        type.FullName == registrationClass && type is { IsAbstract: true, IsSealed: true };

    /// <summary>
    /// Top-level types of an assembly in its own namespace. This leaves out the attributes the
    /// compiler embeds into every assembly (e.g. <c>System.Runtime.CompilerServices.NullableAttribute</c>),
    /// which have the same names everywhere, and coverlet's tracker types.
    /// </summary>
    private static IEnumerable<string> OwnTopLevelTypeNames(Assembly assembly)
    {
        var root = assembly.GetName().Name!;
        return assembly.GetTypes()
            .Where(type => !type.IsNested && type.Namespace is { } ns && (ns == root || ns.StartsWith(root + ".", StringComparison.Ordinal)))
            .Select(type => type.FullName!);
    }
}
