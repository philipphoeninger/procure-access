// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Workspace.Domain;

/// <summary>
/// A test evidence of a candidate product (ARCHITECTURE §5.4, C46, C65; SPEC R-CAN-05; WSP-01):
/// which test, by whom, when, of which version, with an optional link to the report. Part of the
/// <see cref="CandidateProduct"/> aggregate, which alone creates, changes and removes it. 0..n per
/// candidate, only while its status is „Ja“, at most one per catalog procedure.
/// </summary>
/// <remarks>
/// The private note on the qualification of the testers is not a member of this type: it is the
/// row <see cref="TestEvidencePrivateNote"/> beside it (owner decision S6-15 4 (a)), so a copy of an
/// evidence into the directory (Flow B, step 6) cannot carry it (PRV-01, PRV-02).
/// </remarks>
internal sealed class TestEvidence
{
    /// <summary>Longest name of „Andere Prüfung“ (SPEC §7.2).</summary>
    public const int OtherTestTypeNameMaxLength = 120;

    /// <summary>Longest „Wer hat geprüft?“ (SPEC §7.2).</summary>
    public const int TestedByNameMaxLength = 200;

    /// <summary>Longest „Geprüfte Version“ (SPEC §7.2).</summary>
    public const int TestedVersionMaxLength = 200;

    /// <summary>Longest link to the report (SPEC §7.2).</summary>
    public const int EvidenceUrlMaxLength = 2048;

    private TestEvidence()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>The organisation of the candidate's project, for the organisation filter (TEN-01).</summary>
    public Guid OrganizationId { get; private set; }

    public Guid CandidateProductId { get; private set; }

    public TestKind TestKind { get; private set; }

    /// <summary>The key of the catalog procedure (<see cref="TestKind.Procedure"/> only), e.g. <c>softwareTest</c>.</summary>
    public string? TestTypeKey { get; private set; }

    /// <summary>The name of „Andere Prüfung“ (<see cref="TestKind.Other"/> only).</summary>
    public string? OtherTestTypeName { get; private set; }

    public string TestedByName { get; private set; } = null!;

    public DateOnly TestDate { get; private set; }

    public string? TestedVersion { get; private set; }

    public string? EvidenceUrl { get; private set; }

    /// <summary>Orders the evidences of a candidate as they were added.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    internal static TestEvidence Create(CandidateProduct candidate, TestEvidenceFacts facts, DateTimeOffset createdAt)
    {
        var evidence = new TestEvidence
        {
            Id = Guid.NewGuid(),
            OrganizationId = candidate.OrganizationId,
            CandidateProductId = candidate.Id,
            CreatedAt = createdAt,
        };
        evidence.Apply(facts);
        return evidence;
    }

    /// <summary>Whether <paramref name="facts"/> describe what this evidence holds.</summary>
    internal bool Holds(TestEvidenceFacts facts) =>
        TestKind == facts.Kind && TestTypeKey == facts.TestTypeKey && OtherTestTypeName == facts.OtherTestTypeName
        && TestedByName == facts.TestedByName && TestDate == facts.TestDate && TestedVersion == facts.TestedVersion
        && EvidenceUrl == facts.EvidenceUrl;

    /// <summary>
    /// Replaces every field — the evidence of a new product version keeps its row and id (C65).
    /// </summary>
    internal void Apply(TestEvidenceFacts facts)
    {
        facts.Check();
        TestKind = facts.Kind;
        TestTypeKey = facts.TestTypeKey;
        OtherTestTypeName = facts.OtherTestTypeName;
        TestedByName = facts.TestedByName;
        TestDate = facts.TestDate;
        TestedVersion = facts.TestedVersion;
        EvidenceUrl = facts.EvidenceUrl;
    }
}

/// <summary>
/// What a test evidence says, checked by the endpoint before (trimmed, empty optional text as
/// <c>null</c>, the procedure key known to the catalog, the date not in the future).
/// </summary>
internal sealed record TestEvidenceFacts(
    TestKind Kind,
    string? TestTypeKey,
    string? OtherTestTypeName,
    string TestedByName,
    DateOnly TestDate,
    string? TestedVersion,
    string? EvidenceUrl)
{
    /// <summary>Throws when the fields do not fit the kind or exceed their limits.</summary>
    public void Check()
    {
        if (!Enum.IsDefined(Kind))
        {
            throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "Unknown test kind.");
        }

        if ((Kind == TestKind.Procedure) != !string.IsNullOrWhiteSpace(TestTypeKey))
        {
            throw new ArgumentException("A procedure, and only a procedure, names a test type.", nameof(TestTypeKey));
        }

        if ((Kind == TestKind.Other) != !string.IsNullOrWhiteSpace(OtherTestTypeName))
        {
            throw new ArgumentException("„Andere Prüfung“, and only it, names its procedure.", nameof(OtherTestTypeName));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(TestedByName);
        CheckLength(OtherTestTypeName, TestEvidence.OtherTestTypeNameMaxLength, nameof(OtherTestTypeName));
        CheckLength(TestedByName, TestEvidence.TestedByNameMaxLength, nameof(TestedByName));
        CheckLength(TestedVersion, TestEvidence.TestedVersionMaxLength, nameof(TestedVersion));
        CheckLength(EvidenceUrl, TestEvidence.EvidenceUrlMaxLength, nameof(EvidenceUrl));
    }

    private static void CheckLength(string? value, int maxLength, string name)
    {
        if (value?.Length > maxLength)
        {
            throw new ArgumentException($"At most {maxLength} characters.", name);
        }
    }
}
