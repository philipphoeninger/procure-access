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
    /// The moderation permission of <see cref="Roles.Moderator"/> (ARCHITECTURE §9.1); replaces
    /// <c>objects:approve</c> since S4-25. No endpoint needs it since the legacy proposals went
    /// (S6-23), which also took it from PlatformAdmin; the moderation of step 7 uses it again.
    /// </summary>
    public const string ModerationReview = "moderation:review";
}
