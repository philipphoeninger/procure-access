// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace ProcureAccess.Modules.Export.Requests;

// The body of POST /api/v1/exports (ARCHITECTURE §7.2 „Export without account“, Flow A1). Every
// member is nullable, so a missing value becomes a field error of the request check
// (AdHocSelectionSource) instead of a binding failure. Unknown members are refused: a client
// cannot hand in a recommended level, a rationale or anything else the server resolves itself
// (EXP-07); the error names the member's JSON path.

/// <summary>The ad-hoc export request: format, selection and one decision per criterion of the selection.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ExportRequest(
    string? Format,
    ExportSelectionRequest? Selection,
    IReadOnlyList<ExportCriterionRequest?>? Criteria);

/// <summary>The selection by filter-value keys under the release the client's list was built for.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ExportSelectionRequest(
    string? ReleaseVersion,
    string? UseCaseTemplateId,
    IReadOnlyList<string?>? AppTypeIds,
    IReadOnlyList<string?>? ProductPartIds,
    IReadOnlyList<string?>? TestTypeIds);

/// <summary>
/// The user's decision on one criterion of the list (R-LST-03 – R-LST-05): included or not, the
/// level and weight the user chose instead of the recommendation, and a note.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ExportCriterionRequest(
    int? CriterionRevisionId,
    bool? IsIncluded,
    string? OverrideLevel,
    int? OverrideWeight,
    string? Note);
