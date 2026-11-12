// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Domain;

/// <summary>
/// The pseudonym under which a user contributes to the directory (ARCHITECTURE §5.2; SPEC R-CTR-01,
/// FA-13). It holds <b>no account and no organisation reference</b>: only the restricted
/// <see cref="UserContributorLink"/> knows its user, and that link never leaves the module (PRV-04).
/// </summary>
/// <remarks>
/// Groundwork of roadmap M3.4 (sprint 04, S4-25): no endpoint creates or reads profiles yet; they
/// come with S50 in migration step 6.
/// </remarks>
internal sealed class ContributorProfile
{
    /// <summary>Longest display name (SPEC §7.2), counted in UTF-16 code units like <see cref="string.Length"/>.</summary>
    public const int DisplayNameMaxLength = 60;

    private ContributorProfile()
    {
    }

    /// <summary>Random, so the identifier says nothing about other profiles or their order.</summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The pseudonym, unique across all profiles regardless of case (SPEC §7.2; a unique index with
    /// a case-insensitive collation).
    /// </summary>
    public string DisplayName { get; private set; } = null!;

    public ContributorKind Kind { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Creates a profile. <paramref name="createdAt"/> comes from <see cref="TimeProvider"/>
    /// (CLAUDE.md §7). Throws for an empty display name, one longer than
    /// <see cref="DisplayNameMaxLength"/> or an undefined kind; uniqueness is the database's.
    /// </summary>
    public static ContributorProfile Create(string displayName, ContributorKind kind, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        if (displayName.Length > DisplayNameMaxLength)
        {
            throw new ArgumentException(
                $"A display name has at most {DisplayNameMaxLength} characters.", nameof(displayName));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown contributor kind.");
        }

        return new ContributorProfile
        {
            Id = Guid.NewGuid(),
            DisplayName = displayName,
            Kind = kind,
            CreatedAt = createdAt,
        };
    }
}
