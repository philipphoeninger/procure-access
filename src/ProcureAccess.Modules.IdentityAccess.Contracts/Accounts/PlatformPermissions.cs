// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

/// <summary>
/// The permissions the <see cref="PlatformRoles"/> grant (ARCHITECTURE §9.1): role claims of type
/// <see cref="ClaimType"/> in <c>identity."RoleClaims"</c>, which Identity's claims factory puts
/// into the cookie principal and the session probe lists. Here since S7-02 (TECH_DEBT TD-95;
/// formerly <c>Permissions</c> and <c>CustomClaimTypes</c> of the legacy MODELS project, whose
/// unused <c>objects:*</c> values went with them).
/// </summary>
public static class PlatformPermissions
{
    /// <summary>The claim type of a permission.</summary>
    public const string ClaimType = "permission";

    /// <summary>
    /// The moderation permission of <see cref="PlatformRoles.Moderator"/>; replaces
    /// <c>objects:approve</c> since S4-25. No endpoint needs it since the legacy proposals went
    /// (S6-23), which also took it from PlatformAdmin; the moderation of step 7 uses it again.
    /// </summary>
    public const string ModerationReview = "moderation:review";
}
