// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;

namespace ProcureAccess.Modules.IdentityAccess.Domain;

/// <summary>
/// The tenant boundary (ARCHITECTURE §5.2, §5.4): every organisation-scoped entity carries this
/// id as <c>OrganizationId</c>, and the global query filters hide what the current user is no
/// member of (TEN-01). The row itself is scoped too: a non-member never reads its name.
/// </summary>
internal sealed class Organization
{
    /// <summary>Longest name (SPEC §7.2), counted in UTF-16 code units like <see cref="string.Length"/>.</summary>
    public const int NameMaxLength = 120;

    private Organization()
    {
    }

    /// <summary>Random, so the identifier says nothing about other organisations or their order.</summary>
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public OrganizationType Type { get; private set; }

    /// <summary>The ASP.NET Core Identity user id of the creator (§5.2); no foreign key while users are legacy (ADR-014).</summary>
    public string CreatedByUserId { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Creates an organisation. <paramref name="createdAt"/> comes from <see cref="TimeProvider"/>
    /// (CLAUDE.md §7). Throws for an empty or too long name, an undefined type or an empty creator.
    /// The creator's Owner membership is a separate <see cref="OrganizationMembership"/> (R-ORG-01).
    /// </summary>
    public static Organization Create(string name, OrganizationType type, string createdByUserId, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > NameMaxLength)
        {
            throw new ArgumentException($"An organisation name has at most {NameMaxLength} characters.", nameof(name));
        }

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown organisation type.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(createdByUserId);

        return new Organization
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            CreatedByUserId = createdByUserId,
            CreatedAt = createdAt,
        };
    }
}
