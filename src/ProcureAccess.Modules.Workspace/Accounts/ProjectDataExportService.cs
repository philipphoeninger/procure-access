// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.Workspace.Assessments;
using ProcureAccess.Modules.Workspace.Candidates;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Persistence;
using ProcureAccess.Modules.Workspace.Projects;

namespace ProcureAccess.Modules.Workspace.Accounts;

/// <inheritdoc cref="IProjectDataExport"/>
/// <remarks>
/// The organisation names come from the user's memberships (<see cref="IOrganizationMembers"/>),
/// the name and release of a project's criteria list from the Configurator
/// (<see cref="IProjectCriteriaLists"/>). Status, test status and test kind are spelled as the
/// workspace API spells them. The assessments (S7-05, owner decision O-S7-1) are the stored rows
/// of a candidate, named by number and title of the criterion under the release of the project's
/// list (<see cref="AssessmentReader"/>), as the exported criteria lists name a criterion.
/// </remarks>
internal sealed class ProjectDataExportService(
    WorkspaceDbContext db, IOrganizationMembers organizations, IProjectCriteriaLists criteriaLists, AssessmentReader assessments)
    : IProjectDataExport
{
    public async Task<IReadOnlyList<ExportedProject>> GetCreatedByAsync(string userId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        // Through the organisation filter of the request (TEN-01): the projects of the
        // organisations the signed-in user still belongs to, as the criteria lists.
        var projects = await db.ProcurementProjects.AsNoTracking()
                               .Where(p => p.CreatedByUserId == userId)
                               .OrderBy(p => p.CreatedAt).ThenBy(p => p.Id)
                               .ToListAsync(cancellationToken);
        if (projects.Count == 0)
        {
            return [];
        }

        var projectIds = projects.Select(p => p.Id).ToList();
        var candidates = await db.CandidateProducts.AsNoTracking()
                                 .Include(c => c.TestEvidences)
                                 .Where(c => projectIds.Contains(c.ProcurementProjectId))
                                 .OrderBy(c => c.CreatedAt).ThenBy(c => c.Id)
                                 .ToListAsync(cancellationToken);
        var candidateIds = candidates.Select(c => c.Id).ToList();
        var evidenceIds = candidates.SelectMany(c => c.TestEvidences).Select(e => e.Id).ToList();

        // The private rows have no navigation from their candidate or evidence (S6-15), so they
        // are read by key.
        var privateSections = await db.CandidatePrivateSections.AsNoTracking()
                                      .Where(s => candidateIds.Contains(s.CandidateProductId))
                                      .ToDictionaryAsync(s => s.CandidateProductId, cancellationToken);
        var privateNotes = await db.TestEvidencePrivateNotes.AsNoTracking()
                                   .Where(n => evidenceIds.Contains(n.TestEvidenceId))
                                   .ToDictionaryAsync(n => n.TestEvidenceId, n => n.QualificationNote, cancellationToken);
        var assessmentsOfCandidate = (await db.CandidateAssessments.AsNoTracking()
                                              .Where(a => candidateIds.Contains(a.CandidateProductId))
                                              .ToListAsync(cancellationToken))
                                     .ToLookup(a => a.CandidateProductId);

        var organizationNames = (await organizations.ListOfAsync(userId, cancellationToken)).ToDictionary(o => o.Id, o => o.Name);
        var candidatesOfProject = candidates.ToLookup(c => c.ProcurementProjectId);
        var exported = new List<ExportedProject>(projects.Count);
        foreach (var project in projects)
        {
            // A membership removed while this request runs: the project is no longer the user's to read.
            if (!organizationNames.TryGetValue(project.OrganizationId, out var organization))
            {
                continue;
            }

            // One read per project: the contract answers one project, and a user creates few.
            var list = await criteriaLists.FindOfProjectAsync(project.Id, cancellationToken);

            // The catalog is asked only for a project with an assessment. Without the list no
            // criterion can be named: it was replaced in this moment, which removes the rows that
            // are left (a project with a value keeps its list, WSP-02).
            var assessed = candidatesOfProject[project.Id].Any(c => assessmentsOfCandidate[c.Id].Any());
            var criteria = assessed ? await assessments.RequirementsOfSelectionAsync(list, cancellationToken) ?? [] : [];
            exported.Add(new ExportedProject(
                project.Name,
                organization,
                ProjectService.NameOf(project.Status),
                project.CreatedAt,
                list is null ? null : new ExportedProjectCriteriaList(list.Name, list.ReleaseVersion),
                [.. candidatesOfProject[project.Id].Select(c => CandidateOf(
                    c, privateSections.GetValueOrDefault(c.Id), privateNotes, AssessmentsOf(criteria, assessmentsOfCandidate[c.Id])))]));
        }

        return exported;
    }

    private static ExportedCandidate CandidateOf(
        CandidateProduct candidate,
        CandidatePrivateSection? privateSection,
        Dictionary<Guid, string?> privateNotes,
        IReadOnlyList<ExportedAssessment> assessments) => new(
        candidate.Name,
        candidate.VendorName,
        candidate.WebsiteUrl,
        candidate.FunctionalSummary,
        CandidateNames.Of(candidate.TestStatus),
        privateSection?.ContactNotes,
        privateSection?.InternalNotes,
        candidate.CreatedAt,
        [.. candidate.TestEvidences.OrderBy(e => e.CreatedAt).ThenBy(e => e.Id).Select(e => new ExportedTestEvidence(
            CandidateNames.Of(e.TestKind),
            e.TestTypeKey,
            e.OtherTestTypeName,
            e.TestedByName,
            e.TestDate,
            e.TestedVersion,
            e.EvidenceUrl,
            privateNotes.GetValueOrDefault(e.Id)))],
        assessments);

    /// <summary>
    /// The stored rows of one candidate in the order of <paramref name="criteria"/>, the requirement
    /// criteria of the project's list: a criterion without a row has no entry, and nothing is
    /// counted (SIG-01).
    /// </summary>
    private static IReadOnlyList<ExportedAssessment> AssessmentsOf(
        IReadOnlyList<ResolvedCatalogCriterion> criteria, IEnumerable<CandidateAssessment> rows)
    {
        var rowOfCriterion = rows.ToDictionary(r => r.CriterionRevisionId);
        return [.. criteria.Where(c => rowOfCriterion.ContainsKey(c.RevisionId)).Select(c => new ExportedAssessment(
            c.Number,
            c.PlainLanguageTitle ?? c.Title,
            AssessmentProjection.NameOf(rowOfCriterion[c.RevisionId].Fulfillment),
            rowOfCriterion[c.RevisionId].EvidenceNote))];
    }
}
