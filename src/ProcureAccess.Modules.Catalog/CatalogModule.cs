// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;

namespace ProcureAccess.Modules.Catalog;

/// <summary>
/// Registration of the Catalog module (ARCHITECTURE §4.3): criteria, revisions, releases, app
/// types, product parts, use-case templates, test types, priority rules and the procedure mapping.
/// It registers no services yet. Its code arrives with migration step 2 (catalog versioning); until
/// then the criteria code is legacy in API, DAL, MODELS and SERVICES.
/// </summary>
public static class CatalogModule
{
    /// <summary>Registers the services of the Catalog module. Called once from <c>Program.cs</c>.</summary>
    public static IServiceCollection AddCatalogModule(this IServiceCollection services) => services;
}
