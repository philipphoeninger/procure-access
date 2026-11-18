// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Identity;

namespace ProcureAccess.Modules.IdentityAccess.Domain;

/// <summary>
/// An account: the ASP.NET Core Identity user of table <c>identity."Users"</c> (ARCHITECTURE §5.2
/// <c>AppUser</c>, login credentials only). In this module since S6-10 (ADR-014 rule 4); other
/// modules name a user only by its id and read what they may show through
/// <c>IUserDirectory</c> of the contracts.
/// </summary>
/// <remarks>
/// A deleted account stays as a soft-deleted row without address and password (§9.3, S5-28a); the
/// context's query filter hides it, so <c>UserManager</c> does not find it.
/// </remarks>
internal sealed class User : IdentityUser
{
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// When the account was created, set by <c>ClockedUserManager</c> from the <c>TimeProvider</c>;
    /// <c>null</c> for accounts created before S5-27. The personal data export shows it
    /// (SPEC §8.4 <c>account.createdAt</c>; owner decision S5-27 2a).
    /// </summary>
    public DateTimeOffset? CreatedAt { get; set; }
}
