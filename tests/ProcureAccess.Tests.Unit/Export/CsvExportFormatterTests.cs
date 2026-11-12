// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Export.Documents;
using ProcureAccess.Modules.Export.Formatting;
using ProcureAccess.Modules.Export.Texts;
using ProcureAccess.Tests.Shared.Export;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Export;

/// <summary>
/// The CSV file of SPEC §8.3 (EXP-03, EXP-04): UTF-8 with BOM, semicolon, CRLF, RFC 4180 quoting,
/// the columns in a fixed order with the catalog release first, and text cells guarded against
/// formulas (owner decision O-S4-67 (a)). The file is read back with <see cref="CsvReader"/>.
/// </summary>
public class CsvExportFormatterTests
{
    private const string Release = "2026.1";

    // SPEC §8.3, German headers in this order, after „Katalogversion“ (EXP-03). A new or moved
    // column changes this list on purpose (EXP-04).
    private static readonly string[] Headers =
    [
        "Katalogversion", "Nr.", "Kriterium", "Nummer", "Titel", "Originaltitel", "Kurzbeschreibung",
        "Funktionsbereich", "Priorität (empfohlen)", "Priorität (Ihre Wahl)", "Gewichtung",
        "Begründung der Empfehlung", "Ihre Notiz", "Link zum Prüfschritt",
    ];

    private static readonly ExportRow Requirement = new(
        "11.1.3.1c", CriterionKind.Requirement, "11.1.3.1c", "Info und Beziehungen", "Beziehungen verständlich",
        "Überschriften und Listen sind als solche erkennbar.", "Überschriften", "Bildschirmanzeige", PriorityLevel.A, PriorityLevel.B, 7,
        "Wichtig für Screenreader.", "Mit Anbieter klären", "https://example.org/11.1.3.1c");

    private static readonly ExportRow TestedBy = new(
        "PROC-softwareTest", CriterionKind.TestProcedure, null, "Geprüft nach BIT inklusiv Software-Test", null,
        null, null, null, PriorityLevel.B, null, 5, "Ein Prüfbericht zeigt den Stand.", null, "https://example.org/software");

    private readonly CsvExportFormatter _formatter = new(ExportTexts.LoadGerman());

    [Fact(DisplayName = "INV:EXP-04 " + nameof(Render_AnyDocument_IsUtf8WithBomAndCrlfAfterEveryRecord))]
    [Trait("Invariant", "EXP-04")]
    public void Render_AnyDocument_IsUtf8WithBomAndCrlfAfterEveryRecord()
    {
        var bytes = _formatter.Render(Document(Requirement, TestedBy));

        bytes.Take(3).ShouldBe(new byte[] { 0xEF, 0xBB, 0xBF });
        var text = new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(bytes, 3, bytes.Length - 3);
        text.ShouldEndWith("\r\n");
        text.Split("\r\n").Length.ShouldBe(4); // header, two records, empty after the last CRLF
        text.Replace("\r\n", string.Empty, StringComparison.Ordinal).ShouldNotContain('\n');
        _formatter.ContentType.ShouldBe("text/csv; charset=utf-8");
        _formatter.FileExtension.ShouldBe("csv");
    }

    [Fact(DisplayName = "INV:EXP-04 " + nameof(Render_HeaderRow_HasTheColumnsOfSpec83InTheirOrder))]
    [Trait("Invariant", "EXP-04")]
    public void Render_HeaderRow_HasTheColumnsOfSpec83InTheirOrder()
    {
        var records = CsvReader.Read(_formatter.Render(Document(Requirement)));

        records[0].ShouldBe(Headers);
        records.ShouldAllBe(record => record.Count == Headers.Length);
    }

    [Fact(DisplayName = "INV:EXP-03 " + nameof(Render_EveryRecord_NamesTheCatalogRelease))]
    [Trait("Invariant", "EXP-03")]
    public void Render_EveryRecord_NamesTheCatalogRelease()
    {
        var records = CsvReader.Read(_formatter.Render(Document(Requirement, TestedBy)));

        records.Skip(1).Select(record => record[0]).ShouldBe([Release, Release]);
    }

    [Fact(DisplayName = "INV:EXP-04 " + nameof(Render_Requirement_WritesEveryColumnOfItsRow))]
    [Trait("Invariant", "EXP-04")]
    public void Render_Requirement_WritesEveryColumnOfItsRow()
    {
        var record = CsvReader.Read(_formatter.Render(Document(Requirement)))[1];

        record.ShouldBe(
        [
            Release, "1", "11.1.3.1c", "11.1.3.1c", "Beziehungen verständlich", "Überschriften",
            "Überschriften und Listen sind als solche erkennbar.", "Bildschirmanzeige", "A – Mindestanforderung", "B – Bewertungskriterium", "7",
            "Wichtig für Screenreader.", "Mit Anbieter klären", "https://example.org/11.1.3.1c",
        ]);
    }

    [Fact]
    public void Render_NoPlainLanguageTitleAndNoOverride_WritesTheCatalogTitleAndTheRecommendation()
    {
        var row = Requirement with
        {
            PlainLanguageTitle = null,
            ShortDescription = null,
            ChosenLevel = null,
            Weight = null,
            Note = null,
        };

        var record = CsvReader.Read(_formatter.Render(Document(row)))[1];

        record[Column("Titel")].ShouldBe("Info und Beziehungen");
        record[Column("Kurzbeschreibung")].ShouldBe(string.Empty);
        record[Column("Priorität (Ihre Wahl)")].ShouldBe("A – Mindestanforderung");
        record[Column("Gewichtung")].ShouldBe(string.Empty);
        record[Column("Ihre Notiz")].ShouldBe(string.Empty);
    }

    [Fact(DisplayName = "INV:CAT-16 " + nameof(Render_TestedByRow_HasItsKeyAndNoNumberOrOriginalTitle))]
    [Trait("Invariant", "CAT-16")]
    public void Render_TestedByRow_HasItsKeyAndNoNumberOrOriginalTitle()
    {
        var records = CsvReader.Read(_formatter.Render(Document(Requirement, TestedBy)));

        var record = records[2];
        record[Column("Nr.")].ShouldBe("2");
        record[Column("Kriterium")].ShouldBe("PROC-softwareTest");
        record[Column("Nummer")].ShouldBe(string.Empty);
        record[Column("Originaltitel")].ShouldBe(string.Empty);
        record[Column("Titel")].ShouldBe("Geprüft nach BIT inklusiv Software-Test");
        record[Column("Priorität (Ihre Wahl)")].ShouldBe("B – Bewertungskriterium");
    }

    [Fact(DisplayName = "INV:EXP-04 " + nameof(Render_NoteWithSeparatorQuoteAndLineBreak_IsQuotedAndReadBackUnchanged))]
    [Trait("Invariant", "EXP-04")]
    public void Render_NoteWithSeparatorQuoteAndLineBreak_IsQuotedAndReadBackUnchanged()
    {
        const string note = "Erst „klären“; dann \"prüfen\"\nZweite Zeile";

        var bytes = _formatter.Render(Document(Requirement with { Note = note }));

        Encoding.UTF8.GetString(bytes).ShouldContain("\"Erst „klären“; dann \"\"prüfen\"\"\nZweite Zeile\"");
        CsvReader.Read(bytes)[1][Column("Ihre Notiz")].ShouldBe(note);
    }

    [Theory]
    [InlineData("=HYPERLINK(\"https://example.org\")")]
    [InlineData("+1")]
    [InlineData("-1")]
    [InlineData("@SUM(A1)")]
    [InlineData("\tTab")]
    [InlineData("\rWagenrücklauf")]
    public void Render_TextThatAFormulaStartsWith_GetsAnApostrophe(string note)
    {
        var record = CsvReader.Read(_formatter.Render(Document(Requirement with { Note = note })))[1];

        record[Column("Ihre Notiz")].ShouldBe("'" + note);
    }

    [Theory]
    [InlineData("Normale Notiz")]
    [InlineData("Mit - Strich und = Zeichen in der Mitte")]
    [InlineData("")]
    public void Render_OtherText_IsWrittenAsItIs(string note)
    {
        var record = CsvReader.Read(_formatter.Render(Document(Requirement with { Note = note })))[1];

        record[Column("Ihre Notiz")].ShouldBe(note);
    }

    [Fact(DisplayName = "INV:EXP-04 " + nameof(Render_NoIncludedCriterion_WritesOnlyTheHeaderRow))]
    [Trait("Invariant", "EXP-04")]
    public void Render_NoIncludedCriterion_WritesOnlyTheHeaderRow()
    {
        var records = CsvReader.Read(_formatter.Render(Document()));

        records.ShouldHaveSingleItem().ShouldBe(Headers);
    }

    private static int Column(string header) => Array.IndexOf(Headers, header);

    private static ExportDocument Document(params ExportRow[] rows) =>
        new(new ExportContent(Release, null, [], [], [], rows), new DateTimeOffset(2026, 11, 12, 9, 0, 0, TimeSpan.Zero));
}
