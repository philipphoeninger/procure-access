// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;

namespace ProcureAccess.Modules.Directory;

/// <summary>
/// Registration of the Directory module (ARCHITECTURE §4.3): software product identities, published
/// entries and versions, ACRs, experience reports and consent records.
/// It registers no services yet. Its code arrives with migration step 6.
/// </summary>
public static class DirectoryModule
{
    /// <summary>Registers the services of the Directory module. Called once from <c>Program.cs</c>.</summary>
    public static IServiceCollection AddDirectoryModule(this IServiceCollection services) => services;
}
