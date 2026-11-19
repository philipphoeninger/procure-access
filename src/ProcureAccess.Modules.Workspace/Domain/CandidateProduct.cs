// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Workspace.Domain;

/// <summary>
/// A product a procurement project considers (ARCHITECTURE §5.4, C46, C65; SPEC R-CAN-02, R-CAN-05;
/// S6-15): name, vendor, website, functional summary, test status and its test evidences. It belongs
/// to one project and through it to the project's organisation.
/// </summary>
/// <remarks>
/// <para><b>Aggregate.</b> The candidate holds its <see cref="TestEvidences"/> and alone keeps the
/// rules of WSP-01: evidences only while the status is „Ja“, at most one per catalog procedure,
/// leaving „Ja“ only after the evidences are gone.</para>
/// <para><b>Private section.</b> Contacts and internal notes are not members of this type: they
/// are the row <see cref="CandidatePrivateSection"/> beside it (S6-15), so a copy of a candidate
/// into the directory (Flow B, step 6) cannot carry them (PRV-01, PRV-02).</para>
/// </remarks>
internal sealed class CandidateProduct
{
    /// <summary>Longest name and vendor (SPEC §7.2).</summary>
    public const int NameMaxLength = 200;

    /// <summary>Longest website (SPEC §7.2).</summary>
    public const int WebsiteUrlMaxLength = 2048;

    /// <summary>Longest functional summary (SPEC §7.2).</summary>
    public const int FunctionalSummaryMaxLength = 2000;

    private readonly List<TestEvidence> _testEvidences = [];

    private CandidateProduct()
    {
    }

    /// <summary>Random, so the identifier says nothing about other candidates or their order.</summary>
    public Guid Id { get; private set; }

    /// <summary>The organisation of the project; never changes, for the organisation filter (TEN-01).</summary>
    public Guid OrganizationId { get; private set; }

    public Guid ProcurementProjectId { get; private set; }

    /// <summary>
    /// The product of the directory this candidate is linked to. Always <c>null</c> until „Merken“
    /// and the link come with the directory (R-CAN-01, M6; owner decision S6-15 1 (a)); stored now
    /// so that step needs no workspace migration for it. No foreign key: the directory is another
    /// context (ADR-014).
    /// </summary>
    public Guid? SoftwareProductId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? VendorName { get; private set; }

    public string? WebsiteUrl { get; private set; }

    public string? FunctionalSummary { get; private set; }

    public TestStatus TestStatus { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>When the facts, the test status or an evidence last changed; the creation time until then.</summary>
    public DateTimeOffset ModifiedAt { get; private set; }

    /// <summary>
    /// Counts the changes of the facts, the private section, the test status and the set of evidences (<see cref="MarkEdited"/>); the concurrency
    /// token and, with the 🔧 epoch, the <c>ETag</c> (API-03, API-04; S6-20). A changed field of
    /// an evidence counts on the evidence, an assessment nowhere (R-CAN-11): both only move
    /// <see cref="ModifiedAt"/>.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// The user id (IdentityAccess) of who made <see cref="Version"/>, the competing editor of a 409
    /// <c>conflict</c>; <c>null</c> for a row older than S6-20 or made without a user. No foreign
    /// key across contexts (ADR-014).
    /// </summary>
    public string? ModifiedByUserId { get; private set; }

    /// <summary>The test evidences, in the order they were added.</summary>
    public IReadOnlyList<TestEvidence> TestEvidences => _testEvidences;

    /// <summary>
    /// Creates a candidate of <paramref name="project"/> with the status „Nicht bekannt“ and no
    /// evidence. Throws for an archived project (R-PRJ-05) and for facts outside their limits.
    /// </summary>
    /// <param name="createdByUserId">The first <see cref="ModifiedByUserId"/>; <c>null</c> when no user is known.</param>
    public static CandidateProduct Create(
        ProcurementProject project, CandidateFacts facts, DateTimeOffset createdAt, string? createdByUserId = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        CheckOpen(project);
        facts.Check();
        return new CandidateProduct
        {
            Id = Guid.NewGuid(),
            OrganizationId = project.OrganizationId,
            ProcurementProjectId = project.Id,
            Name = facts.Name,
            VendorName = facts.VendorName,
            WebsiteUrl = facts.WebsiteUrl,
            FunctionalSummary = facts.FunctionalSummary,
            TestStatus = TestStatus.Unknown,
            CreatedAt = createdAt,
            ModifiedAt = createdAt,
            Version = 1,
            ModifiedByUserId = createdByUserId,
        };
    }

    /// <summary>The facts as they are, for a change that names only some of them.</summary>
    public CandidateFacts Facts => new(Name, VendorName, WebsiteUrl, FunctionalSummary);

    /// <summary>Replaces the facts. The same facts change nothing; returns whether they changed.</summary>
    public bool ChangeFacts(CandidateFacts facts, DateTimeOffset now)
    {
        facts.Check();
        if (facts == Facts)
        {
            return false;
        }

        (Name, VendorName, WebsiteUrl, FunctionalSummary) = (facts.Name, facts.VendorName, facts.WebsiteUrl, facts.FunctionalSummary);
        ModifiedAt = now;
        return true;
    }

    /// <summary>
    /// Changes the test status (WSP-01). Leaving „Ja“ while evidences exist is refused unless
    /// <paramref name="removeEvidences"/> asks to remove them with the change (the database
    /// removes their private notes).
    /// </summary>
    public TestStatusChangeOutcome ChangeTestStatus(TestStatus status, bool removeEvidences, DateTimeOffset now)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown test status.");
        }

        if (status == TestStatus)
        {
            return TestStatusChangeOutcome.Done;
        }

        if (status != TestStatus.Tested && _testEvidences.Count > 0 && !removeEvidences)
        {
            return TestStatusChangeOutcome.EvidencesExist;
        }

        _testEvidences.Clear();
        TestStatus = status;
        ModifiedAt = now;
        return TestStatusChangeOutcome.Done;
    }

    /// <summary>
    /// Adds an evidence (WSP-01): refused while the status is not „Ja“ and for a catalog procedure
    /// that already has one.
    /// </summary>
    /// <param name="createdByUserId">The first editor of the evidence; <c>null</c> when no user is known.</param>
    public EvidenceChange AddEvidence(TestEvidenceFacts facts, DateTimeOffset now, string? createdByUserId = null)
    {
        ArgumentNullException.ThrowIfNull(facts);
        if (Refusal(facts, except: null) is { } refused)
        {
            return new EvidenceChange(refused, null);
        }

        var evidence = TestEvidence.Create(this, facts, now, createdByUserId);
        _testEvidences.Add(evidence);
        ModifiedAt = now;
        return new EvidenceChange(EvidenceChangeOutcome.Done, evidence);
    }

    /// <summary>
    /// Replaces the fields of one evidence — the same row for a new product version (C65). Refused
    /// as <see cref="AddEvidence"/> is; the same fields change nothing.
    /// </summary>
    public EvidenceChange ChangeEvidence(Guid evidenceId, TestEvidenceFacts facts, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(facts);
        var evidence = _testEvidences.SingleOrDefault(e => e.Id == evidenceId)
                       ?? throw new ArgumentException("Not an evidence of this candidate.", nameof(evidenceId));
        if (Refusal(facts, except: evidence) is { } refused)
        {
            return new EvidenceChange(refused, null);
        }

        if (!evidence.Holds(facts))
        {
            evidence.Apply(facts);
            ModifiedAt = now;
        }

        return new EvidenceChange(EvidenceChangeOutcome.Done, evidence);
    }

    /// <summary>
    /// Counts a change of the facts, the private section, the test status or the set of evidences by <paramref name="userId"/>: a new
    /// <see cref="Version"/>, so every <c>ETag</c> issued before is stale (API-03).
    /// </summary>
    public void MarkEdited(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        Version++;
        ModifiedByUserId = userId;
    }

    /// <summary>
    /// Records a change of what belongs to the candidate outside this type — its private section,
    /// the private note of an evidence or an assessment — as its last change. The
    /// <see cref="Version"/> stays.
    /// </summary>
    public void MarkChanged(DateTimeOffset now) => ModifiedAt = now;

    /// <summary>Removes one evidence (the database removes its private note).</summary>
    public void RemoveEvidence(Guid evidenceId, DateTimeOffset now)
    {
        var evidence = _testEvidences.SingleOrDefault(e => e.Id == evidenceId)
                       ?? throw new ArgumentException("Not an evidence of this candidate.", nameof(evidenceId));
        _testEvidences.Remove(evidence);
        ModifiedAt = now;
    }

    /// <summary>Throws for an archived project: nothing of it changes but its status (R-PRJ-05).</summary>
    private static void CheckOpen(ProcurementProject project)
    {
        if (project.IsArchived)
        {
            throw new InvalidOperationException("The candidates of an archived project cannot change.");
        }
    }

    private EvidenceChangeOutcome? Refusal(TestEvidenceFacts facts, TestEvidence? except)
    {
        facts.Check();
        if (TestStatus != TestStatus.Tested)
        {
            return EvidenceChangeOutcome.NotTested;
        }

        var duplicate = facts.Kind == TestKind.Procedure
                        && _testEvidences.Any(e => e != except && e.TestKind == TestKind.Procedure && e.TestTypeKey == facts.TestTypeKey);
        return duplicate ? EvidenceChangeOutcome.DuplicateTestType : null;
    }
}

/// <summary>
/// The facts of a candidate, checked by the endpoint before (trimmed, empty optional text as
/// <c>null</c>, the website an <c>https</c> address). Only the name is required (owner decision
/// S6-15 2 (a)).
/// </summary>
internal sealed record CandidateFacts(string Name, string? VendorName, string? WebsiteUrl, string? FunctionalSummary)
{
    /// <summary>Throws for a missing name and for a field above its limit.</summary>
    public void Check()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        CheckLength(Name, CandidateProduct.NameMaxLength, nameof(Name));
        CheckLength(VendorName, CandidateProduct.NameMaxLength, nameof(VendorName));
        CheckLength(WebsiteUrl, CandidateProduct.WebsiteUrlMaxLength, nameof(WebsiteUrl));
        CheckLength(FunctionalSummary, CandidateProduct.FunctionalSummaryMaxLength, nameof(FunctionalSummary));
    }

    private static void CheckLength(string? value, int maxLength, string name)
    {
        if (value?.Length > maxLength)
        {
            throw new ArgumentException($"At most {maxLength} characters.", name);
        }
    }
}

/// <summary>How a change of the test status ended.</summary>
internal enum TestStatusChangeOutcome
{
    Done,

    /// <summary>The status is to leave „Ja“ while evidences exist and their removal was not asked for (409 <c>test-status-conflict</c>).</summary>
    EvidencesExist,
}

/// <summary>How adding or changing an evidence ended.</summary>
internal enum EvidenceChangeOutcome
{
    Done,

    /// <summary>The status is not „Ja“ (409 <c>test-status-conflict</c>).</summary>
    NotTested,

    /// <summary>The catalog procedure already has an evidence of this candidate (400 at <c>testTypeId</c>).</summary>
    DuplicateTestType,
}

/// <param name="Evidence">The added or changed evidence; only with <see cref="EvidenceChangeOutcome.Done"/>.</param>
internal sealed record EvidenceChange(EvidenceChangeOutcome Outcome, TestEvidence? Evidence);
