// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;

namespace ProcureAccess.Modules.Workspace;

/// <summary>
/// Registration of the Workspace module (ARCHITECTURE §4.3): the organisations' procurement
/// projects, candidate products, test evidences and private assessments.
/// It registers no services yet. Its code arrives with migration step 5; until then proposals,
/// products and favorites are legacy.
/// </summary>
public static class WorkspaceModule
{
    /// <summary>Registers the services of the Workspace module. Called once from <c>Program.cs</c>.</summary>
    public static IServiceCollection AddWorkspaceModule(this IServiceCollection services) => services;
}
