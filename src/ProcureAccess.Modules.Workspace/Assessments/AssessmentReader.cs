// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Persistence;

namespace ProcureAccess.Modules.Workspace.Assessments;

/// <summary>
/// Reads the assessments of candidates against the project's list (SPEC R-CAN-06 – R-CAN-08;
/// ARCHITECTURE C34, C53; S6-18): the list's criteria under its release, the summary the project
/// and candidate views show and the rows of the assessment tab. Every read runs through the
/// organisation filter of the request (TEN-01).
/// </summary>
/// <remarks>
/// The list is Configurator's (<see cref="IProjectCriteriaLists"/>, <see cref="IConfigurationReader"/>),
/// its criteria with their allowed ratings are the catalog's (<see cref="ICatalogSelectionResolver"/>,
/// resolved under the list's stored release, active or not): nothing of either is copied into the
/// workspace, so an adjusted list shows at once — a newly included criterion as „Nicht
/// eingeschätzt“, an excluded one not at all while its row stays (C51, WSP-03).
/// </remarks>
internal sealed class AssessmentReader(
    WorkspaceDbContext db,
    IProjectCriteriaLists lists,
    IConfigurationReader configurations,
    ICatalogSelectionResolver catalog)
{
    /// <summary>The criteria of the list of <paramref name="projectId"/>; <c>null</c> while it has none.</summary>
    public async Task<ProjectCriteriaScope?> ScopeOfProjectAsync(Guid projectId, CancellationToken cancellationToken) =>
        await ScopeOfAsync(await lists.FindOfProjectAsync(projectId, cancellationToken), cancellationToken);

    /// <summary>The criteria of <paramref name="list"/> for the assessments; <c>null</c> without a list.</summary>
    public async Task<ProjectCriteriaScope?> ScopeOfAsync(ProjectCriteriaList? list, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(list, cancellationToken) is not var (saved, selection))
        {
            return null;
        }

        var included = saved.Criteria.Where(c => c.IsIncluded).Select(c => c.CriterionRevisionId).ToHashSet();
        var criteria = selection.Criteria.Where(c => included.Contains(c.RevisionId)).ToList();
        return new ProjectCriteriaScope(
            saved.ReleaseVersion,
            [.. criteria.Where(c => c.Kind == CriterionKind.Requirement)],
            [.. criteria.Where(c => c.Kind == CriterionKind.TestProcedure && c.TestTypeKey is not null)],
            selection.MappedTestTypeKeys.Count > 1);
    }

    /// <summary>
    /// Every requirement criterion of the selection of <paramref name="list"/> in list order, also
    /// one the list excludes; <c>null</c> without a list. A stored assessment of an excluded
    /// criterion stays (C51, WSP-03), and the personal data export names it (S7-05). Procedure
    /// criteria are derived and have no rows (CAT-17).
    /// </summary>
    public async Task<IReadOnlyList<ResolvedCatalogCriterion>?> RequirementsOfSelectionAsync(
        ProjectCriteriaList? list, CancellationToken cancellationToken) =>
        await ResolveAsync(list, cancellationToken) is var (_, selection)
            ? [.. selection.Criteria.Where(c => c.Kind == CriterionKind.Requirement)]
            : null;

    /// <summary>The stored list and its selection under the list's release; <c>null</c> without a list.</summary>
    private async Task<(SavedConfiguration Saved, ResolvedCatalogSelection Selection)?> ResolveAsync(
        ProjectCriteriaList? list, CancellationToken cancellationToken)
    {
        if (list is null)
        {
            return null;
        }

        // Gone between the two reads: the list was replaced in this moment (no transaction spans the
        // two contexts). The next read sees the new copy.
        if (await configurations.FindAsync(list.Id, cancellationToken) is not { } saved)
        {
            return null;
        }

        var resolved = await catalog.ResolveAsync(
            saved.ReleaseVersion,
            new CatalogSelectionQuery(saved.UseCaseKey, saved.AppTypeKeys, saved.ProductPartKeys, saved.TestTypeKeys),
            cancellationToken);
        if (resolved.Selection is not { } selection)
        {
            // A stored list was checked when it was saved, and its release never changes (ADR-002).
            throw new InvalidOperationException(
                $"The selection of criteria list {list.Id} does not resolve under release {saved.ReleaseVersion}: {resolved.Outcome}.");
        }

        return (saved, selection);
    }

    /// <summary>
    /// The summaries of <paramref name="candidates"/>, all of one project with the list
    /// <paramref name="list"/>, by candidate id; <c>null</c> while the project has no list. One
    /// query for all of them.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, CandidateAssessmentSummary>?> SummariesAsync(
        ProjectCriteriaList? list, IReadOnlyCollection<AssessedCandidate> candidates, CancellationToken cancellationToken)
    {
        if (await ScopeOfAsync(list, cancellationToken) is not { } scope)
        {
            return null;
        }

        var ids = candidates.Select(c => c.Id).ToList();
        var rows = await db.CandidateAssessments.AsNoTracking()
                           .Where(a => ids.Contains(a.CandidateProductId) && a.Fulfillment != Fulfillment.Unknown)
                           .Select(a => new { a.CandidateProductId, a.CriterionRevisionId, a.Fulfillment })
                           .ToListAsync(cancellationToken);
        var valuesByCandidate = rows.ToLookup(r => r.CandidateProductId);
        return candidates.ToDictionary(
            c => c.Id,
            c => AssessmentProjection.SummaryOf(
                scope,
                valuesByCandidate[c.Id].ToDictionary(r => r.CriterionRevisionId, r => r.Fulfillment),
                c.TestStatus,
                c.EvidenceTestTypeKeys));
    }

    /// <summary>The assessment tab of the candidate, or <c>null</c> when it does not exist for the request.</summary>
    public async Task<CandidateAssessmentsResponse?> FindAsync(Guid candidateId, CancellationToken cancellationToken)
    {
        var candidate = await db.CandidateProducts.AsNoTracking()
                                .Include(c => c.TestEvidences)
                                .SingleOrDefaultAsync(c => c.Id == candidateId, cancellationToken);
        if (candidate is null)
        {
            return null;
        }

        if (await ScopeOfProjectAsync(candidate.ProcurementProjectId, cancellationToken) is not { } scope)
        {
            return new CandidateAssessmentsResponse(candidateId, null, false, new AssessmentProgressResponse(0, 0), [], []);
        }

        var rows = await db.CandidateAssessments.AsNoTracking()
                           .Where(a => a.CandidateProductId == candidateId)
                           .ToDictionaryAsync(a => a.CriterionRevisionId, cancellationToken);
        var summary = AssessmentProjection.SummaryOf(
            scope,
            rows.ToDictionary(r => r.Key, r => r.Value.Fulfillment),
            candidate.TestStatus,
            AssessedCandidate.Of(candidate).EvidenceTestTypeKeys);
        return new CandidateAssessmentsResponse(
            candidateId,
            scope.ReleaseVersion,
            scope.RatingsFromSeveralProcedures,
            summary.Progress,
            [.. scope.Requirements.Select(c => new AssessmentCriterionResponse(
                c.RevisionId,
                c.Number,
                c.Title,
                c.PlainLanguageTitle,
                c.ProductPart,
                AssessmentProjection.NamesOf(c.AllowedRatings),
                AssessmentProjection.NameOf(rows.GetValueOrDefault(c.RevisionId)?.Fulfillment ?? Fulfillment.Unknown),
                rows.GetValueOrDefault(c.RevisionId)?.EvidenceNote))],
            summary.ProcedureAnswers);
    }
}

/// <summary>What a summary needs of a candidate: its test status and the procedures it has an evidence of (CAT-17).</summary>
internal sealed record AssessedCandidate(Guid Id, TestStatus TestStatus, IReadOnlyCollection<string> EvidenceTestTypeKeys)
{
    /// <summary>Of a candidate loaded with its evidences.</summary>
    public static AssessedCandidate Of(CandidateProduct candidate) => new(
        candidate.Id,
        candidate.TestStatus,
        [.. candidate.TestEvidences.Where(e => e.TestTypeKey is not null).Select(e => e.TestTypeKey!)]);
}
