// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;

namespace ProcureAccess.Modules.Configurator;

/// <summary>
/// Registration of the Configurator module (ARCHITECTURE §4.3): saved criteria configurations
/// (organisation lists and project copies), user overrides and copies to a newer release.
/// It registers no services yet. Its code arrives with migration step 3 (saved lists).
/// </summary>
public static class ConfiguratorModule
{
    /// <summary>Registers the services of the Configurator module. Called once from <c>Program.cs</c>.</summary>
    public static IServiceCollection AddConfiguratorModule(this IServiceCollection services) => services;
}
