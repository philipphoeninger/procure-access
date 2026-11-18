// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Workspace.Domain;

/// <summary>
/// The assessment of one requirement criterion for one candidate product (ARCHITECTURE §5.4, C53;
/// SPEC R-CAN-06; S6-18): a value and the note „Worauf stützt sich Ihre Einschätzung?“.
/// </summary>
/// <remarks>
/// <para><b>Lazy rows.</b> A criterion without a row is „Nicht eingeschätzt“; a row is stored with
/// the first value or note and removed when it would say nothing (<see cref="SaysNothing"/>).</para>
/// <para><b>Key.</b> One row per candidate and criterion revision of the release the project's
/// list is pinned to. The revision belongs to the catalog, another context, so it is an id without
/// a foreign key (ADR-014). Procedure criteria never get a row (CAT-17); the service checks the
/// revision against the project's list before it stores one.</para>
/// <para><b>Concurrency.</b> Single-field rows: the last write wins, no <c>If-Match</c> (R-CAN-11).</para>
/// </remarks>
internal sealed class CandidateAssessment
{
    /// <summary>Longest note (SPEC §7.2).</summary>
    public const int EvidenceNoteMaxLength = 1000;

    private CandidateAssessment()
    {
    }

    public Guid CandidateProductId { get; private set; }

    /// <summary>The criterion revision in the release of the project's list (catalog id).</summary>
    public int CriterionRevisionId { get; private set; }

    /// <summary>The organisation of the candidate, for the organisation filter (TEN-01).</summary>
    public Guid OrganizationId { get; private set; }

    public Fulfillment Fulfillment { get; private set; }

    public string? EvidenceNote { get; private set; }

    public DateTimeOffset ModifiedAt { get; private set; }

    /// <summary>A row without a value and without a note is the same as no row.</summary>
    public bool SaysNothing => Fulfillment == Fulfillment.Unknown && EvidenceNote is null;

    /// <summary>A new row of <paramref name="candidate"/> for a criterion, still saying nothing.</summary>
    public static CandidateAssessment Of(CandidateProduct candidate, int criterionRevisionId)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        return new CandidateAssessment
        {
            CandidateProductId = candidate.Id,
            CriterionRevisionId = criterionRevisionId,
            OrganizationId = candidate.OrganizationId,
        };
    }

    /// <summary>
    /// Replaces value and note (<c>null</c> for none); returns whether anything changed. The caller
    /// has checked the value against the allowed ratings of the criterion (CAT-15).
    /// </summary>
    public bool Change(Fulfillment fulfillment, string? evidenceNote, DateTimeOffset changedAt)
    {
        if (!Enum.IsDefined(fulfillment))
        {
            throw new ArgumentOutOfRangeException(nameof(fulfillment), fulfillment, "Unknown fulfilment.");
        }

        if (evidenceNote is { Length: 0 or > EvidenceNoteMaxLength })
        {
            throw new ArgumentException($"A note has 1 to {EvidenceNoteMaxLength} characters.", nameof(evidenceNote));
        }

        if (fulfillment == Fulfillment && evidenceNote == EvidenceNote)
        {
            return false;
        }

        Fulfillment = fulfillment;
        EvidenceNote = evidenceNote;
        ModifiedAt = changedAt;
        return true;
    }
}
