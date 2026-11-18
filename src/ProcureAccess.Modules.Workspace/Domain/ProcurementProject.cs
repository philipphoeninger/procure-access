// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Workspace.Domain;

/// <summary>
/// A procurement project, the Beschaffungsvorgang (ARCHITECTURE §5.4; SPEC R-PRJ-01, R-PRJ-05):
/// owned by an organisation, with a name and a status. Its copy of a criteria list (C53) comes
/// with S6-12; its candidate products (<see cref="CandidateProduct"/>, S6-15) refer to it. A project is never deleted in v1 (SPEC §3).
/// </summary>
internal sealed class ProcurementProject
{
    /// <summary>Longest name (SPEC §7.2), counted in UTF-16 code units like <see cref="string.Length"/>.</summary>
    public const int NameMaxLength = 120;

    private ProcurementProject()
    {
    }

    /// <summary>Random, so the identifier says nothing about other projects or their order.</summary>
    public Guid Id { get; private set; }

    /// <summary>The owning organisation (IdentityAccess); no foreign key across contexts (ADR-014).</summary>
    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; } = null!;

    public ProjectStatus Status { get; private set; }

    /// <summary>The user id of the creator (IdentityAccess); no foreign key across contexts (ADR-014).</summary>
    public string CreatedByUserId { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// When the name, the status or something below the project last changed (<see cref="MarkChanged"/>);
    /// the creation time until then.
    /// </summary>
    public DateTimeOffset ModifiedAt { get; private set; }

    /// <summary>
    /// Counts the changes of the name, the status and the assigned criteria list (<see cref="MarkEdited"/>); the concurrency
    /// token and, with the 🔧 epoch, the <c>ETag</c> (API-03, API-04; S6-20). A change below the
    /// project (<see cref="MarkChanged"/>) does not count: it overwrites nothing of the project.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// The user id (IdentityAccess) of who made <see cref="Version"/>, the competing editor of a 409
    /// <c>conflict</c>; <c>null</c> for a row older than S6-20 or made without a user. No foreign
    /// key across contexts (ADR-014).
    /// </summary>
    public string? ModifiedByUserId { get; private set; }

    /// <summary>An archived project is read-only for everyone; only its status may change (R-PRJ-05).</summary>
    public bool IsArchived => Status == ProjectStatus.Archived;

    /// <summary>
    /// Creates a project with the status „Laufend“. <paramref name="createdAt"/> comes from
    /// <see cref="TimeProvider"/> (CLAUDE.md §7). Throws for an empty or too long name.
    /// </summary>
    public static ProcurementProject Create(Guid organizationId, string name, string createdByUserId, DateTimeOffset createdAt)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("A project belongs to an organisation.", nameof(organizationId));
        }

        CheckName(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdByUserId);

        return new ProcurementProject
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = name,
            Status = ProjectStatus.Open,
            CreatedByUserId = createdByUserId,
            CreatedAt = createdAt,
            ModifiedAt = createdAt,
            Version = 1,
            ModifiedByUserId = createdByUserId,
        };
    }

    /// <summary>
    /// Changes the name. Throws for an empty or too long name and for an archived project; the
    /// same name changes nothing.
    /// </summary>
    public void Rename(string name, DateTimeOffset now)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("An archived project cannot be renamed.");
        }

        CheckName(name);
        if (name == Name)
        {
            return;
        }

        Name = name;
        ModifiedAt = now;
    }

    /// <summary>
    /// Changes the status; every change is allowed, also away from „Archiviert“ (owner decision
    /// S6-11 2 (a)). The same status changes nothing.
    /// </summary>
    public void ChangeStatus(ProjectStatus status, DateTimeOffset now)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown project status.");
        }

        if (status == Status)
        {
            return;
        }

        Status = status;
        ModifiedAt = now;
    }

    /// <summary>
    /// Counts a change of the name, the status or the assigned criteria list by <paramref name="userId"/>: a new
    /// <see cref="Version"/>, so every <c>ETag</c> issued before is stale (API-03).
    /// </summary>
    public void MarkEdited(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        Version++;
        ModifiedByUserId = userId;
    }

    /// <summary>
    /// Records a change below the project — a candidate, its test status, an evidence — as the
    /// project's last change (owner decision S6-15 5 (a)). Throws for an archived project, which
    /// changes nothing but its status. The <see cref="Version"/> stays.
    /// </summary>
    public void MarkChanged(DateTimeOffset now)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("Nothing of an archived project changes but its status.");
        }

        ModifiedAt = now;
    }

    private static void CheckName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > NameMaxLength)
        {
            throw new ArgumentException($"A project name has at most {NameMaxLength} characters.", nameof(name));
        }
    }
}
