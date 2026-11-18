// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Workspace.Domain;

/// <summary>
/// The private section of a candidate product, „Nur für Ihre Organisation“ (SPEC R-CAN-02; UX copy
/// <c>candidate.form.privateSection</c>): contacts and internal notes. Never published (PRV-01,
/// PRV-02).
/// </summary>
/// <remarks>
/// Its own row beside <see cref="CandidateProduct"/>, one per candidate, created with it and keyed
/// by its id (S6-15). The candidate has no member that leads here, so the publication of step 6,
/// which copies a candidate and its evidences into the directory, cannot reach these fields.
/// </remarks>
internal sealed class CandidatePrivateSection
{
    /// <summary>Longest contacts and internal notes (SPEC §7.2).</summary>
    public const int NotesMaxLength = 2000;

    private CandidatePrivateSection()
    {
    }

    /// <summary>The candidate; also the key, one row per candidate.</summary>
    public Guid CandidateProductId { get; private set; }

    /// <summary>The organisation of the candidate, for the organisation filter (TEN-01).</summary>
    public Guid OrganizationId { get; private set; }

    /// <summary>„Ansprechpersonen und Kontaktdaten“.</summary>
    public string? ContactNotes { get; private set; }

    /// <summary>„Interne Notizen“.</summary>
    public string? InternalNotes { get; private set; }

    public static CandidatePrivateSection Of(CandidateProduct candidate, string? contactNotes, string? internalNotes)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        var section = new CandidatePrivateSection { CandidateProductId = candidate.Id, OrganizationId = candidate.OrganizationId };
        section.Change(contactNotes, internalNotes);
        return section;
    }

    /// <summary>Replaces both notes; returns whether one changed.</summary>
    public bool Change(string? contactNotes, string? internalNotes)
    {
        CheckLength(contactNotes, nameof(contactNotes));
        CheckLength(internalNotes, nameof(internalNotes));
        if (contactNotes == ContactNotes && internalNotes == InternalNotes)
        {
            return false;
        }

        (ContactNotes, InternalNotes) = (contactNotes, internalNotes);
        return true;
    }

    private static void CheckLength(string? value, string name)
    {
        if (value?.Length > NotesMaxLength)
        {
            throw new ArgumentException($"At most {NotesMaxLength} characters.", name);
        }
    }
}
