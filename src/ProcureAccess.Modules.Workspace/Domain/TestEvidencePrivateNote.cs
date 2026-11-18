// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Workspace.Domain;

/// <summary>
/// The private note of a test evidence, „Ihre Notiz zur Qualifikation der prüfenden Stelle“
/// (ARCHITECTURE §5.4 <c>TesterQualificationNote</c>; UX copy <c>candidate.evidence.qualification</c>).
/// Never published (PRV-01, PRV-02).
/// </summary>
/// <remarks>
/// Its own row beside <see cref="TestEvidence"/>, one per evidence, created with it and keyed by
/// its id (owner decision S6-15 4 (a)); deleted with the evidence by the database.
/// </remarks>
internal sealed class TestEvidencePrivateNote
{
    /// <summary>Longest note (SPEC §7.2 v2.53, like the other private notes).</summary>
    public const int QualificationNoteMaxLength = 2000;

    private TestEvidencePrivateNote()
    {
    }

    /// <summary>The evidence; also the key, one row per evidence.</summary>
    public Guid TestEvidenceId { get; private set; }

    /// <summary>The organisation of the evidence, for the organisation filter (TEN-01).</summary>
    public Guid OrganizationId { get; private set; }

    public string? QualificationNote { get; private set; }

    public static TestEvidencePrivateNote Of(TestEvidence evidence, string? qualificationNote)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        var note = new TestEvidencePrivateNote { TestEvidenceId = evidence.Id, OrganizationId = evidence.OrganizationId };
        note.Change(qualificationNote);
        return note;
    }

    /// <summary>Replaces the note; returns whether it changed.</summary>
    public bool Change(string? qualificationNote)
    {
        if (qualificationNote?.Length > QualificationNoteMaxLength)
        {
            throw new ArgumentException($"At most {QualificationNoteMaxLength} characters.", nameof(qualificationNote));
        }

        if (qualificationNote == QualificationNote)
        {
            return false;
        }

        QualificationNote = qualificationNote;
        return true;
    }
}
