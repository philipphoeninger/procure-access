// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ClosedXML.Excel;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Export.Documents;
using ProcureAccess.Modules.Export.Texts;

namespace ProcureAccess.Modules.Export.Formatting;

/// <summary>
/// The XLSX file of SPEC §8.3 with ClosedXML (ARCHITECTURE D12; EXP-01 – EXP-03): the sheet
/// „Kriterien“ with the columns of <see cref="CriteriaColumns"/> as an Excel table named like the
/// sheet (owner decision O-S4-66 (a)) and the sheet „Info“ with tool, date, catalog release,
/// selection, counts per priority, the two notes and the source line.
/// </summary>
/// <remarks>
/// <para><b>Accessibility (EXP-02).</b> The table gives the data a real header row that screen
/// readers announce and Excel lists under its name; the header row is frozen. No fill colour, no
/// table style and no conditional format: nothing is said by colour. Column widths are fixed and long
/// texts wrap, so nothing is cut off; ClosedXML never measures fonts here, so the server needs none.</para>
/// <para><b>Cells.</b> Numbers are numbers and texts are texts — a text that starts with <c>=</c> stays
/// text in XLSX, so it needs no guard (unlike the CSV). A test-step link is also a hyperlink.</para>
/// <para><b>Time.</b> ClosedXML stamps the document with the machine clock; the formatter sets created
/// and modified to the generation time of the document (CLAUDE §7), and the author to the tool name.</para>
/// </remarks>
internal sealed class XlsxExportFormatter(ExportTexts texts, ExportToolUrl toolUrl) : IExportFormatter
{
    private const int HeaderRow = 1;
    private const string DateFormat = "dd.mm.yyyy";

    // Widths in characters by column; a text wraps within its width.
    private static readonly Dictionary<string, double> Widths = new(StringComparer.Ordinal)
    {
        [ExportTextKeys.ColumnIndex] = 6,
        [ExportTextKeys.ColumnKey] = 18,
        [ExportTextKeys.ColumnNumber] = 12,
        [ExportTextKeys.ColumnTitle] = 40,
        [ExportTextKeys.ColumnOriginalTitle] = 40,
        [ExportTextKeys.ColumnShortDescription] = 40,
        [ExportTextKeys.ColumnProductPart] = 22,
        [ExportTextKeys.ColumnRecommended] = 24,
        [ExportTextKeys.ColumnChosen] = 24,
        [ExportTextKeys.ColumnWeight] = 12,
        [ExportTextKeys.ColumnRationale] = 50,
        [ExportTextKeys.ColumnNote] = 40,
        [ExportTextKeys.ColumnTestStep] = 40,
    };

    public ExportFormat Format => ExportFormat.Xlsx;

    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public string FileExtension => "xlsx";

    public byte[] Render(ExportDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        using var workbook = new XLWorkbook();
        var generatedAt = document.GeneratedAt.UtcDateTime;
        workbook.Properties.Created = generatedAt;
        workbook.Properties.Modified = generatedAt;
        workbook.Properties.Author = texts[ExportTextKeys.AppName];
        workbook.Properties.LastModifiedBy = texts[ExportTextKeys.AppName];

        AddCriteriaSheet(workbook, document.Content);
        AddInfoSheet(workbook, document.Content, generatedAt);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void AddCriteriaSheet(XLWorkbook workbook, ExportContent content)
    {
        var name = texts[ExportTextKeys.SheetCriteria];
        var sheet = workbook.Worksheets.Add(name);
        var columns = CriteriaColumns.All;

        for (var c = 0; c < columns.Count; c++)
        {
            sheet.Cell(HeaderRow, c + 1).Value = texts[columns[c].HeaderKey];
            sheet.Column(c + 1).Width = Widths[columns[c].HeaderKey];
        }

        for (var i = 0; i < content.Rows.Count; i++)
        {
            for (var c = 0; c < columns.Count; c++)
            {
                var cell = sheet.Cell(HeaderRow + 1 + i, c + 1);
                SetValue(cell, columns[c].Value(content.Rows[i], i + 1, texts));
                if (columns[c].HeaderKey == ExportTextKeys.ColumnTestStep)
                {
                    LinkTo(cell);
                }
            }
        }

        // An Excel table needs a data row, so a list without criteria gets one empty row.
        var lastRow = HeaderRow + Math.Max(1, content.Rows.Count);
        var range = sheet.Range(HeaderRow, 1, lastRow, columns.Count);
        var table = range.CreateTable(name);
        table.Theme = XLTableTheme.None;
        table.ShowRowStripes = false;
        table.ShowAutoFilter = true;
        table.HeadersRow().Style.Font.Bold = true;
        range.Style.Alignment.WrapText = true;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        sheet.SheetView.FreezeRows(HeaderRow);
    }

    private void AddInfoSheet(XLWorkbook workbook, ExportContent content, DateTime generatedAt)
    {
        var sheet = workbook.Worksheets.Add(texts[ExportTextKeys.SheetInfo]);
        var row = 0;

        IXLCell Line(string? label, XLCellValue value)
        {
            row++;
            if (label is not null)
            {
                sheet.Cell(row, 1).Value = label;
                sheet.Cell(row, 1).Style.Font.Bold = true;
            }

            var cell = sheet.Cell(row, 2);
            cell.Value = value;
            return cell;
        }

        Line(texts[ExportTextKeys.InfoTool], texts[ExportTextKeys.AppName]);
        var url = Line(texts[ExportTextKeys.InfoUrl], toolUrl.Value.AbsoluteUri);
        url.SetHyperlink(new XLHyperlink(toolUrl.Value));
        Line(texts[ExportTextKeys.InfoGenerated], generatedAt.Date).Style.DateFormat.Format = DateFormat;
        Line(texts[ExportTextKeys.InfoRelease], content.ReleaseVersion);

        Line(texts[ExportTextKeys.UseCase], content.UseCase?.Name ?? texts[ExportTextKeys.NoUseCase]);
        Line(texts[ExportTextKeys.AppTypes], Names(content.AppTypes));
        Line(texts[ExportTextKeys.ProductParts], Names(content.ProductParts));
        if (content.TestTypes.Count > 0)
        {
            Line(texts[ExportTextKeys.TestTypes], Names(content.TestTypes));
        }

        // Requirement criteria by the level that applies; „Geprüft nach …“ is not counted (UX copy U106).
        Line(texts[ExportTextKeys.InfoCounts], Blank.Value);
        foreach (var level in Enum.GetValues<PriorityLevel>())
        {
            var count = content.Rows.Count(r => r.Kind == CriterionKind.Requirement && r.EffectiveLevel == level);
            Line(texts.Level(level), count);
        }

        Line(texts[ExportTextKeys.InfoNotes], texts[ExportTextKeys.PriorityNote]);
        Line(null, texts[ExportTextKeys.LegalNote]);
        row++;
        sheet.Cell(row, 1).Value = texts[ExportTextKeys.InfoSource];

        sheet.Column(1).Width = 28;
        sheet.Column(2).Width = 100;
        var labelsAndValues = sheet.Range(1, 1, row - 1, 2).Style.Alignment;
        labelsAndValues.WrapText = true;
        labelsAndValues.Vertical = XLAlignmentVerticalValues.Top;
    }

    // Names of a selection layer, separated by commas (UX copy `criteria.list.summary.appTypes`).
    private static string Names(IReadOnlyList<CatalogValueName> values) => string.Join(", ", values.Select(v => v.Name));

    private static void SetValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                break;
            case int number:
                cell.Value = number;
                break;
            case string text:
                cell.Value = text;
                break;
            default:
                throw new ArgumentException($"A cell cannot hold a {value.GetType().Name}.", nameof(value));
        }
    }

    private static void LinkTo(IXLCell cell)
    {
        if (cell.Value.IsText
            && Uri.TryCreate(cell.Value.GetText(), UriKind.Absolute, out var url)
            && (url.Scheme == Uri.UriSchemeHttps || url.Scheme == Uri.UriSchemeHttp))
        {
            cell.SetHyperlink(new XLHyperlink(url));
        }
    }
}
