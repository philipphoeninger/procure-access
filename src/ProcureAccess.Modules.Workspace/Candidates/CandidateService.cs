// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Platform.Contracts.Concurrency;
using ProcureAccess.Modules.Workspace.Assessments;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Persistence;

namespace ProcureAccess.Modules.Workspace.Candidates;

/// <summary>
/// Creates, reads and changes the candidate products of a project with their private section, test
/// status and test evidences (SPEC R-CAN-02, R-CAN-05; WSP-01; S6-15) for
/// <see cref="CandidateEndpoints"/>. Every read runs through the organisation filter of the request
/// (TEN-01); the endpoints check the role on the organisation before they call it.
/// </summary>
/// <remarks>
/// <para>Every change loads the candidate with its evidences and its project, changes them and
/// saves once. A change of an archived project's candidate is refused (R-PRJ-05). A change moves
/// the last change of the candidate and of its project (owner decision S6-15 5 (a)).</para>
/// <para><b>Concurrency (API-03; S6-20).</b> A change of a candidate or an evidence names the
/// version its sender has seen (<see cref="EditPrecondition"/>); a stale one is refused before
/// anything else is checked. What counts a new version:
/// <list type="bullet">
/// <item>of the candidate — its facts, its private section, its test status, and an evidence added
/// or removed: the set of evidences belongs to the test status (WSP-01), so a status change never
/// meets an evidence its sender has not seen;</item>
/// <item>of the evidence — its fields and its private note; the candidate keeps its version.</item>
/// </list>
/// The project keeps its version through all of them; it is only stamped. A save that met a
/// concurrent one runs again (<see cref="ConcurrentChange"/>).</para>
/// <para>The private rows are read and written beside the candidate, never through it.</para>
/// </remarks>
internal sealed class CandidateService(
    WorkspaceDbContext db, IProjectCriteriaLists criteriaLists, AssessmentReader assessments, EntityTags tags, TimeProvider clock)
{
    /// <summary>The organisation of the candidate, or <c>null</c> when it does not exist for the request (TEN-01).</summary>
    public async Task<Guid?> OrganizationOfCandidateAsync(Guid id, CancellationToken cancellationToken) =>
        await db.CandidateProducts.AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => (Guid?)c.OrganizationId)
                .SingleOrDefaultAsync(cancellationToken);

    /// <summary>The organisation of the evidence, or <c>null</c> when it does not exist for the request (TEN-01).</summary>
    public async Task<Guid?> OrganizationOfEvidenceAsync(Guid id, CancellationToken cancellationToken) =>
        await db.TestEvidences.AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => (Guid?)e.OrganizationId)
                .SingleOrDefaultAsync(cancellationToken);

    /// <summary>The candidate with its private section and evidences, or <c>null</c> (TEN-01).</summary>
    public async Task<CandidateResponse?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var candidate = await db.CandidateProducts.AsNoTracking()
                                .Include(c => c.TestEvidences)
                                .SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        return candidate is null ? null : await ResponseOfAsync(candidate, cancellationToken);
    }

    /// <summary>Stores a new candidate of the project with the status „Nicht bekannt“ (SIG-03).</summary>
    /// <param name="input">Checked; the name is set, empty optional text is <c>""</c> or <c>null</c>.</param>
    public Task<CandidateResult<CandidateResponse>> CreateAsync(
        Guid projectId, CandidateEdit input, string userId, CancellationToken cancellationToken) =>
        ConcurrentChange.RunAsync(db, () => CreateOnceAsync(projectId, input, userId, cancellationToken));

    private async Task<CandidateResult<CandidateResponse>> CreateOnceAsync(
        Guid projectId, CandidateEdit input, string userId, CancellationToken cancellationToken)
    {
        var project = await db.ProcurementProjects.SingleOrDefaultAsync(p => p.Id == projectId, cancellationToken);
        if (project is null)
        {
            return CandidateResult<CandidateResponse>.Of(CandidateOutcome.NotFound);
        }

        if (project.IsArchived)
        {
            return CandidateResult<CandidateResponse>.Of(CandidateOutcome.ProjectArchived);
        }

        var now = clock.GetUtcNow();
        var facts = new CandidateFacts(input.Name!, Cleared(input.Vendor), Cleared(input.Website), Cleared(input.FunctionalSummary));
        var candidate = CandidateProduct.Create(project, facts, now, userId);
        db.CandidateProducts.Add(candidate);
        db.CandidatePrivateSections.Add(
            CandidatePrivateSection.Of(candidate, Cleared(input.ContactNotes), Cleared(input.InternalNotes)));
        project.MarkChanged(now);
        await db.SaveChangesAsync(cancellationToken);
        return CandidateResult<CandidateResponse>.Done(await ResponseOfAsync(candidate, cancellationToken));
    }

    /// <summary>Changes what <paramref name="input"/> names; the same values change nothing.</summary>
    public Task<CandidateResult<CandidateResponse>> ChangeAsync(
        Guid id, CandidateEdit input, EditPrecondition edit, CancellationToken cancellationToken) =>
        ConcurrentChange.RunAsync(db, () => ChangeOnceAsync(id, input, edit, cancellationToken));

    private async Task<CandidateResult<CandidateResponse>> ChangeOnceAsync(
        Guid id, CandidateEdit input, EditPrecondition edit, CancellationToken cancellationToken)
    {
        if (await LoadAsync(id, cancellationToken) is not { } loaded)
        {
            return CandidateResult<CandidateResponse>.Of(CandidateOutcome.NotFound);
        }

        if (!tags.Matches(edit.IfMatch, loaded.Candidate.Version))
        {
            return CandidateResult<CandidateResponse>.Conflict(loaded.Candidate.ModifiedByUserId);
        }

        if (loaded.Project.IsArchived)
        {
            return CandidateResult<CandidateResponse>.Of(CandidateOutcome.ProjectArchived);
        }

        var (candidate, project) = loaded;
        var now = clock.GetUtcNow();
        var current = candidate.Facts;
        var facts = new CandidateFacts(
            input.Name ?? current.Name,
            Merged(input.Vendor, current.VendorName),
            Merged(input.Website, current.WebsiteUrl),
            Merged(input.FunctionalSummary, current.FunctionalSummary));
        var section = await db.CandidatePrivateSections.SingleAsync(s => s.CandidateProductId == id, cancellationToken);
        var factsChanged = candidate.ChangeFacts(facts, now);
        var sectionChanged = section.Change(
            Merged(input.ContactNotes, section.ContactNotes), Merged(input.InternalNotes, section.InternalNotes));
        if (factsChanged || sectionChanged)
        {
            candidate.MarkChanged(now);
            candidate.MarkEdited(edit.UserId);
            project.MarkChanged(now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return CandidateResult<CandidateResponse>.Done(await ResponseOfAsync(candidate, cancellationToken));
    }

    /// <summary>
    /// Changes the test status (WSP-01): leaving „Ja“ with evidences is refused unless
    /// <paramref name="removeEvidences"/> asks to remove them in the same save.
    /// </summary>
    public Task<CandidateResult<CandidateResponse>> ChangeTestStatusAsync(
        Guid id, TestStatus status, bool removeEvidences, EditPrecondition edit, CancellationToken cancellationToken) =>
        ConcurrentChange.RunAsync(db, () => ChangeTestStatusOnceAsync(id, status, removeEvidences, edit, cancellationToken));

    private async Task<CandidateResult<CandidateResponse>> ChangeTestStatusOnceAsync(
        Guid id, TestStatus status, bool removeEvidences, EditPrecondition edit, CancellationToken cancellationToken)
    {
        if (await LoadAsync(id, cancellationToken) is not { } loaded)
        {
            return CandidateResult<CandidateResponse>.Of(CandidateOutcome.NotFound);
        }

        var (candidate, project) = loaded;
        if (!tags.Matches(edit.IfMatch, candidate.Version))
        {
            return CandidateResult<CandidateResponse>.Conflict(candidate.ModifiedByUserId);
        }

        if (project.IsArchived)
        {
            return CandidateResult<CandidateResponse>.Of(CandidateOutcome.ProjectArchived);
        }

        var now = clock.GetUtcNow();
        var before = candidate.TestStatus;
        if (candidate.ChangeTestStatus(status, removeEvidences, now) == TestStatusChangeOutcome.EvidencesExist)
        {
            return CandidateResult<CandidateResponse>.Of(CandidateOutcome.TestStatusConflict, candidate.TestEvidences.Count);
        }

        if (candidate.TestStatus != before)
        {
            candidate.MarkEdited(edit.UserId);
            project.MarkChanged(now);
        }

        // The removed evidences are orphans of a required relationship, so EF deletes them; the
        // database deletes their private notes (cascade).
        await db.SaveChangesAsync(cancellationToken);
        return CandidateResult<CandidateResponse>.Done(await ResponseOfAsync(candidate, cancellationToken));
    }

    /// <summary>Adds an evidence with its private note (WSP-01).</summary>
    public Task<CandidateResult<TestEvidenceResponse>> AddEvidenceAsync(
        Guid candidateId, EvidenceInput input, string userId, CancellationToken cancellationToken) =>
        ConcurrentChange.RunAsync(db, () => AddEvidenceOnceAsync(candidateId, input, userId, cancellationToken));

    private async Task<CandidateResult<TestEvidenceResponse>> AddEvidenceOnceAsync(
        Guid candidateId, EvidenceInput input, string userId, CancellationToken cancellationToken)
    {
        if (await LoadAsync(candidateId, cancellationToken) is not { } loaded)
        {
            return CandidateResult<TestEvidenceResponse>.Of(CandidateOutcome.NotFound);
        }

        var (candidate, project) = loaded;
        if (project.IsArchived)
        {
            return CandidateResult<TestEvidenceResponse>.Of(CandidateOutcome.ProjectArchived);
        }

        var now = clock.GetUtcNow();
        var change = candidate.AddEvidence(input.Facts, now, userId);
        if (Refused<TestEvidenceResponse>(change, candidate) is { } refused)
        {
            return refused;
        }

        var evidence = change.Evidence!;
        var note = TestEvidencePrivateNote.Of(evidence, input.QualificationNote);
        db.TestEvidencePrivateNotes.Add(note);
        candidate.MarkEdited(userId);
        project.MarkChanged(now);
        await db.SaveChangesAsync(cancellationToken);
        return CandidateResult<TestEvidenceResponse>.Done(ResponseOf(evidence, note));
    }

    /// <summary>
    /// Replaces the fields of an evidence and its private note; a new product version keeps the row
    /// (C65). The same values change nothing.
    /// </summary>
    public Task<CandidateResult<TestEvidenceResponse>> ChangeEvidenceAsync(
        Guid evidenceId, EvidenceInput input, EditPrecondition edit, CancellationToken cancellationToken) =>
        ConcurrentChange.RunAsync(db, () => ChangeEvidenceOnceAsync(evidenceId, input, edit, cancellationToken));

    private async Task<CandidateResult<TestEvidenceResponse>> ChangeEvidenceOnceAsync(
        Guid evidenceId, EvidenceInput input, EditPrecondition edit, CancellationToken cancellationToken)
    {
        if (await LoadByEvidenceAsync(evidenceId, cancellationToken) is not { } loaded)
        {
            return CandidateResult<TestEvidenceResponse>.Of(CandidateOutcome.NotFound);
        }

        var (candidate, project, evidence) = loaded;
        if (!tags.Matches(edit.IfMatch, evidence.Version))
        {
            return CandidateResult<TestEvidenceResponse>.Conflict(evidence.ModifiedByUserId);
        }

        if (project.IsArchived)
        {
            return CandidateResult<TestEvidenceResponse>.Of(CandidateOutcome.ProjectArchived);
        }

        var now = clock.GetUtcNow();
        // Asked before the change: the time of the last change says nothing when two changes share an instant.
        var factsChange = !evidence.Holds(input.Facts);
        var change = candidate.ChangeEvidence(evidenceId, input.Facts, now);
        if (Refused<TestEvidenceResponse>(change, candidate) is { } refused)
        {
            return refused;
        }

        var note = await db.TestEvidencePrivateNotes.SingleAsync(n => n.TestEvidenceId == evidenceId, cancellationToken);
        if (note.Change(input.QualificationNote) || factsChange)
        {
            evidence.MarkEdited(edit.UserId);
            candidate.MarkChanged(now);
            project.MarkChanged(now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return CandidateResult<TestEvidenceResponse>.Done(ResponseOf(change.Evidence!, note));
    }

    /// <summary>Removes an evidence; the database removes its private note.</summary>
    public Task<CandidateResult<TestEvidenceResponse>> RemoveEvidenceAsync(
        Guid evidenceId, EditPrecondition edit, CancellationToken cancellationToken) =>
        ConcurrentChange.RunAsync(db, () => RemoveEvidenceOnceAsync(evidenceId, edit, cancellationToken));

    /// <returns>No value: <see cref="CandidateOutcome.Done"/> says the evidence is gone.</returns>
    private async Task<CandidateResult<TestEvidenceResponse>> RemoveEvidenceOnceAsync(
        Guid evidenceId, EditPrecondition edit, CancellationToken cancellationToken)
    {
        if (await LoadByEvidenceAsync(evidenceId, cancellationToken) is not { } loaded)
        {
            return CandidateResult<TestEvidenceResponse>.Of(CandidateOutcome.NotFound);
        }

        var (candidate, project, evidence) = loaded;
        if (!tags.Matches(edit.IfMatch, evidence.Version))
        {
            return CandidateResult<TestEvidenceResponse>.Conflict(evidence.ModifiedByUserId);
        }

        if (project.IsArchived)
        {
            return CandidateResult<TestEvidenceResponse>.Of(CandidateOutcome.ProjectArchived);
        }

        var now = clock.GetUtcNow();
        candidate.RemoveEvidence(evidenceId, now);
        candidate.MarkEdited(edit.UserId);
        project.MarkChanged(now);
        await db.SaveChangesAsync(cancellationToken);
        return CandidateResult<TestEvidenceResponse>.Of(CandidateOutcome.Done);
    }

    /// <summary>A change's optional text: <c>null</c> keeps <paramref name="current"/>, <c>""</c> clears it.</summary>
    private static string? Merged(string? named, string? current) => named is null ? current : Cleared(named);

    private static string? Cleared(string? value) => string.IsNullOrEmpty(value) ? null : value;

    private static CandidateResult<T>? Refused<T>(EvidenceChange change, CandidateProduct candidate) where T : class =>
        change.Outcome switch
        {
            EvidenceChangeOutcome.Done => null,
            EvidenceChangeOutcome.NotTested => CandidateResult<T>.Of(CandidateOutcome.TestStatusConflict, candidate.TestEvidences.Count),
            EvidenceChangeOutcome.DuplicateTestType => CandidateResult<T>.Of(CandidateOutcome.DuplicateTestType),
            _ => throw new InvalidOperationException($"Unknown outcome {change.Outcome} of an evidence change."),
        };

    private async Task<(CandidateProduct Candidate, ProcurementProject Project)?> LoadAsync(Guid id, CancellationToken cancellationToken)
    {
        var candidate = await db.CandidateProducts.Include(c => c.TestEvidences).SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (candidate is null)
        {
            return null;
        }

        // The same organisation as the candidate, so the filter shows it.
        var project = await db.ProcurementProjects.SingleAsync(p => p.Id == candidate.ProcurementProjectId, cancellationToken);
        return (candidate, project);
    }

    private async Task<(CandidateProduct Candidate, ProcurementProject Project, TestEvidence Evidence)?> LoadByEvidenceAsync(
        Guid evidenceId, CancellationToken cancellationToken)
    {
        var candidateId = await db.TestEvidences.AsNoTracking()
                                  .Where(e => e.Id == evidenceId)
                                  .Select(e => (Guid?)e.CandidateProductId)
                                  .SingleOrDefaultAsync(cancellationToken);
        if (candidateId is not { } id || await LoadAsync(id, cancellationToken) is not { } loaded)
        {
            return null;
        }

        // Gone between the two reads when another request removed it.
        return loaded.Candidate.TestEvidences.SingleOrDefault(e => e.Id == evidenceId) is { } evidence
            ? (loaded.Candidate, loaded.Project, evidence)
            : null;
    }

    private async Task<CandidateResponse> ResponseOfAsync(CandidateProduct candidate, CancellationToken cancellationToken)
    {
        var section = await db.CandidatePrivateSections.AsNoTracking()
                              .SingleAsync(s => s.CandidateProductId == candidate.Id, cancellationToken);
        var evidenceIds = candidate.TestEvidences.Select(e => e.Id).ToList();
        var notes = await db.TestEvidencePrivateNotes.AsNoTracking()
                            .Where(n => evidenceIds.Contains(n.TestEvidenceId))
                            .ToDictionaryAsync(n => n.TestEvidenceId, cancellationToken);

        // Progress and the seven counts against the project's list (C34; S6-18); null without one.
        var list = await criteriaLists.FindOfProjectAsync(candidate.ProcurementProjectId, cancellationToken);
        var summary = (await assessments.SummariesAsync(list, [AssessedCandidate.Of(candidate)], cancellationToken))?[candidate.Id];
        return new CandidateResponse(
            candidate.Id,
            candidate.ProcurementProjectId,
            candidate.OrganizationId,
            candidate.Name,
            candidate.VendorName,
            candidate.WebsiteUrl,
            candidate.FunctionalSummary,
            CandidateNames.Of(candidate.TestStatus),
            new CandidatePrivateSectionResponse(section.ContactNotes, section.InternalNotes),
            [.. candidate.TestEvidences.OrderBy(e => e.CreatedAt).ThenBy(e => e.Id).Select(e => ResponseOf(e, notes[e.Id]))],
            candidate.CreatedAt,
            candidate.ModifiedAt,
            summary?.Progress,
            summary?.Fulfillment,
            tags.Of(candidate.Version));
    }

    private TestEvidenceResponse ResponseOf(TestEvidence evidence, TestEvidencePrivateNote note) => new(
        evidence.Id,
        CandidateNames.Of(evidence.TestKind),
        evidence.TestTypeKey,
        evidence.OtherTestTypeName,
        evidence.TestedByName,
        evidence.TestDate,
        evidence.TestedVersion,
        evidence.EvidenceUrl,
        note.QualificationNote,
        tags.Of(evidence.Version));
}

/// <summary>
/// A checked change of a candidate: what is <c>null</c> stays, <c>""</c> clears an optional field,
/// any other value is trimmed and within its limit. For a new candidate, <see cref="Name"/> is set.
/// </summary>
internal sealed record CandidateEdit(
    string? Name,
    string? Vendor,
    string? Website,
    string? FunctionalSummary,
    string? ContactNotes,
    string? InternalNotes);

/// <summary>A checked evidence with its private note (<c>null</c> for none).</summary>
internal sealed record EvidenceInput(TestEvidenceFacts Facts, string? QualificationNote);

/// <summary>How a candidate or evidence request ended.</summary>
internal enum CandidateOutcome
{
    Done,

    /// <summary>Unknown, or of another organisation (TEN-01).</summary>
    NotFound,

    /// <summary>The project is archived (409 <c>project-archived</c>, R-PRJ-05).</summary>
    ProjectArchived,

    /// <summary>Test status and evidences do not fit (409 <c>test-status-conflict</c>, WSP-01).</summary>
    TestStatusConflict,

    /// <summary>The catalog procedure already has an evidence of the candidate (400 at <c>testTypeId</c>, WSP-01).</summary>
    DuplicateTestType,

    /// <summary>The <c>If-Match</c> is not the tag of the stored version (409 <c>conflict</c>, API-03).</summary>
    Conflict,
}

/// <param name="Value">Only with <see cref="CandidateOutcome.Done"/>.</param>
/// <param name="EvidenceCount">The candidate's evidences, for <see cref="CandidateOutcome.TestStatusConflict"/>.</param>
/// <param name="EditorUserId">Who made the stored version, for <see cref="CandidateOutcome.Conflict"/>.</param>
internal sealed record CandidateResult<T>(CandidateOutcome Outcome, T? Value, int EvidenceCount, string? EditorUserId = null)
    where T : class
{
    public static CandidateResult<T> Done(T value) => new(CandidateOutcome.Done, value, 0);

    public static CandidateResult<T> Conflict(string? editorUserId) => new(CandidateOutcome.Conflict, null, 0, editorUserId);

    public static CandidateResult<T> Of(CandidateOutcome outcome, int evidenceCount = 0) => new(outcome, null, evidenceCount);
}
