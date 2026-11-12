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
/// texts of the files came with S4-20a; the XLSX renderer is a placeholder until S4-20b. The source
/// of saved configurations follows in step 3.
/// </summary>
public static class ExportModule
{
    /// <summary>
    /// Registers the services of the Export module. Called once from <c>Program.cs</c>, after the
    /// Platform module (<see cref="TimeProvider"/>) and the Catalog module, whose
    /// <c>ICatalogSelectionResolver</c> the ad-hoc source uses.
    /// </summary>
    public static IServiceCollection AddExportModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddSingleton(PriorityWeightRange.From(configuration));
        services.AddScoped<AdHocSelectionSource>();
        services.AddScoped<ExportFileWriter>();

        // Read once here, so a key missing in Texts/de.json stops the start, not an export.
        services.AddSingleton(ExportTexts.LoadGerman());
        services.AddSingleton<IExportFormatter, CsvExportFormatter>();

        // Placeholder until the XLSX renderer of S4-20b.
        services.AddSingleton<IExportFormatter>(new StubExportFormatter(ExportFormat.Xlsx));
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
