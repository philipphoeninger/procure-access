// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Export.Documents;

namespace ProcureAccess.Modules.Export.Formatting;

/// <summary>The file formats of SPEC §8.3, named in the request as <c>xlsx</c> and <c>csv</c>.</summary>
internal enum ExportFormat
{
    Xlsx = 1,
    Csv = 2,
}

/// <summary>Request names of <see cref="ExportFormat"/>.</summary>
internal static class ExportFormats
{
    public static ExportFormat? Parse(string? name) => name switch
    {
        "xlsx" => ExportFormat.Xlsx,
        "csv" => ExportFormat.Csv,
        _ => null,
    };
}

/// <summary>
/// Renders an export document into one file format (ARCHITECTURE §4.4 „One formatter, two
/// sources“). One implementation per <see cref="ExportFormat"/>.
/// </summary>
internal interface IExportFormatter
{
    ExportFormat Format { get; }

    /// <summary>The <c>Content-Type</c> of the file.</summary>
    string ContentType { get; }

    /// <summary>The file name extension without dot.</summary>
    string FileExtension { get; }

    byte[] Render(ExportDocument document);
}
