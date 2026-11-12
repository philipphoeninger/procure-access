// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;

namespace ProcureAccess.Modules.Export.Documents;

/// <summary>
/// What an export source hands to the formatters (ARCHITECTURE §4.4 „One formatter, two sources“):
/// the release, the selection by name and one row per included criterion in list order. The same
/// input gives the same content from either source; only <see cref="ExportDocument"/> adds the
/// generation date.
/// </summary>
internal sealed record ExportContent(
    string ReleaseVersion,
    CatalogValueName? UseCase,
    IReadOnlyList<CatalogValueName> AppTypes,
    IReadOnlyList<CatalogValueName> ProductParts,
    IReadOnlyList<CatalogValueName> TestTypes,
    IReadOnlyList<ExportRow> Rows);

/// <summary>
/// One included criterion with the values of the columns of SPEC §8.3; the running number is its
/// position in <see cref="ExportContent.Rows"/>. The short description waits for O-S4-6 (S4-20).
/// </summary>
/// <param name="Number">The EN 301 549 number; null for a „Geprüft nach …“ row (<c>PROC-…</c>).</param>
/// <param name="RecommendedLevel">Resolved on the server, never taken from the client (EXP-07).</param>
/// <param name="ChosenLevel">The user's level; null when the user kept the recommendation.</param>
/// <param name="Weight">The weight that applies, only while B applies: the user's, else the recommended one when the recommendation is B too (O-S4-27).</param>
internal sealed record ExportRow(
    string Key,
    CriterionKind Kind,
    string? Number,
    string Title,
    string? PlainLanguageTitle,
    string? OriginalTitle,
    string? ProductPart,
    PriorityLevel RecommendedLevel,
    PriorityLevel? ChosenLevel,
    int? Weight,
    string Rationale,
    string? Note,
    string? TestStepUrl)
{
    /// <summary>The level that applies: the user's choice, else the recommendation.</summary>
    public PriorityLevel EffectiveLevel => ChosenLevel ?? RecommendedLevel;
}

/// <summary>The content with the moment it was generated, which the formatters write into the file and its name.</summary>
internal sealed record ExportDocument(ExportContent Content, DateTimeOffset GeneratedAt);
