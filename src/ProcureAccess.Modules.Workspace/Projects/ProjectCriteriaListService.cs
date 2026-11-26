// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Platform.Contracts.Concurrency;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Persistence;

namespace ProcureAccess.Modules.Workspace.Projects;

/// <summary>
/// Assigns or replaces the criteria list of a procurement project (SPEC R-PRJ-02, R-PRJ-04;
/// ARCHITECTURE §5.4, C47, C53; S6-12) for <see cref="ProjectCriteriaListEndpoint"/>: the project
/// gets its own copy of an organisation list, made and stored by Configurator
/// (<see cref="IProjectCriteriaLists"/>), which also deletes the previous copy. The link lives on
/// the copy (owner decision S6-12 D1 (a)); the project row only counts the assignment as a new
/// version.
/// </summary>
/// <remarks>
/// <para><b>The lock (C47, C51, C53; WSP-02; S6-18).</b> The list can be replaced until a candidate
/// of the project has an assessment with a value other than „Nicht eingeschätzt“. Rows with that
/// value — a note without a value — do not lock; they belong to criteria of the replaced copy and
/// are deleted with it (owner decision S6-18 4 (a)). Adjusting the copy within its release is
/// never locked: that goes through the saved-list endpoints.</para>
/// <para><b>One step for the project (O-31, owner decision O-S7-4; S7-08).</b> The copy is stored by
/// Configurator's context, the assessments by this one, and no transaction spans both. So the
/// assignment holds the project's row lock (<see cref="WorkspaceRowLock"/>) from before its checks
/// until its commit, and so does the save of an assessment: an assessment either is stored before
/// the lock is checked, and locks the list, or is checked against the new list. What the lock does
/// not give: when the process ends between Configurator's commit and this one, the copy is replaced
/// while the notes without a value and the project's version are as before.</para>
/// <para><b>Concurrency (API-03; S6-20, owner decision D2 (a)).</b> The assignment is a change of
/// the project — its answers name the list —, so it needs the project's <c>ETag</c> in
/// <c>If-Match</c> and counts a new version of the project. The copy is stored before the version:
/// a version counted for a refused source would make the sender's own tag stale for nothing. A
/// rename or status change sent meanwhile waits for the row lock and then answers 409
/// <c>conflict</c> itself, as its tag is stale by then.</para>
/// </remarks>
internal sealed class ProjectCriteriaListService(WorkspaceDbContext db, IProjectCriteriaLists lists, EntityTags tags)
{
    /// <summary>
    /// Copies <paramref name="sourceConfigurationId"/> into the project. In this order: an unknown
    /// project (also another organisation's, TEN-01), a stale <c>If-Match</c> (API-03), an archived
    /// project (R-PRJ-05), a locked list
    /// (<see cref="IsLockedAsync"/>, C47), then the source — a list of the project's organisation
    /// that is not a project copy (WSP-02).
    /// </summary>
    public Task<CriteriaListAssignment> AssignAsync(
        Guid projectId, Guid sourceConfigurationId, EditPrecondition edit, CancellationToken cancellationToken) =>
        ConcurrentChange.RunAsync(db, () => WorkspaceRowLock.InTransactionAsync(
            db, ct => AssignOnceAsync(projectId, sourceConfigurationId, edit, ct), cancellationToken));

    private async Task<CriteriaListAssignment> AssignOnceAsync(
        Guid projectId, Guid sourceConfigurationId, EditPrecondition edit, CancellationToken cancellationToken)
    {
        // Visible through the filter first: only then the lock, and only then the row as it is now.
        if (!await db.ProcurementProjects.AnyAsync(p => p.Id == projectId, cancellationToken))
        {
            return new CriteriaListAssignment(CriteriaListAssignmentOutcome.NotFound, null);
        }

        await WorkspaceRowLock.LockProjectAsync(db, projectId, cancellationToken);
        var project = await db.ProcurementProjects.SingleOrDefaultAsync(p => p.Id == projectId, cancellationToken);
        if (project is null)
        {
            return new CriteriaListAssignment(CriteriaListAssignmentOutcome.NotFound, null);
        }

        if (!tags.Matches(edit.IfMatch, project.Version))
        {
            return new CriteriaListAssignment(CriteriaListAssignmentOutcome.Conflict, null, EditorUserId: project.ModifiedByUserId);
        }

        if (project.IsArchived)
        {
            return new CriteriaListAssignment(CriteriaListAssignmentOutcome.Archived, null);
        }

        if (await IsLockedAsync(projectId, cancellationToken))
        {
            return new CriteriaListAssignment(CriteriaListAssignmentOutcome.Locked, null);
        }

        var copy = await lists.CopyIntoProjectAsync(
            sourceConfigurationId, projectId, project.OrganizationId, edit.UserId, cancellationToken);
        if (copy.Outcome == ProjectCopyOutcome.Copied)
        {
            // What is left are notes without a value on criteria of the replaced copy.
            await AssessmentsOf(projectId).Where(a => a.Fulfillment == Fulfillment.Unknown).ExecuteDeleteAsync(cancellationToken);
            project.MarkEdited(edit.UserId);
            await db.SaveChangesAsync(cancellationToken);
        }

        return copy.Outcome switch
        {
            ProjectCopyOutcome.Copied => new CriteriaListAssignment(
                CriteriaListAssignmentOutcome.Done, ResponseOf(copy.List!), tags.Of(project.Version)),
            ProjectCopyOutcome.SourceNotFound => new CriteriaListAssignment(CriteriaListAssignmentOutcome.SourceNotFound, null),
            ProjectCopyOutcome.SourceIsProjectCopy => new CriteriaListAssignment(CriteriaListAssignmentOutcome.SourceIsProjectCopy, null),
            _ => throw new InvalidOperationException($"Unknown outcome {copy.Outcome} of a project copy."),
        };
    }

    /// <summary>
    /// <c>true</c> when a candidate of the project has an assessment other than „Nicht
    /// eingeschätzt“, so its list can no longer be replaced (WSP-02).
    /// </summary>
    public async Task<bool> IsLockedAsync(Guid projectId, CancellationToken cancellationToken) =>
        await AssessmentsOf(projectId).AnyAsync(a => a.Fulfillment != Fulfillment.Unknown, cancellationToken);

    private IQueryable<CandidateAssessment> AssessmentsOf(Guid projectId) =>
        db.CandidateAssessments.Where(a => db.CandidateProducts.Any(
            c => c.Id == a.CandidateProductId && c.ProcurementProjectId == projectId));

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

    /// <summary>The <c>If-Match</c> is not the tag of the project's stored version (409 <c>conflict</c>, API-03).</summary>
    Conflict,
}

/// <param name="List">The project's new copy; only with <see cref="CriteriaListAssignmentOutcome.Done"/>.</param>
/// <param name="ProjectEtag">The <c>ETag</c> of the project after the assignment; only with <see cref="CriteriaListAssignmentOutcome.Done"/>.</param>
/// <param name="EditorUserId">Who made the project's stored version; only with <see cref="CriteriaListAssignmentOutcome.Conflict"/>.</param>
internal sealed record CriteriaListAssignment(
    CriteriaListAssignmentOutcome Outcome, ProjectCriteriaListResponse? List, string? ProjectEtag = null, string? EditorUserId = null);
