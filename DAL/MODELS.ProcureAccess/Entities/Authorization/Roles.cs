// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Authorization;

/// <summary>
/// Role names of the legacy Identity tables. The platform roles of ARCHITECTURE §9.1 replace the
/// legacy <c>Admin</c> and <c>Approver</c> since S4-25 (migration <c>RenamePlatformRoles</c>,
/// owner decisions O-S4-81, O-S4-82); <see cref="Member"/> becomes an organisation role in step 3.
/// </summary>
public static class Roles
{
    public const string Member = "Member";

    /// <summary>Operates the platform (SPEC SD22: held by the owner); formerly <c>Admin</c>.</summary>
    public const string PlatformAdmin = "PlatformAdmin";

    /// <summary>Maintains the catalog; no endpoint needs it yet (SPEC SD20: releases by CLI).</summary>
    public const string CatalogEditor = "CatalogEditor";

    /// <summary>Reviews contributions; formerly <c>Approver</c>.</summary>
    public const string Moderator = "Moderator";

    /// <summary>The roles the <c>roles</c> command grants and revokes (SPEC R-ADM-03).</summary>
    public static readonly IReadOnlyList<string> PlatformRoles = [PlatformAdmin, CatalogEditor, Moderator];
}
