// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;
using ProcureAccess.Modules.Configurator.Contracts.Drafts;

namespace ProcureAccess.Modules.Configurator.Configurations;

// Bodies and answers of the saved-list endpoints (SPEC §8.10, S5-20a). Internal like the endpoints;
// the request reuses the draft shape of the anonymous export (DraftSelection, DraftCriterion), so a
// draft S11 can export it can also save.

/// <summary>
/// Body of <c>POST /api/v1/configurations</c>: the organisation the list is saved for (the one the
/// header shows, R-SHL-03), its name and the draft. Unknown members are refused, so a client cannot
/// hand in a recommended level or anything else the server resolves (EXP-07).
/// </summary>
/// <param name="Name">Required, at most 120 characters after trimming (SPEC §7.2).</param>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record SaveConfigurationBody(
    Guid? OrganizationId,
    string? Name,
    DraftSelection? Selection,
    IReadOnlyList<DraftCriterion?>? Criteria);

/// <summary>Answer of a saved list: its id.</summary>
internal sealed record CreatedConfiguration(Guid Id);

/// <summary>
/// A saved list as S11 shows it: the texts and the order of the criteria come from the criteria
/// endpoint of its release; <paramref name="Criteria"/> holds one decision per criterion, by revision id.
/// </summary>
internal sealed record ConfigurationResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string ReleaseVersion,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt,
    ConfigurationSelectionResponse Selection,
    IReadOnlyList<ConfigurationCriterionResponse> Criteria);

/// <summary>The selection by the value keys of the pinned release, named as in the request and the criteria endpoint.</summary>
internal sealed record ConfigurationSelectionResponse(
    string? UseCaseTemplateId,
    IReadOnlyList<string> AppTypeIds,
    IReadOnlyList<string> ProductPartIds,
    IReadOnlyList<string> TestTypeIds);

/// <summary>
/// The decision on one criterion. <paramref name="ResolvedLevel"/> is the level recommended when the
/// list was saved (CAT-08); <paramref name="OverrideLevel"/> is <c>null</c> while it applies.
/// Levels are <c>A</c>, <c>B</c>, <c>Info</c>.
/// </summary>
internal sealed record ConfigurationCriterionResponse(
    int CriterionRevisionId,
    bool IsIncluded,
    string ResolvedLevel,
    string? OverrideLevel,
    int? OverrideWeight,
    string? Note);

/// <summary>A list as S14 lists it (owner decision S5-20 3a: no creator).</summary>
internal sealed record ConfigurationSummaryResponse(
    Guid Id,
    string Name,
    string ReleaseVersion,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt);

/// <summary>
/// Body of <c>PATCH /api/v1/configurations/{id}/criteria/{criterionRevisionId}</c> (R-SAV-03): the
/// whole decision on one criterion, as a row of the save request names it. Unknown members are refused.
/// </summary>
/// <param name="IsIncluded">Required.</param>
/// <param name="OverrideLevel"><c>A</c>, <c>B</c> or <c>Info</c>; <c>null</c> for the recommendation.</param>
/// <param name="OverrideWeight">Within 🔧 <c>Configurator:PriorityWeight</c>, only while B applies.</param>
/// <param name="Note">At most 1 000 characters; empty or <c>null</c> for none.</param>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ConfigurationDecisionBody(bool? IsIncluded, string? OverrideLevel, int? OverrideWeight, string? Note);

/// <summary>
/// Body of <c>PUT /api/v1/configurations/{id}/selection</c> („Auswahl ändern“, R-SAV-03; owner
/// decision S5-21 3a): the new selection in the shape of the save request, under the list's release.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ChangeSelectionBody(DraftSelection? Selection);

/// <summary>
/// Answer of a selection change: the criteria whose decisions the new selection could not keep,
/// because they are no longer part of it (owner decision S5-21 4a).
/// </summary>
internal sealed record ChangedSelection(IReadOnlyList<ConfigurationCriterionName> DroppedDecisions);

/// <summary>
/// Body of <c>POST /api/v1/configurations/{id}/copies</c> (R-LST-14, ARCHITECTURE C58): the release
/// of the copy; only <c>active</c> exists.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record CopyConfigurationBody(string? Release);

/// <summary>
/// Answer of a copy (CAT-22): the new list, its release, the selection values the active release
/// no longer allows (German names, layer order; <c>criteria.list.copy.dropped</c>) and the criteria
/// whose decisions could not be carried over (<c>criteria.list.copy.notCarried</c>).
/// </summary>
internal sealed record CopiedConfiguration(
    Guid Id,
    string ReleaseVersion,
    IReadOnlyList<string> DroppedValues,
    IReadOnlyList<ConfigurationCriterionName> NotCarried);

/// <summary>
/// A criterion of a list as a report names it, under the release the decision was made in: its
/// number (<c>null</c> for „Geprüft nach …“) and its title (the plain-language title, else the
/// catalog title, as the export's „Titel“, SPEC §8.3).
/// </summary>
internal sealed record ConfigurationCriterionName(int CriterionRevisionId, string? Number, string Title);

/// <summary>
/// Body of <c>POST /api/v1/configurations/{id}/share-links</c> (R-SAV-04, S5-23): the last day the
/// link works, as an ISO date (<c>2027-02-14</c>); required, between tomorrow and the 🔧 maximum
/// (TEN-12). Unknown members are refused.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record CreateShareLinkBody(DateOnly? ExpiresOn);

/// <summary>
/// A shared list for someone without an account (R-SAV-05, SPEC §8.5; owner decision S5-23 a): the
/// shape of <see cref="ConfigurationResponse"/> without the list's id, its organisation and its
/// dates, plus the end of the link. No organisation, user or project data (TEN-08); the texts come
/// from the criteria endpoint of <paramref name="ReleaseVersion"/>, as for S11.
/// </summary>
internal sealed record SharedConfigurationResponse(
    string Name,
    string ReleaseVersion,
    DateTimeOffset ExpiresAt,
    ConfigurationSelectionResponse Selection,
    IReadOnlyList<ConfigurationCriterionResponse> Criteria);
