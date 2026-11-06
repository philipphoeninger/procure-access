// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;

namespace ProcureAccess.Modules.Catalog.Releases;

/// <summary>
/// The content of a release folder after it passed the validation (S3-15): everything
/// <c>catalog import</c> (S3-16) needs to build the release, with filter values by key. Criteria
/// are ordered by key (ordinal); every other list keeps the order of its file.
/// </summary>
internal sealed record ReleaseDefinition(
    string Version,
    string? ReleaseNotes,
    IReadOnlyList<CriterionDefinition> Criteria,
    IReadOnlyList<ExclusionDefinition> Exclusions,
    IReadOnlyList<ProcedureMappingDefinition> Procedures);

/// <summary>
/// One criterion with the content of its revision. <see cref="Supersedes"/> names the criteria of an
/// earlier release that this one replaces (owner, S3-15 decision 3a); a merge names several.
/// </summary>
internal sealed record CriterionDefinition(
    string Key,
    CriterionKind Kind,
    string? TestTypeKey,
    string? Category,
    IReadOnlyList<string> Supersedes,
    string TitleDe,
    string? PlainLanguageDe,
    string? RationaleDe,
    int SortOrder,
    IReadOnlyList<StandardReferenceDefinition> StandardReferences,
    IReadOnlyList<ApplicabilityDefinition> Applicability,
    IReadOnlyList<PriorityDefinition> Priorities,
    IReadOnlyList<ProcedureTextDefinition> ProcedureTexts);

internal sealed record StandardReferenceDefinition(Standard Standard, string Clause, string? Url);

/// <summary>One app type, optionally limited to one product part (the folder lists them as a cross product).</summary>
internal sealed record ApplicabilityDefinition(string AppTypeKey, string? ProductPartKey);

/// <summary>A priority rule; both keys null = catalog default (ARCHITECTURE §5.1).</summary>
internal sealed record PriorityDefinition(
    string? AppTypeKey,
    string? UseCaseKey,
    PriorityLevel Level,
    int? Weight,
    string RationaleDe);

internal sealed record ProcedureTextDefinition(string TestTypeKey, ProcedureTextContent Content);

internal sealed record FilterValueReference(FilterLayer Layer, string Key);

internal sealed record ExclusionDefinition(FilterValueReference Source, FilterValueReference Target, string ReasonDe);

internal sealed record ProcedureMappingDefinition(string AppTypeKey, string TestTypeKey);

/// <summary>
/// A validation error: the file relative to the release folder (<c>criteria/11.1.3.1c.json</c>),
/// the JSON path in it (<c>$.procedureTexts[0].allowedRatings[2]</c>; <c>$</c> for the file as a
/// whole) and what is wrong.
/// </summary>
internal sealed record ReleaseFolderError(string File, string Path, string Message)
{
    public override string ToString() => $"{File} {Path}: {Message}";
}

/// <summary>
/// What reading a release folder gave: the definition if the folder is valid, otherwise every error
/// found (and no definition, so nothing half-valid reaches the import; R-ADM-01).
/// </summary>
internal sealed record ReleaseFolderResult(ReleaseDefinition? Release, IReadOnlyList<ReleaseFolderError> Errors)
{
    public bool IsValid => Release is not null;
}
