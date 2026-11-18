using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Migrations
{
    /// <summary>
    /// PlatformAdmin loses the permission <c>moderation:review</c> (S6-23, ARCHITECTURE §9.1,
    /// §12 step 5; owner decision O-S6-4 (a)). It carried it since S4-25 only for the legacy
    /// proposals, which the legacy context's migration <c>RetireProposals</c> retires. Moderator
    /// keeps it for the moderation of step 7. A data change only; the model is unchanged.
    /// </summary>
    /// <remarks>
    /// The role claims belong to this context since S6-10a, so the change lives here and not in the
    /// legacy migration (no migration crosses two contexts, ADR-014). A session issued before keeps
    /// the claim until its security stamp or its sign-in renews the principal. <c>Down</c> gives the
    /// permission back, as <c>AdoptIdentityTables</c> did. Internal, like everything outside
    /// <c>Contracts</c> (ARC-03).
    /// </remarks>
    internal partial class RevokePlatformAdminModerationReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM identity."RoleClaims" c
                 USING identity."Roles" r
                 WHERE c."RoleId" = r."Id"
                   AND r."NormalizedName" = 'PLATFORMADMIN'
                   AND c."ClaimType" = 'permission'
                   AND c."ClaimValue" = 'moderation:review';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO identity."RoleClaims" ("RoleId", "ClaimType", "ClaimValue")
                SELECT r."Id", 'permission', 'moderation:review'
                  FROM identity."Roles" r
                 WHERE r."NormalizedName" = 'PLATFORMADMIN'
                   AND NOT EXISTS (
                       SELECT 1 FROM identity."RoleClaims" c
                        WHERE c."RoleId" = r."Id" AND c."ClaimType" = 'permission' AND c."ClaimValue" = 'moderation:review');
                """);
        }
    }
}
