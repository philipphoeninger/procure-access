using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.ProcureAccess.Migrations
{
    /// <summary>
    /// The platform roles of ARCHITECTURE §9.1 (S4-25, roadmap M3.4): <c>Approver</c> becomes
    /// <c>Moderator</c>, <c>Admin</c> becomes <c>PlatformAdmin</c> and the permission
    /// <c>objects:approve</c> becomes <c>moderation:review</c> (owner decision O-S4-81); users keep
    /// their roles, because the rows are renamed, not replaced. Then every environment gets the three
    /// platform roles, also where the Development rebuild never created a role (O-S4-82), and
    /// Moderator and PlatformAdmin carry <c>moderation:review</c>. A JWT issued before keeps the old
    /// names until it expires (30 minutes); the next refresh reads the new ones.
    /// </summary>
    public partial class RenamePlatformRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE identity."Roles"
                   SET "Name" = 'Moderator', "NormalizedName" = 'MODERATOR', "ConcurrencyStamp" = gen_random_uuid()::text
                 WHERE "NormalizedName" = 'APPROVER';

                UPDATE identity."Roles"
                   SET "Name" = 'PlatformAdmin', "NormalizedName" = 'PLATFORMADMIN', "ConcurrencyStamp" = gen_random_uuid()::text
                 WHERE "NormalizedName" = 'ADMIN';

                UPDATE identity."RoleClaims"
                   SET "ClaimValue" = 'moderation:review'
                 WHERE "ClaimType" = 'permission' AND "ClaimValue" = 'objects:approve';

                INSERT INTO identity."Roles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
                SELECT gen_random_uuid()::text, role.name, upper(role.name), gen_random_uuid()::text
                  FROM (VALUES ('PlatformAdmin'), ('CatalogEditor'), ('Moderator')) AS role (name)
                 WHERE NOT EXISTS (SELECT 1 FROM identity."Roles" r WHERE r."NormalizedName" = upper(role.name));

                INSERT INTO identity."RoleClaims" ("RoleId", "ClaimType", "ClaimValue")
                SELECT r."Id", 'permission', 'moderation:review'
                  FROM identity."Roles" r
                 WHERE r."NormalizedName" IN ('MODERATOR', 'PLATFORMADMIN')
                   AND NOT EXISTS (
                       SELECT 1 FROM identity."RoleClaims" c
                        WHERE c."RoleId" = r."Id" AND c."ClaimType" = 'permission' AND c."ClaimValue" = 'moderation:review');
                """);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Gives the roles their legacy names back and drops <c>CatalogEditor</c> with its grants.
        /// A role that Up created where none existed stays, under its legacy name.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM identity."Roles" WHERE "NormalizedName" = 'CATALOGEDITOR';

                UPDATE identity."RoleClaims"
                   SET "ClaimValue" = 'objects:approve'
                 WHERE "ClaimType" = 'permission' AND "ClaimValue" = 'moderation:review';

                UPDATE identity."Roles"
                   SET "Name" = 'Approver', "NormalizedName" = 'APPROVER', "ConcurrencyStamp" = gen_random_uuid()::text
                 WHERE "NormalizedName" = 'MODERATOR';

                UPDATE identity."Roles"
                   SET "Name" = 'Admin', "NormalizedName" = 'ADMIN', "ConcurrencyStamp" = gen_random_uuid()::text
                 WHERE "NormalizedName" = 'PLATFORMADMIN';
                """);
        }
    }
}
