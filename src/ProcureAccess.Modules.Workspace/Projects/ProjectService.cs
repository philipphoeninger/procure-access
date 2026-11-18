// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Persistence;

namespace ProcureAccess.Modules.Workspace.Projects;

/// <summary>
/// Creates, reads, lists and changes the organisation's procurement projects (SPEC R-PRJ-01,
/// R-PRJ-05; S6-11) for <see cref="ProjectEndpoints"/>. Every read runs through the organisation
/// filter of the request (TEN-01); the endpoints check the role on the project's organisation
/// before they call it.
/// </summary>
/// <remarks>
/// <b>Seam for S6-20 (API-03).</b> Every change of a stored project goes through
/// <see cref="ChangeAsync"/>: it loads the row, changes it and saves once. The concurrency token
/// and the <c>If-Match</c> check join there; until then the last change wins.
/// <para>The project's criteria list (S6-12) is stored on Configurator's copy, not on the project
/// (owner decision S6-12 D1 (a)); the answers read it through <see cref="IProjectCriteriaLists"/>.</para>
/// </remarks>
internal sealed class ProjectService(WorkspaceDbContext db, IProjectCriteriaLists criteriaLists, TimeProvider clock)
{
    /// <summary>The number of candidate products of every project until S6-15 stores them.</summary>
    private const int NoProductsYet = 0;

    /// <summary>The wire names of the status, the suffixes of <c>projects.status.*</c>.</summary>
    private static readonly IReadOnlyDictionary<string, ProjectStatus> StatusByName =
        new Dictionary<string, ProjectStatus>(StringComparer.Ordinal)
        {
            ["open"] = ProjectStatus.Open,
            ["done"] = ProjectStatus.Done,
            ["archived"] = ProjectStatus.Archived,
        };

    /// <summary>The names a body may use for the status, in the order of the UX copy.</summary>
    public static IReadOnlyCollection<string> StatusNames => [.. StatusByName.Keys];

    /// <summary>The status of <paramref name="name"/>, or <c>null</c> for an unknown name.</summary>
    public static ProjectStatus? StatusOf(string name) =>
        StatusByName.TryGetValue(name, out var status) ? status : null;

    /// <summary>The wire name of <paramref name="status"/>.</summary>
    public static string NameOf(ProjectStatus status) => StatusByName.Single(pair => pair.Value == status).Key;

    /// <summary>Stores a new project of <paramref name="organizationId"/> with the status „Laufend“.</summary>
    public async Task<ProjectResponse> CreateAsync(
        Guid organizationId, string name, string createdByUserId, CancellationToken cancellationToken)
    {
        var project = ProcurementProject.Create(organizationId, name, createdByUserId, clock.GetUtcNow());
        db.ProcurementProjects.Add(project);
        await db.SaveChangesAsync(cancellationToken);
        return ResponseOf(project, null);
    }

    /// <summary>The project, or <c>null</c> when it does not exist for the request (TEN-01).</summary>
    public async Task<ProjectResponse?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await db.ProcurementProjects.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, cancellationToken) is { } project
            ? ResponseOf(project, await criteriaLists.FindOfProjectAsync(project.Id, cancellationToken))
            : null;

    /// <summary>
    /// Every project of the organisation, also the archived ones, the most recently changed first;
    /// S30 sorts and filters them (owner decision S6-11 3 (a), as S14 does).
    /// </summary>
    public async Task<IReadOnlyList<ProjectSummaryResponse>> ListAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var projects = await db.ProcurementProjects.AsNoTracking()
                               .Where(p => p.OrganizationId == organizationId)
                               .OrderByDescending(p => p.ModifiedAt)
                               .ThenBy(p => p.Id)
                               .Select(p => new { p.Id, p.Name, p.Status, p.ModifiedAt })
                               .ToListAsync(cancellationToken);
        return [.. projects.Select(p => new ProjectSummaryResponse(p.Id, p.Name, NameOf(p.Status), NoProductsYet, p.ModifiedAt))];
    }

    /// <summary>The organisation of the project, or <c>null</c> when it does not exist for the request (TEN-01).</summary>
    public async Task<Guid?> OrganizationOfAsync(Guid id, CancellationToken cancellationToken) =>
        await db.ProcurementProjects.AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => (Guid?)p.OrganizationId)
                .SingleOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Changes the name, the status or both (R-PRJ-05). On an archived project only the status may
    /// change: a change that names a name is refused, also together with a status (owner decision
    /// S6-11 2 (a)). The caller has checked <paramref name="name"/> against the limits.
    /// </summary>
    public async Task<ProjectChange> ChangeAsync(Guid id, string? name, ProjectStatus? status, CancellationToken cancellationToken)
    {
        var project = await db.ProcurementProjects.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (project is null)
        {
            return new ProjectChange(ProjectChangeOutcome.NotFound, null);
        }

        if (project.IsArchived && name is not null)
        {
            return new ProjectChange(ProjectChangeOutcome.Archived, null);
        }

        var now = clock.GetUtcNow();
        if (name is not null)
        {
            project.Rename(name, now);
        }

        if (status is { } newStatus)
        {
            project.ChangeStatus(newStatus, now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return new ProjectChange(
            ProjectChangeOutcome.Done, ResponseOf(project, await criteriaLists.FindOfProjectAsync(project.Id, cancellationToken)));
    }

    private static ProjectResponse ResponseOf(ProcurementProject project, ProjectCriteriaList? list) => new(
        project.Id,
        project.OrganizationId,
        project.Name,
        NameOf(project.Status),
        NoProductsYet,
        project.CreatedAt,
        project.ModifiedAt,
        list is null ? null : new ProjectCriteriaListResponse(list.Id, list.Name, list.ReleaseVersion));
}

/// <summary>How a change of a project ended.</summary>
internal enum ProjectChangeOutcome
{
    Done,

    /// <summary>Unknown, or of another organisation (TEN-01).</summary>
    NotFound,

    /// <summary>The project is archived and the change is more than a status (409 <c>project-archived</c>).</summary>
    Archived,
}

/// <param name="Project">The project after the change; only with <see cref="ProjectChangeOutcome.Done"/>.</param>
internal sealed record ProjectChange(ProjectChangeOutcome Outcome, ProjectResponse? Project);
