// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text;
using ProcureAccess.Modules.Export.Documents;

namespace ProcureAccess.Modules.Export.Formatting;

/// <summary>
/// Placeholder of the XLSX and CSV renderers of S4-20 (SPEC §8.3), so the pipeline runs end to end
/// before them: plain UTF-8 text with the release and one line per row (key, level that applies).
/// It already answers with the content type and file name extension of its format; only the body
/// is not yet that format. No screen calls the export before S4-21.
/// </summary>
internal sealed class StubExportFormatter(ExportFormat format) : IExportFormatter
{
    public ExportFormat Format => format;

    public string ContentType => format switch
    {
        ExportFormat.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ExportFormat.Csv => "text/csv; charset=utf-8",
        _ => throw new InvalidOperationException($"Unknown export format {format}."),
    };

    public string FileExtension => format switch
    {
        ExportFormat.Xlsx => "xlsx",
        ExportFormat.Csv => "csv",
        _ => throw new InvalidOperationException($"Unknown export format {format}."),
    };

    public byte[] Render(ExportDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var text = new StringBuilder()
            .Append("Katalogversion: ").AppendLine(document.Content.ReleaseVersion);
        foreach (var row in document.Content.Rows)
        {
            text.Append(row.Key).Append(';').AppendLine(row.EffectiveLevel.ToString());
        }

        return Encoding.UTF8.GetBytes(text.ToString());
    }
}
