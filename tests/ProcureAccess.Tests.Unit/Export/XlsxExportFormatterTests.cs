// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ClosedXML.Excel;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Export.Documents;
using ProcureAccess.Modules.Export.Formatting;
using ProcureAccess.Modules.Export.Texts;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Export;

/// <summary>
/// The XLSX file of SPEC §8.3 (EXP-01 – EXP-03): every file is re-opened with ClosedXML and compared
/// with the content it was written from — the table „Kriterien“ (owner decision O-S4-66 (a)) and
/// the sheet „Info“.
/// </summary>
public class XlsxExportFormatterTests
{
    private const string Release = "2026.1";

    private static readonly DateTimeOffset GeneratedAt = new(2026, 11, 12, 23, 30, 0, TimeSpan.Zero);

    private static readonly string[] Headers =
    [
        "Nr.", "Kriterium", "Nummer", "Titel", "Originaltitel", "Kurzbeschreibung", "Funktionsbereich",
        "Priorität (empfohlen)", "Priorität (Ihre Wahl)", "Gewichtung", "Begründung der Empfehlung",
        "Ihre Notiz", "Link zum Prüfschritt",
    ];

    private static readonly ExportRow RequirementA = new(
        "11.1.3.1c", CriterionKind.Requirement, "11.1.3.1c", "Info und Beziehungen", "Beziehungen verständlich",
        "Überschriften", "Bildschirmanzeige", PriorityLevel.A, PriorityLevel.B, 7,
        "Wichtig für Screenreader.", "Mit Anbieter klären", "https://example.org/11.1.3.1c");

    private static readonly ExportRow RequirementInfo = new(
        "11.2.1", CriterionKind.Requirement, "11.2.1", "Kontrast", null, "Kontrast", null, PriorityLevel.Info, null, null,
        "Hilft bei Sehbehinderung.", null, null);

    private static readonly ExportRow TestedBy = new(
        "PROC-softwareTest", CriterionKind.TestProcedure, null, "Geprüft nach BIT inklusiv Software-Test", null,
        null, null, PriorityLevel.B, null, 5, "Ein Prüfbericht zeigt den Stand.", null, "https://example.org/software");

    private readonly XlsxExportFormatter _formatter = new(ExportTexts.LoadGerman(), new ExportToolUrl(new Uri("https://procureaccess.de")));

    [Fact(DisplayName = "INV:EXP-01 " + nameof(Render_Reopened_TableHoldsEveryRowWithPriorityOverrideAndWeight))]
    [Trait("Invariant", "EXP-01")]
    public void Render_Reopened_TableHoldsEveryRowWithPriorityOverrideAndWeight()
    {
        using var workbook = Reopen(Document(RequirementA, RequirementInfo));

        var rows = DataRows(workbook);
        rows.Count.ShouldBe(2);
        Cell(rows[0], "Nr.").GetValue<int>().ShouldBe(1);
        Cell(rows[0], "Kriterium").GetString().ShouldBe("11.1.3.1c");
        Cell(rows[0], "Nummer").GetString().ShouldBe("11.1.3.1c");
        Cell(rows[0], "Titel").GetString().ShouldBe("Beziehungen verständlich");
        Cell(rows[0], "Originaltitel").GetString().ShouldBe("Überschriften");
        Cell(rows[0], "Kurzbeschreibung").IsEmpty().ShouldBeTrue();
        Cell(rows[0], "Funktionsbereich").GetString().ShouldBe("Bildschirmanzeige");
        Cell(rows[0], "Priorität (empfohlen)").GetString().ShouldBe("A – Mindestanforderung");
        Cell(rows[0], "Priorität (Ihre Wahl)").GetString().ShouldBe("B – Bewertungskriterium");
        Cell(rows[0], "Gewichtung").Value.IsNumber.ShouldBeTrue();
        Cell(rows[0], "Gewichtung").GetValue<int>().ShouldBe(7);
        Cell(rows[0], "Begründung der Empfehlung").GetString().ShouldBe("Wichtig für Screenreader.");
        Cell(rows[0], "Ihre Notiz").GetString().ShouldBe("Mit Anbieter klären");
        Cell(rows[0], "Link zum Prüfschritt").GetString().ShouldBe("https://example.org/11.1.3.1c");
        Cell(rows[0], "Link zum Prüfschritt").GetHyperlink().ExternalAddress.ShouldBe(new Uri("https://example.org/11.1.3.1c"));

        // No plain-language title, no override, no weight, no note.
        Cell(rows[1], "Titel").GetString().ShouldBe("Kontrast");
        Cell(rows[1], "Priorität (Ihre Wahl)").GetString().ShouldBe("Hinweis");
        Cell(rows[1], "Gewichtung").IsEmpty().ShouldBeTrue();
        Cell(rows[1], "Ihre Notiz").IsEmpty().ShouldBeTrue();
        Cell(rows[1], "Link zum Prüfschritt").HasHyperlink.ShouldBeFalse();
    }

    [Fact(DisplayName = "INV:CAT-16 " + nameof(Render_TestedByRow_HasItsKeyAndNoNumberOrOriginalTitle))]
    [Trait("Invariant", "CAT-16")]
    public void Render_TestedByRow_HasItsKeyAndNoNumberOrOriginalTitle()
    {
        using var workbook = Reopen(Document(RequirementA, TestedBy));

        var row = DataRows(workbook)[1];
        Cell(row, "Kriterium").GetString().ShouldBe("PROC-softwareTest");
        Cell(row, "Nummer").IsEmpty().ShouldBeTrue();
        Cell(row, "Originaltitel").IsEmpty().ShouldBeTrue();
        Cell(row, "Priorität (Ihre Wahl)").GetString().ShouldBe("B – Bewertungskriterium");
    }

    [Fact(DisplayName = "INV:EXP-01 " + nameof(Render_TextThatLooksLikeAFormula_StaysText))]
    [Trait("Invariant", "EXP-01")]
    public void Render_TextThatLooksLikeAFormula_StaysText()
    {
        using var workbook = Reopen(Document(RequirementA with { Note = "=HYPERLINK(\"https://example.org\")" }));

        var note = Cell(DataRows(workbook)[0], "Ihre Notiz");
        note.HasFormula.ShouldBeFalse();
        note.GetString().ShouldBe("=HYPERLINK(\"https://example.org\")");
    }

    [Fact(DisplayName = "INV:EXP-02 " + nameof(Render_CriteriaSheet_IsATableNamedKriterienWithAFrozenGermanHeaderRow))]
    [Trait("Invariant", "EXP-02")]
    public void Render_CriteriaSheet_IsATableNamedKriterienWithAFrozenGermanHeaderRow()
    {
        using var workbook = Reopen(Document(RequirementA, RequirementInfo, TestedBy));

        workbook.Worksheets.Select(s => s.Name).ShouldBe(["Kriterien", "Info"]);
        var sheet = workbook.Worksheet("Kriterien");
        var table = sheet.Tables.ShouldHaveSingleItem();
        table.Name.ShouldBe("Kriterien");
        workbook.Table("Kriterien").ShouldBeSameAs(table);
        table.ShowHeaderRow.ShouldBeTrue();
        table.RangeAddress.ToString().ShouldBe("A1:M4");
        table.HeadersRow().Cells().Select(c => c.GetString()).ShouldBe(Headers);
        table.HeadersRow().Cells().ShouldAllBe(c => c.Style.Font.Bold);
        sheet.SheetView.SplitRow.ShouldBe(1);
    }

    [Fact(DisplayName = "INV:EXP-02 " + nameof(Render_EverySheet_SaysNothingByColour))]
    [Trait("Invariant", "EXP-02")]
    public void Render_EverySheet_SaysNothingByColour()
    {
        using var workbook = Reopen(Document(RequirementA, RequirementInfo, TestedBy));

        workbook.Table("Kriterien").Theme.ShouldBe(XLTableTheme.None);
        workbook.Table("Kriterien").ShowRowStripes.ShouldBeFalse();
        foreach (var sheet in workbook.Worksheets)
        {
            sheet.ConditionalFormats.ShouldBeEmpty(sheet.Name);
            sheet.CellsUsed().ShouldAllBe(c => c.Style.Fill.PatternType == XLFillPatternValues.None);
        }
    }

    [Fact(DisplayName = "INV:EXP-02 " + nameof(Render_NoIncludedCriterion_IsATableWithOneEmptyRow))]
    [Trait("Invariant", "EXP-02")]
    public void Render_NoIncludedCriterion_IsATableWithOneEmptyRow()
    {
        using var workbook = Reopen(Document());

        var table = workbook.Table("Kriterien");
        table.RangeAddress.ToString().ShouldBe("A1:M2");
        table.DataRange.CellsUsed().ShouldBeEmpty();
    }

    [Fact(DisplayName = "INV:EXP-03 " + nameof(Render_InfoSheet_NamesToolDateReleaseSelectionCountsNotesAndSource))]
    [Trait("Invariant", "EXP-03")]
    public void Render_InfoSheet_NamesToolDateReleaseSelectionCountsNotesAndSource()
    {
        var content = new ExportContent(
            Release,
            new CatalogValueName("hr", "HR-Anwendung"),
            [new CatalogValueName("web", "Website"), new CatalogValueName("mobile", "Mobile Anwendung")],
            [new CatalogValueName("base", "Basisfunktionalität")],
            [new CatalogValueName("softwareTest", "BIT inklusiv Software-Test")],
            [RequirementA, RequirementInfo, TestedBy]);

        using var workbook = Reopen(new ExportDocument(content, GeneratedAt));

        var info = InfoLines(workbook);
        info.ShouldBe(
        [
            ("Werkzeug", "ProcureAccess"),
            ("Adresse", "https://procureaccess.de/"),
            ("Erstellt am", "12.11.2026"),
            ("Katalogversion", Release),
            ("Verwendungszweck", "HR-Anwendung"),
            ("Anwendungstyp", "Website, Mobile Anwendung"),
            ("Funktionsbereiche", "Basisfunktionalität"),
            ("Prüfverfahren", "BIT inklusiv Software-Test"),
            ("Kriterien je Priorität", ""),
            ("A – Mindestanforderung", "0"),
            ("B – Bewertungskriterium", "1"),
            ("Hinweis", "1"),
            ("Hinweise", Texts["criteria.list.priorityNote"]),
            ("", Texts["criteria.export.legalNote"]),
            (Texts["exportFile.info.source"], ""),
        ]);
        var sheet = workbook.Worksheet("Info");
        sheet.Cell(2, 2).GetHyperlink().ExternalAddress.ShouldBe(new Uri("https://procureaccess.de"));
        sheet.Cell(3, 2).Value.IsDateTime.ShouldBeTrue();
    }

    [Fact]
    public void Render_NoUseCaseAndNoTestType_SaysKeinerDavonPasstAndLeavesTestTypesOut()
    {
        using var workbook = Reopen(Document(RequirementA));

        var info = InfoLines(workbook);
        info.ShouldContain(("Verwendungszweck", "Keiner davon passt"));
        info.Select(l => l.Label).ShouldNotContain("Prüfverfahren");
    }

    [Fact]
    public void Render_Document_TakesItsTimeAndAuthorFromTheExportNotTheServer()
    {
        using var workbook = Reopen(Document(RequirementA));

        // ClosedXML reads the stored UTC time back as local time; the instant is what counts.
        workbook.Properties.Created.ToUniversalTime().ShouldBe(GeneratedAt.UtcDateTime);
        workbook.Properties.Modified.ToUniversalTime().ShouldBe(GeneratedAt.UtcDateTime);
        workbook.Properties.Author.ShouldBe("ProcureAccess");
        workbook.Properties.LastModifiedBy.ShouldBe("ProcureAccess");
        _formatter.ContentType.ShouldBe("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        _formatter.FileExtension.ShouldBe("xlsx");
    }

    private static readonly ExportTexts Texts = ExportTexts.LoadGerman();

    private XLWorkbook Reopen(ExportDocument document) => new(new MemoryStream(_formatter.Render(document)));

    private static List<IXLRangeRow> DataRows(XLWorkbook workbook) => [.. workbook.Table("Kriterien").DataRange.Rows()];

    private static IXLCell Cell(IXLRangeRow row, string header) => row.Cell(Array.IndexOf(Headers, header) + 1);

    private static List<(string Label, string Value)> InfoLines(XLWorkbook workbook)
    {
        var sheet = workbook.Worksheet("Info");
        var last = sheet.LastRowUsed()!.RowNumber();
        return [.. Enumerable.Range(1, last).Select(r => (sheet.Cell(r, 1).GetFormattedString(), sheet.Cell(r, 2).GetFormattedString()))];
    }

    private static ExportDocument Document(params ExportRow[] rows) =>
        new(new ExportContent(Release, null, [new CatalogValueName("web", "Website")], [new CatalogValueName("base", "Basisfunktionalität")], [], rows), GeneratedAt);
}
