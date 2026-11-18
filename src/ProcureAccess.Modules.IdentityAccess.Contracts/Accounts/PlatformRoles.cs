// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

/// <summary>
/// The platform roles of ARCHITECTURE §9.1, the role names of <c>identity."Roles"</c>. They replace
/// the legacy <c>Admin</c> and <c>Approver</c> since S4-25 (owner decisions O-S4-81, O-S4-82). The
/// global <c>Member</c> role is gone since S5-12a (owner decision O-S5-3 (a), #301): membership is
/// an organisation role; rows a database still holds are unused. Here since S6-10 (formerly
/// <c>Roles</c> of the legacy MODELS project).
/// </summary>
public static class PlatformRoles
{
    /// <summary>Operates the platform (SPEC SD22: held by the owner); formerly <c>Admin</c>.</summary>
    public const string PlatformAdmin = "PlatformAdmin";

    /// <summary>Maintains the catalog; no endpoint needs it yet (SPEC SD20: releases by CLI).</summary>
    public const string CatalogEditor = "CatalogEditor";

    /// <summary>Reviews contributions; formerly <c>Approver</c>.</summary>
    public const string Moderator = "Moderator";

    /// <summary>The roles the <c>roles</c> command grants and revokes (SPEC R-ADM-03).</summary>
    public static readonly IReadOnlyList<string> All = [PlatformAdmin, CatalogEditor, Moderator];
}
