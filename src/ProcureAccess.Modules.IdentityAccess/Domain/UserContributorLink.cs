// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Domain;

/// <summary>
/// Which user holds a <see cref="ContributorProfile"/> (ARCHITECTURE §5.2). A restricted table:
/// never mapped to a DTO, never returned together with a profile or entry, never logged together
/// with one above <c>Debug</c> (PRV-04). A user may hold several profiles; a profile belongs to at
/// most one user, so <see cref="ContributorProfileId"/> is the key.
/// </summary>
/// <remarks>
/// <see cref="UserId"/> names a row of the legacy table <c>identity."Users"</c> without a foreign
/// key: no key crosses two database contexts (ADR-014). Account deletion removes the link
/// (SPEC R-AUTH-10, „detached“) once users belong to this module.
/// </remarks>
internal sealed class UserContributorLink
{
    private UserContributorLink()
    {
    }

    public Guid ContributorProfileId { get; private set; }

    /// <summary>The ASP.NET Core Identity user id.</summary>
    public string UserId { get; private set; } = null!;

    public static UserContributorLink Create(string userId, ContributorProfile profile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(profile);
        return new UserContributorLink { UserId = userId, ContributorProfileId = profile.Id };
    }
}
