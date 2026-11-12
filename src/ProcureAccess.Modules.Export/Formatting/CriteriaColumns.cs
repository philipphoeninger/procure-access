// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Export.Documents;
using ProcureAccess.Modules.Export.Texts;

namespace ProcureAccess.Modules.Export.Formatting;

/// <summary>
/// A column of the criteria table: the UX copy key of its header and its value for a row with its
/// running number (from 1). A value is an <see cref="int"/>, a <see cref="string"/> or null (empty cell).
/// </summary>
internal sealed record CriteriaColumn(string HeaderKey, Func<ExportRow, int, ExportTexts, object?> Value);

/// <summary>
/// The columns of the criteria table in the order of SPEC §8.3, shared by every format so that CSV
/// and XLSX hold the same cells. The CSV puts „Katalogversion“ before them (EXP-03).
/// </summary>
internal static class CriteriaColumns
{
    public static IReadOnlyList<CriteriaColumn> All { get; } =
    [
        new(ExportTextKeys.ColumnIndex, (_, index, _) => index),
        new(ExportTextKeys.ColumnKey, (row, _, _) => row.Key),

        // Null for a „Geprüft nach …“ row (SPEC §8.3), and so is the original title.
        new(ExportTextKeys.ColumnNumber, (row, _, _) => row.Number),

        // The plain-language title, else the catalog title, as the H2 of S11 (O-S4-32).
        new(ExportTextKeys.ColumnTitle, (row, _, _) => row.PlainLanguageTitle ?? row.Title),
        new(ExportTextKeys.ColumnOriginalTitle, (row, _, _) => row.OriginalTitle),

        // Empty while the project team has not written it (ADR-016).
        new(ExportTextKeys.ColumnShortDescription, (row, _, _) => row.ShortDescription),
        new(ExportTextKeys.ColumnProductPart, (row, _, _) => row.ProductPart),
        new(ExportTextKeys.ColumnRecommended, (row, _, texts) => texts.Level(row.RecommendedLevel)),

        // The level that applies: the user's choice, else the recommendation (owner decision O-S4-65 (a)).
        new(ExportTextKeys.ColumnChosen, (row, _, texts) => texts.Level(row.EffectiveLevel)),

        // The weight that applies; only B has one.
        new(ExportTextKeys.ColumnWeight, (row, _, _) => row.Weight),
        new(ExportTextKeys.ColumnRationale, (row, _, _) => row.Rationale),
        new(ExportTextKeys.ColumnNote, (row, _, _) => row.Note),
        new(ExportTextKeys.ColumnTestStep, (row, _, _) => row.TestStepUrl),
    ];
}
