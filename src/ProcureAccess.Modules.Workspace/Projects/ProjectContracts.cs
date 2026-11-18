// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace ProcureAccess.Modules.Workspace.Projects;

// Bodies and answers of the project endpoints (SPEC R-PRJ-01, R-PRJ-05; S6-11). Internal like the
// endpoints. Unknown members of a body are refused, as the saved lists do.

/// <summary>Body of <c>POST /api/v1/organizations/{organizationId}/projects</c>.</summary>
/// <param name="Name">Required, at most 120 characters after trimming (SPEC §7.2).</param>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record CreateProjectBody(string? Name);

/// <summary>
/// Body of <c>PATCH /api/v1/projects/{id}</c>: what is named changes, what is missing or
/// <c>null</c> stays.
/// </summary>
/// <param name="Name">At most 120 characters after trimming (SPEC §7.2); never on an archived project.</param>
/// <param name="Status"><c>open</c>, <c>done</c> or <c>archived</c>.</param>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ChangeProjectBody(string? Name, string? Status);

/// <summary>
/// A project as its pages read it (S32 and the answers of creating and changing it). The creator is
/// stored but not served (owner decision S6-11 4 (a)).
/// </summary>
/// <param name="Status"><c>open</c>, <c>done</c> or <c>archived</c> (<c>projects.status.*</c>).</param>
/// <param name="ProductCount">The number of candidate products; 0 until they exist (S6-15).</param>
/// <param name="ModifiedAt">The last change of the project.</param>
/// <param name="CriteriaList">The project's own copy of a list (S6-12, C53); <c>null</c> while it has none.</param>
internal sealed record ProjectResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string Status,
    int ProductCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt,
    ProjectCriteriaListResponse? CriteriaList);

/// <summary>
/// Body of <c>PUT /api/v1/projects/{id}/criteria-configuration</c> (R-PRJ-02, R-PRJ-04; S6-12):
/// the organisation list the project gets a copy of.
/// </summary>
/// <param name="SourceConfigurationId">Required; a saved list of the project's organisation, not a project's copy.</param>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record AssignCriteriaListBody(Guid? SourceConfigurationId);

/// <summary>
/// The project's copy of a list (C53): its id — S11 opens it through <c>/api/v1/configurations/{id}</c>
/// (C51) —, its name, which is the name of the list it was copied from
/// (<c>projects.detail.criteria.copyNote</c>), and its catalog release.
/// </summary>
internal sealed record ProjectCriteriaListResponse(Guid Id, string Name, string ReleaseVersion);

/// <summary>A project as S30 lists it: name, status, number of products, last change (R-PRJ-01).</summary>
internal sealed record ProjectSummaryResponse(
    Guid Id,
    string Name,
    string Status,
    int ProductCount,
    DateTimeOffset ModifiedAt);
