// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Persistence;

namespace ProcureAccess.Modules.Workspace.Assessments;

/// <summary>
/// Stores the assessment of one criterion for a candidate (SPEC R-CAN-06; ARCHITECTURE §5.4, C31,
/// C53; CAT-15, CAT-17; S6-18) for <see cref="AssessmentEndpoints"/>.
/// </summary>
/// <remarks>
/// <para><b>Which criteria.</b> Only an included requirement criterion of the project's list can be
/// assessed: a procedure criterion is derived (CAT-17), an excluded criterion is hidden and keeps
/// what it has (C51), and without a list there is nothing to assess — one answer for all
/// (<see cref="AssessmentOutcome.NotInList"/>, owner decision S6-18 2 (a)).</para>
/// <para><b>Lazy rows.</b> The row is stored with the first value or note and removed when it has
/// neither (C53).</para>
/// <para><b>Last write wins</b> (R-CAN-11): no <c>If-Match</c>. Two first writes of the same row at
/// once meet at the table's key; the one that loses is repeated on the row the other stored.</para>
/// <para><b>Last change.</b> A changed assessment moves <c>ModifiedAt</c> of the candidate and of
/// its project (owner decision S6-18 3 (a), as S6-15 5 (a)).</para>
/// </remarks>
internal sealed class AssessmentService(WorkspaceDbContext db, AssessmentReader reader, TimeProvider clock)
{
    /// <summary>
    /// Sets value and note of the criterion <paramref name="criterionRevisionId"/> for the candidate.
    /// The caller has checked the note against its limit; <c>null</c> is no note.
    /// </summary>
    public async Task<AssessmentResult> SetAsync(
        Guid candidateId, int criterionRevisionId, Fulfillment value, string? note, CancellationToken cancellationToken)
    {
        try
        {
            return await SetOnceAsync(candidateId, criterionRevisionId, value, note, cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Another request stored or removed the same row in between, or gave the candidate or
            // the project, which this save only stamps, a new version (S6-20). On a fresh read the
            // same change succeeds: the last write wins (R-CAN-11).
            db.ChangeTracker.Clear();
            return await SetOnceAsync(candidateId, criterionRevisionId, value, note, cancellationToken);
        }
    }

    private async Task<AssessmentResult> SetOnceAsync(
        Guid candidateId, int criterionRevisionId, Fulfillment value, string? note, CancellationToken cancellationToken)
    {
        var candidate = await db.CandidateProducts.SingleOrDefaultAsync(c => c.Id == candidateId, cancellationToken);
        if (candidate is null)
        {
            return new AssessmentResult(AssessmentOutcome.NotFound, null);
        }

        // The same organisation as the candidate, so the filter shows it.
        var project = await db.ProcurementProjects.SingleAsync(p => p.Id == candidate.ProcurementProjectId, cancellationToken);
        if (project.IsArchived)
        {
            return new AssessmentResult(AssessmentOutcome.ProjectArchived, null);
        }

        var scope = await reader.ScopeOfProjectAsync(project.Id, cancellationToken);
        var criterion = scope?.Requirements.SingleOrDefault(c => c.RevisionId == criterionRevisionId);
        if (criterion is null)
        {
            return new AssessmentResult(AssessmentOutcome.NotInList, null);
        }

        if (!AssessmentProjection.IsAllowed(value, criterion.AllowedRatings))
        {
            return new AssessmentResult(AssessmentOutcome.ValueNotAllowed, null, AssessmentProjection.NamesOf(criterion.AllowedRatings));
        }

        var row = await db.CandidateAssessments.SingleOrDefaultAsync(
            a => a.CandidateProductId == candidateId && a.CriterionRevisionId == criterionRevisionId, cancellationToken);
        var isNew = row is null;
        row ??= CandidateAssessment.Of(candidate, criterionRevisionId);
        var now = clock.GetUtcNow();
        if (row.Change(value, note, now))
        {
            if (isNew)
            {
                db.CandidateAssessments.Add(row);
            }
            else if (row.SaysNothing)
            {
                db.CandidateAssessments.Remove(row);
            }

            candidate.MarkChanged(now);
            project.MarkChanged(now);
            await db.SaveChangesAsync(cancellationToken);
        }

        return new AssessmentResult(
            AssessmentOutcome.Done, new AssessmentResponse(criterionRevisionId, AssessmentProjection.NameOf(value), note));
    }
}

/// <summary>How setting an assessment ended.</summary>
internal enum AssessmentOutcome
{
    Done,

    /// <summary>The candidate is unknown, or of another organisation (TEN-01).</summary>
    NotFound,

    /// <summary>The project is archived (409 <c>project-archived</c>, R-PRJ-05).</summary>
    ProjectArchived,

    /// <summary>
    /// Not an included requirement criterion of the project's list: a procedure criterion (CAT-17),
    /// an excluded one, one of no or another list (400 at <c>criterionRevisionId</c>).
    /// </summary>
    NotInList,

    /// <summary>The criterion does not allow the value (400 at <c>value</c>, CAT-15).</summary>
    ValueNotAllowed,
}

/// <param name="Assessment">Only with <see cref="AssessmentOutcome.Done"/>.</param>
/// <param name="AllowedRatings">The levels the criterion allows; only with <see cref="AssessmentOutcome.ValueNotAllowed"/>.</param>
internal sealed record AssessmentResult(
    AssessmentOutcome Outcome, AssessmentResponse? Assessment, IReadOnlyList<string>? AllowedRatings = null);
