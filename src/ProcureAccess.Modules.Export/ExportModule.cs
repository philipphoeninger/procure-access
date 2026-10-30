// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;

namespace ProcureAccess.Modules.Export;

/// <summary>
/// Registration of the Export module (ARCHITECTURE §4.3): the format renderers (XLSX, CSV) and the
/// export sources.
/// It registers no services yet. Its code arrives with migration step 2 (export pipeline).
/// </summary>
public static class ExportModule
{
    /// <summary>Registers the services of the Export module. Called once from <c>Program.cs</c>.</summary>
    public static IServiceCollection AddExportModule(this IServiceCollection services) => services;
}
