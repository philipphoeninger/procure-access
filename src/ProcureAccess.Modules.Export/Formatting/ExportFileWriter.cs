// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using ProcureAccess.Modules.Export.Documents;

namespace ProcureAccess.Modules.Export.Formatting;

/// <summary>A rendered export file, held in memory only and never stored (EXP-05).</summary>
internal sealed record ExportFile(byte[] Content, string ContentType, string FileName);

/// <summary>
/// Turns export content into a file: adds the generation time from <see cref="TimeProvider"/>,
/// renders it with the formatter of the format and names it as SPEC §8.3 says,
/// <c>ProcureAccess_Kriterien_&lt;release&gt;_&lt;YYYY-MM-DD&gt;.&lt;extension&gt;</c>. The date is the UTC day,
/// as for the usage events. Saved lists add a slug of the list name in step 3.
/// </summary>
internal sealed class ExportFileWriter(IEnumerable<IExportFormatter> formatters, TimeProvider time)
{
    private readonly Dictionary<ExportFormat, IExportFormatter> _formatters = formatters.ToDictionary(f => f.Format);

    public ExportFile Write(ExportContent content, ExportFormat format)
    {
        ArgumentNullException.ThrowIfNull(content);
        var formatter = _formatters.TryGetValue(format, out var found)
            ? found
            : throw new InvalidOperationException($"No formatter is registered for {format}.");

        var document = new ExportDocument(content, time.GetUtcNow());
        var day = document.GeneratedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return new ExportFile(
            formatter.Render(document),
            formatter.ContentType,
            $"ProcureAccess_Kriterien_{content.ReleaseVersion}_{day}.{formatter.FileExtension}");
    }
}
