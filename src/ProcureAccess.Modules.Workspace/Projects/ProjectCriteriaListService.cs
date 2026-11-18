// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Persistence;

namespace ProcureAccess.Modules.Workspace.Projects;

/// <summary>
/// Assigns or replaces the criteria list of a procurement project (SPEC R-PRJ-02, R-PRJ-04;
/// ARCHITECTURE §5.4, C47, C53; S6-12) for <see cref="ProjectCriteriaListEndpoint"/>: the project
/// gets its own copy of an organisation list, made and stored by Configurator
/// (<see cref="IProjectCriteriaLists"/>), which also deletes the previous copy. The project row is
/// not changed: the link lives on the copy (owner decision S6-12 D1 (a)).
/// </summary>
internal sealed class ProjectCriteriaListService(
    WorkspaceDbContext db,
    IProjectCriteriaLists lists,
    ICriteriaListLock listLock)
{
    /// <summary>
    /// Copies <paramref name="sourceConfigurationId"/> into the project. In this order: an unknown
    /// project (also another organisation's, TEN-01), an archived one (R-PRJ-05), a locked list
    /// (<see cref="ICriteriaListLock"/>, C47), then the source — a list of the project's organisation
    /// that is not a project copy (WSP-02).
    /// </summary>
    public async Task<CriteriaListAssignment> AssignAsync(
        Guid projectId, Guid sourceConfigurationId, string userId, CancellationToken cancellationToken)
    {
        var project = await db.ProcurementProjects.AsNoTracking()
                              .Where(p => p.Id == projectId)
                              .Select(p => new { p.OrganizationId, p.Status })
                              .SingleOrDefaultAsync(cancellationToken);
        if (project is null)
        {
            return new CriteriaListAssignment(CriteriaListAssignmentOutcome.NotFound, null);
        }

        if (project.Status == ProjectStatus.Archived)
        {
            return new CriteriaListAssignment(CriteriaListAssignmentOutcome.Archived, null);
        }

        if (await listLock.IsLockedAsync(projectId, cancellationToken))
        {
            return new CriteriaListAssignment(CriteriaListAssignmentOutcome.Locked, null);
        }

        var copy = await lists.CopyIntoProjectAsync(sourceConfigurationId, projectId, project.OrganizationId, userId, cancellationToken);
        return copy.Outcome switch
        {
            ProjectCopyOutcome.Copied => new CriteriaListAssignment(CriteriaListAssignmentOutcome.Done, ResponseOf(copy.List!)),
            ProjectCopyOutcome.SourceNotFound => new CriteriaListAssignment(CriteriaListAssignmentOutcome.SourceNotFound, null),
            ProjectCopyOutcome.SourceIsProjectCopy => new CriteriaListAssignment(CriteriaListAssignmentOutcome.SourceIsProjectCopy, null),
            _ => throw new InvalidOperationException($"Unknown outcome {copy.Outcome} of a project copy."),
        };
    }

    private static ProjectCriteriaListResponse ResponseOf(ProjectCriteriaList list) => new(list.Id, list.Name, list.ReleaseVersion);
}

/// <summary>How an assignment of a list to a project ended.</summary>
internal enum CriteriaListAssignmentOutcome
{
    Done,

    /// <summary>Unknown, or of another organisation (TEN-01).</summary>
    NotFound,

    /// <summary>The project is archived (409 <c>project-archived</c>).</summary>
    Archived,

    /// <summary>An assessment other than „Nicht eingeschätzt“ exists (409 <c>locked-criteria-list</c>, C47).</summary>
    Locked,

    /// <summary>The source is unknown to the request or a list of another organisation (400 at <c>sourceConfigurationId</c>).</summary>
    SourceNotFound,

    /// <summary>The source is a project's copy, never offered as a source (400 at <c>sourceConfigurationId</c>, WSP-02).</summary>
    SourceIsProjectCopy,
}

/// <param name="List">The project's new copy; only with <see cref="CriteriaListAssignmentOutcome.Done"/>.</param>
internal sealed record CriteriaListAssignment(CriteriaListAssignmentOutcome Outcome, ProjectCriteriaListResponse? List);
