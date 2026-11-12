// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Text;
using ProcureAccess.Modules.Export.Documents;
using ProcureAccess.Modules.Export.Texts;

namespace ProcureAccess.Modules.Export.Formatting;

/// <summary>
/// The CSV file of SPEC §8.3 (EXP-03, EXP-04): UTF-8 with BOM, semicolon as separator (German
/// spreadsheet default), CRLF after every record, RFC 4180 quoting. A header row, then one record
/// per included criterion; the first column „Katalogversion“ holds the release in every record,
/// because the CSV has no „Info“ sheet. The columns follow <see cref="CriteriaColumns"/>.
/// </summary>
/// <remarks>
/// A text cell that a spreadsheet would read as a formula (it starts with <c>=</c>, <c>+</c>,
/// <c>-</c>, <c>@</c>, a tab or a carriage return) gets a leading apostrophe (OWASP „CSV
/// injection“; owner decision O-S4-67 (a)). The file goes on to other people in a procurement, and
/// notes are free text. Numbers are written by the server and never start that way.
/// </remarks>
internal sealed class CsvExportFormatter(ExportTexts texts) : IExportFormatter
{
    private const char Separator = ';';
    private const string RecordEnd = "\r\n";

    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    public ExportFormat Format => ExportFormat.Csv;

    public string ContentType => "text/csv; charset=utf-8";

    public string FileExtension => "csv";

    public byte[] Render(ExportDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var content = document.Content;
        var csv = new StringBuilder();

        AppendRecord(csv, [texts[ExportTextKeys.ColumnRelease], .. CriteriaColumns.All.Select(c => texts[c.HeaderKey])]);
        for (var i = 0; i < content.Rows.Count; i++)
        {
            var row = content.Rows[i];
            AppendRecord(csv, [content.ReleaseVersion, .. CriteriaColumns.All.Select(c => c.Value(row, i + 1, texts))]);
        }

        return [.. Utf8WithBom.GetPreamble(), .. Utf8WithBom.GetBytes(csv.ToString())];
    }

    private static void AppendRecord(StringBuilder csv, IReadOnlyList<object?> values)
    {
        for (var i = 0; i < values.Count; i++)
        {
            if (i > 0)
            {
                csv.Append(Separator);
            }

            csv.Append(Field(values[i]));
        }

        csv.Append(RecordEnd);
    }

    /// <summary>One field: a number as written, a text guarded against formulas and quoted when it must be.</summary>
    internal static string Field(object? value) => value switch
    {
        null => string.Empty,
        int number => number.ToString(CultureInfo.InvariantCulture),
        string text => Quoted(Guarded(text)),
        _ => throw new ArgumentException($"A cell cannot hold a {value.GetType().Name}.", nameof(value)),
    };

    private static string Guarded(string text) =>
        text.Length > 0 && text[0] is '=' or '+' or '-' or '@' or '\t' or '\r' ? "'" + text : text;

    // RFC 4180 §2: a field with the separator, a quote or a line break is quoted, and a quote inside
    // it is doubled.
    private static string Quoted(string text) =>
        text.AsSpan().IndexOfAny([Separator, '"', '\r', '\n']) < 0
            ? text
            : $"\"{text.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
