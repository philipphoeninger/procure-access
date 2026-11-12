// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Authorization;

public static class Permissions
{
    public const string ObjectsCreate = "objects:create";
    public const string ObjectsRead = "objects:read";
    public const string ObjectsUpdate = "objects:update";
    public const string ObjectsDelete = "objects:delete";

    /// <summary>
    /// Review and approve proposals; carried by <see cref="Roles.Moderator"/> and
    /// <see cref="Roles.PlatformAdmin"/>. Replaces <c>objects:approve</c> since S4-25 (ARCHITECTURE §9.1).
    /// </summary>
    public const string ModerationReview = "moderation:review";
}
