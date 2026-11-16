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
