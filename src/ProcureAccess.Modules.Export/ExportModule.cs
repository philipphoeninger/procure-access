// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Export.Formatting;
using ProcureAccess.Modules.Export.Http;
using ProcureAccess.Modules.Export.Sources;
using ProcureAccess.Modules.Export.Texts;

namespace ProcureAccess.Modules.Export;

/// <summary>
/// Registration of the Export module (ARCHITECTURE §4.3): the format renderers (XLSX, CSV) and the
/// export sources. Since S4-19 it holds the export pipeline — <see cref="AdHocSelectionSource"/>
/// behind <c>POST /api/v1/exports</c> (<see cref="MapExportEndpoints"/>). The CSV renderer and the
/// texts of the files came with S4-20a, the XLSX renderer with S4-20b. Since S5-19 it holds
/// <see cref="PersistedConfigurationSource"/>, the source of saved lists; its endpoint
/// <c>POST /api/v1/configurations/{id}/exports</c> follows with S5-21.
/// </summary>
public static class ExportModule
{
    /// <summary>
    /// Registers the services of the Export module. Called once from <c>Program.cs</c>, after the
    /// Platform module (<see cref="TimeProvider"/>), the Catalog module, whose
    /// <c>ICatalogSelectionResolver</c> both sources use, and the Configurator module, whose
    /// <c>IConfigurationReader</c> the source of saved lists and whose <c>IConfigurationDraftCheck</c>
    /// the ad-hoc source uses (C153).
    /// </summary>
    public static IServiceCollection AddExportModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddScoped<AdHocSelectionSource>();
        services.AddScoped<PersistedConfigurationSource>();
        services.AddScoped<ExportFileWriter>();

        // Read once here, so a key missing in Texts/de.json stops the start, not an export.
        services.AddSingleton(ExportTexts.LoadGerman());
        services.AddSingleton(ExportToolUrl.From(configuration));
        services.AddSingleton<IExportFormatter, CsvExportFormatter>();
        services.AddSingleton<IExportFormatter, XlsxExportFormatter>();
        return services;
    }

    /// <summary>
    /// Maps the export endpoints as minimal APIs on <paramref name="routes"/>, the host's versioned
    /// group <c>/api/v{version}</c>, as <c>MapCatalogEndpoints</c> does. Map it before the Swagger
    /// set-up (#176).
    /// </summary>
    public static IEndpointRouteBuilder MapExportEndpoints(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);
        ExportEndpoint.Map(routes).WithTags("Export");
        return routes;
    }
}
