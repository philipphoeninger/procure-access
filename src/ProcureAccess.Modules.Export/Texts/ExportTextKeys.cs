// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;

namespace ProcureAccess.Modules.Export.Texts;

/// <summary>
/// The UX copy keys the export files use (UX copy §5.6 „Exportdatei“, owner decision O-S4-62 (a)).
/// Their texts are in <c>Texts/de.json</c>, which <c>scripts/check-i18n.mjs</c> compares with the
/// UX copy; <see cref="ExportTexts"/> refuses to start without every key of <see cref="Required"/>.
/// </summary>
internal static class ExportTextKeys
{
    public const string SheetCriteria = "exportFile.sheet.criteria";
    public const string SheetInfo = "exportFile.sheet.info";
    public const string ColumnRelease = "exportFile.column.release";
    public const string ColumnIndex = "exportFile.column.index";
    public const string ColumnKey = "exportFile.column.key";
    public const string ColumnNumber = "exportFile.column.number";
    public const string ColumnTitle = "exportFile.column.title";
    public const string ColumnOriginalTitle = "exportFile.column.originalTitle";
    public const string ColumnShortDescription = "exportFile.column.shortDescription";
    public const string ColumnProductPart = "exportFile.column.productPart";
    public const string ColumnRecommended = "exportFile.column.recommended";
    public const string ColumnChosen = "exportFile.column.chosen";
    public const string ColumnWeight = "exportFile.column.weight";
    public const string ColumnRationale = "exportFile.column.rationale";
    public const string ColumnNote = "exportFile.column.note";
    public const string ColumnTestStep = "exportFile.column.testStep";

    public const string InfoTool = "exportFile.info.tool";
    public const string InfoUrl = "exportFile.info.url";
    public const string InfoGenerated = "exportFile.info.generated";
    public const string InfoRelease = "exportFile.info.release";
    public const string InfoCounts = "exportFile.info.counts";
    public const string InfoNotes = "exportFile.info.notes";
    public const string InfoSource = "exportFile.info.source";

    // Screen texts the sheet „Info“ reuses (UX copy U109).
    public const string AppName = "shell.appName";
    public const string UseCase = "criteria.list.summary.useCase";
    public const string AppTypes = "criteria.list.summary.appTypes";
    public const string ProductParts = "criteria.list.summary.parts";
    public const string TestTypes = "criteria.list.summary.testTypes";
    public const string NoUseCase = "criteria.select.step1.none";
    public const string PriorityNote = "criteria.list.priorityNote";
    public const string LegalNote = "criteria.export.legalNote";

    /// <summary>The label of a priority level, as the S11 chip shows it (SPEC SD6, provisional until O-14).</summary>
    public static string Level(PriorityLevel level) => level switch
    {
        PriorityLevel.A => "criteria.priority.A",
        PriorityLevel.B => "criteria.priority.B",
        PriorityLevel.Info => "criteria.priority.Info",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown priority level."),
    };

    /// <summary>Every key the renderers read; a key missing in <c>de.json</c> stops the start.</summary>
    public static IReadOnlyList<string> Required { get; } =
    [
        SheetCriteria, SheetInfo,
        ColumnRelease, ColumnIndex, ColumnKey, ColumnNumber, ColumnTitle, ColumnOriginalTitle,
        ColumnShortDescription, ColumnProductPart, ColumnRecommended, ColumnChosen, ColumnWeight,
        ColumnRationale, ColumnNote, ColumnTestStep,
        InfoTool, InfoUrl, InfoGenerated, InfoRelease, InfoCounts, InfoNotes, InfoSource,
        AppName, UseCase, AppTypes, ProductParts, TestTypes, NoUseCase, PriorityNote, LegalNote,
        .. Enum.GetValues<PriorityLevel>().Select(Level),
    ];
}
