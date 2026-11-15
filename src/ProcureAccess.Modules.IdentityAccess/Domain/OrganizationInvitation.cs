// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;

namespace ProcureAccess.Modules.IdentityAccess.Domain;

/// <summary>
/// An invitation into an organisation by e-mail (ARCHITECTURE §5.2, §9.1, C72, C80; SPEC R-ORG-02;
/// S5-17a): bound to the invited address, with the role chosen on invitation, valid for a 🔧
/// lifetime and accepted at most once. Only the SHA-256 hash of the link's token is stored, so the
/// table alone never yields a usable link (SEC-11). Scoped by its organisation like the memberships
/// (TEN-01); the read by token is the allow-listed unfiltered call (TEN-02), because the invitee is
/// no member yet.
/// </summary>
internal sealed class OrganizationInvitation
{
    /// <summary>Longest address: the column of the legacy <c>identity."Users"</c>.</summary>
    public const int EmailMaxLength = 256;

    private OrganizationInvitation()
    {
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    /// <summary>The address as the Owner typed it (trimmed), for the pending list and the mismatch text.</summary>
    public string Email { get; private set; } = null!;

    /// <summary>
    /// <see cref="Email"/> normalised like ASP.NET Core Identity's <c>NormalizedEmail</c>
    /// (<see cref="NormalizeEmail"/>): the key of the address binding and of one pending invitation
    /// per address.
    /// </summary>
    public string NormalizedEmail { get; private set; } = null!;

    public OrganizationRole Role { get; private set; }

    /// <summary>SHA-256 of the token's bytes.</summary>
    public byte[] TokenHash { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>When it was accepted; null while open. An accepted row stays until it expires (owner decision S5-17 D7 (a)).</summary>
    public DateTimeOffset? AcceptedAt { get; private set; }

    /// <summary>
    /// Creates an open invitation. <paramref name="createdAt"/> comes from <see cref="TimeProvider"/>
    /// (CLAUDE.md §7). Throws for an empty organisation, an empty or too long address, an undefined
    /// role, a hash of the wrong length or a lifetime that is not positive.
    /// </summary>
    public static OrganizationInvitation Create(
        Guid organizationId, string email, OrganizationRole role, byte[] tokenHash, DateTimeOffset createdAt, TimeSpan lifetime)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("An invitation needs its organisation.", nameof(organizationId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        if (email.Length > EmailMaxLength)
        {
            throw new ArgumentException($"An address has at most {EmailMaxLength} characters.", nameof(email));
        }

        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown organisation role.");
        }

        ArgumentNullException.ThrowIfNull(tokenHash);
        if (tokenHash.Length != AccountToken.HashLength)
        {
            throw new ArgumentException($"A token hash has {AccountToken.HashLength} bytes.", nameof(tokenHash));
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lifetime, TimeSpan.Zero);

        return new OrganizationInvitation
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Email = email,
            NormalizedEmail = NormalizeEmail(email),
            Role = role,
            TokenHash = tokenHash,
            CreatedAt = createdAt,
            ExpiresAt = createdAt + lifetime,
        };
    }

    /// <summary>True while it can be accepted: not accepted and not expired at <paramref name="now"/>.</summary>
    public bool IsOpenAt(DateTimeOffset now) => AcceptedAt is null && ExpiresAt > now;

    /// <summary>True when it is for <paramref name="email"/>, compared as Identity compares addresses.</summary>
    public bool IsFor(string? email) =>
        !string.IsNullOrEmpty(email) && string.Equals(NormalizedEmail, NormalizeEmail(email), StringComparison.Ordinal);

    /// <summary>Marks it accepted. Throws when it is not open at <paramref name="now"/>.</summary>
    public void Accept(DateTimeOffset now)
    {
        if (!IsOpenAt(now))
        {
            throw new InvalidOperationException("Only an open invitation can be accepted.");
        }

        AcceptedAt = now;
    }

    /// <summary>
    /// The address as ASP.NET Core Identity's default lookup normaliser writes <c>NormalizedEmail</c>
    /// (upper case, invariant culture), so the binding matches the account's address exactly as the
    /// sign-in finds it.
    /// </summary>
    public static string NormalizeEmail(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        return email.Trim().ToUpperInvariant();
    }

    /// <summary>Only the type: the address must never reach a log (PRV-11).</summary>
    public override string ToString() => nameof(OrganizationInvitation);
}
