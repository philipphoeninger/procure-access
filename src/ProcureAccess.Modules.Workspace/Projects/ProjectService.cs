// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Platform.Contracts.Concurrency;
using ProcureAccess.Modules.Workspace.Assessments;
using ProcureAccess.Modules.Workspace.Candidates;
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
/// <b>Concurrency (API-03; S6-20).</b> A change of a stored project names the version its sender
/// has seen (<see cref="EditPrecondition"/>): <see cref="ChangeAsync"/> loads the row, refuses a
/// stale <c>If-Match</c>, changes it and saves once; a change counts a new version
/// (<see cref="ProcurementProject.MarkEdited"/>). A save that met a concurrent one runs again
/// (<see cref="ConcurrentChange"/>).
/// <para>The project's criteria list (S6-12) is stored on Configurator's copy, not on the project
/// (owner decision S6-12 D1 (a)); the answers read it through <see cref="IProjectCriteriaLists"/>.</para>
/// <para>Every candidate of a project with a list carries its progress, the seven counts and the
/// derived procedure answers (<see cref="AssessmentReader"/>; C34, SIG-13; S6-18).</para>
/// </remarks>
internal sealed class ProjectService(
    WorkspaceDbContext db, IProjectCriteriaLists criteriaLists, AssessmentReader assessments, EntityTags tags, TimeProvider clock)
{
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
        return ResponseOf(project, [], null);
    }

    /// <summary>The project, or <c>null</c> when it does not exist for the request (TEN-01).</summary>
    public async Task<ProjectResponse?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await db.ProcurementProjects.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, cancellationToken) is { } project
            ? await ResponseOfAsync(project, cancellationToken)
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
                               .Select(p => new
                               {
                                   p.Id,
                                   p.Name,
                                   p.Status,
                                   p.ModifiedAt,
                                   ProductCount = db.CandidateProducts.Count(c => c.ProcurementProjectId == p.Id),
                               })
                               .ToListAsync(cancellationToken);
        return [.. projects.Select(p => new ProjectSummaryResponse(p.Id, p.Name, NameOf(p.Status), p.ProductCount, p.ModifiedAt))];
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
    /// S6-11 2 (a)). The caller has checked <paramref name="name"/> against the limits. A stale
    /// <c>If-Match</c> is refused before anything else (API-03); the same values change nothing and
    /// keep the version.
    /// </summary>
    public Task<ProjectChange> ChangeAsync(
        Guid id, string? name, ProjectStatus? status, EditPrecondition edit, CancellationToken cancellationToken) =>
        ConcurrentChange.RunAsync(db, async () =>
        {
            var project = await db.ProcurementProjects.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (project is null)
            {
                return new ProjectChange(ProjectChangeOutcome.NotFound, null);
            }

            if (!tags.Matches(edit.IfMatch, project.Version))
            {
                return new ProjectChange(ProjectChangeOutcome.Conflict, null, project.ModifiedByUserId);
            }

            if (project.IsArchived && name is not null)
            {
                return new ProjectChange(ProjectChangeOutcome.Archived, null);
            }

            var now = clock.GetUtcNow();
            var before = (project.Name, project.Status);
            if (name is not null)
            {
                project.Rename(name, now);
            }

            if (status is { } newStatus)
            {
                project.ChangeStatus(newStatus, now);
            }

            if ((project.Name, project.Status) != before)
            {
                project.MarkEdited(edit.UserId);
            }

            await db.SaveChangesAsync(cancellationToken);
            return new ProjectChange(ProjectChangeOutcome.Done, await ResponseOfAsync(project, cancellationToken));
        });

    private async Task<ProjectResponse> ResponseOfAsync(ProcurementProject project, CancellationToken cancellationToken)
    {
        var list = await criteriaLists.FindOfProjectAsync(project.Id, cancellationToken);
        return ResponseOf(project, await CandidatesOfAsync(project.Id, list, cancellationToken), list);
    }

    /// <summary>
    /// The candidates of the project for S32, in the order they were added (S6-15), each with its
    /// assessment summary against <paramref name="list"/> (S6-18).
    /// </summary>
    private async Task<IReadOnlyList<CandidateSummaryResponse>> CandidatesOfAsync(
        Guid projectId, ProjectCriteriaList? list, CancellationToken cancellationToken)
    {
        var candidates = await db.CandidateProducts.AsNoTracking()
                                 .Where(c => c.ProcurementProjectId == projectId)
                                 .OrderBy(c => c.CreatedAt)
                                 .ThenBy(c => c.Id)
                                 .Select(c => new
                                 {
                                     c.Id,
                                     c.Name,
                                     c.VendorName,
                                     c.TestStatus,
                                     EvidenceTestTypeKeys = c.TestEvidences.Where(e => e.TestTypeKey != null).Select(e => e.TestTypeKey!).ToList(),
                                 })
                                 .ToListAsync(cancellationToken);
        var summaries = await assessments.SummariesAsync(
            list, [.. candidates.Select(c => new AssessedCandidate(c.Id, c.TestStatus, c.EvidenceTestTypeKeys))], cancellationToken);
        return [.. candidates.Select(c =>
        {
            var summary = summaries?[c.Id];
            return new CandidateSummaryResponse(
                c.Id, c.Name, c.VendorName, CandidateNames.Of(c.TestStatus), summary?.Progress, summary?.Fulfillment, summary?.ProcedureAnswers);
        })];
    }

    private ProjectResponse ResponseOf(
        ProcurementProject project, IReadOnlyList<CandidateSummaryResponse> candidates, ProjectCriteriaList? list) => new(
        project.Id,
        project.OrganizationId,
        project.Name,
        NameOf(project.Status),
        candidates.Count,
        candidates,
        project.CreatedAt,
        project.ModifiedAt,
        list is null ? null : new ProjectCriteriaListResponse(list.Id, list.Name, list.ReleaseVersion),
        tags.Of(project.Version));
}

/// <summary>How a change of a project ended.</summary>
internal enum ProjectChangeOutcome
{
    Done,

    /// <summary>Unknown, or of another organisation (TEN-01).</summary>
    NotFound,

    /// <summary>The project is archived and the change is more than a status (409 <c>project-archived</c>).</summary>
    Archived,

    /// <summary>The <c>If-Match</c> is not the tag of the stored version (409 <c>conflict</c>, API-03).</summary>
    Conflict,
}

/// <param name="Project">The project after the change; only with <see cref="ProjectChangeOutcome.Done"/>.</param>
/// <param name="EditorUserId">Who made the stored version; only with <see cref="ProjectChangeOutcome.Conflict"/>.</param>
internal sealed record ProjectChange(ProjectChangeOutcome Outcome, ProjectResponse? Project, string? EditorUserId = null);
