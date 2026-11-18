using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Migrations
{
    /// <summary>
    /// The foreign keys onto <c>identity."Users"</c> (S6-10b, TD-72, #387; owner decision S6-10 3),
    /// possible since the users belong to this context (S6-10a): <c>AccountTokens.UserId</c>,
    /// <c>UserContributorLinks.UserId</c>, <c>OrganizationMemberships.UserId</c> and
    /// <c>UserPreferences.UserId</c> with cascade, <c>Organizations.CreatedByUserId</c> with restrict
    /// (an organisation never ends because a user row is deleted). The five columns become
    /// <c>character varying(200)</c> like <c>Users.Id</c>; every id is a GUID of 36 characters.
    /// </summary>
    /// <remarks>
    /// <para><b>Orphans first.</b> Accounts are only soft-deleted, so a deployed database holds no
    /// row without its user. A Development database does: the rebuild deleted every user on each
    /// start and left their tokens, memberships and organisations. These rows are deleted before
    /// the keys are added, an organisation whose creator is gone with its memberships and
    /// invitations (cascade). Criteria lists and share links of such an organisation belong to
    /// other contexts and stay (Development only).</para>
    /// <para><b>Soft-deleted accounts</b> (owner decision S6-10 4; S5-28a R5): their role, claim,
    /// login and token rows go and their lockout columns are reset once here; the account
    /// deletion does the same from now on.</para>
    /// <para><c>Down</c> drops the keys and widens the columns again; deleted rows do not come
    /// back. Internal, like everything outside <c>Contracts</c> (ARC-03).</para>
    /// </remarks>
    internal partial class AddUserForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM identity."AccountTokens" t
                 WHERE NOT EXISTS (SELECT 1 FROM identity."Users" u WHERE u."Id" = t."UserId");

                DELETE FROM identity."UserContributorLinks" l
                 WHERE NOT EXISTS (SELECT 1 FROM identity."Users" u WHERE u."Id" = l."UserId");

                DELETE FROM identity."UserPreferences" p
                 WHERE NOT EXISTS (SELECT 1 FROM identity."Users" u WHERE u."Id" = p."UserId");

                DELETE FROM identity."Organizations" o
                 WHERE NOT EXISTS (SELECT 1 FROM identity."Users" u WHERE u."Id" = o."CreatedByUserId");

                DELETE FROM identity."OrganizationMemberships" m
                 WHERE NOT EXISTS (SELECT 1 FROM identity."Users" u WHERE u."Id" = m."UserId");
                """);

            migrationBuilder.Sql("""
                DELETE FROM identity."UserRoles" r USING identity."Users" u
                 WHERE u."Id" = r."UserId" AND u."IsDeleted";

                DELETE FROM identity."UserClaims" c USING identity."Users" u
                 WHERE u."Id" = c."UserId" AND u."IsDeleted";

                DELETE FROM identity."UserLogins" l USING identity."Users" u
                 WHERE u."Id" = l."UserId" AND u."IsDeleted";

                DELETE FROM identity."UserTokens" t USING identity."Users" u
                 WHERE u."Id" = t."UserId" AND u."IsDeleted";

                UPDATE identity."Users"
                   SET "LockoutEnd" = NULL, "AccessFailedCount" = 0
                 WHERE "IsDeleted" AND ("LockoutEnd" IS NOT NULL OR "AccessFailedCount" <> 0);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "UserPreferences",
                type: "character varying(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "UserContributorLinks",
                type: "character varying(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                schema: "identity",
                table: "Organizations",
                type: "character varying(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "OrganizationMemberships",
                type: "character varying(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "AccountTokens",
                type: "character varying(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_CreatedByUserId",
                schema: "identity",
                table: "Organizations",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountTokens_Users_UserId",
                schema: "identity",
                table: "AccountTokens",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationMemberships_Users_UserId",
                schema: "identity",
                table: "OrganizationMemberships",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Users_CreatedByUserId",
                schema: "identity",
                table: "Organizations",
                column: "CreatedByUserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserContributorLinks_Users_UserId",
                schema: "identity",
                table: "UserContributorLinks",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferences_Users_UserId",
                schema: "identity",
                table: "UserPreferences",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountTokens_Users_UserId",
                schema: "identity",
                table: "AccountTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationMemberships_Users_UserId",
                schema: "identity",
                table: "OrganizationMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Users_CreatedByUserId",
                schema: "identity",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserContributorLinks_Users_UserId",
                schema: "identity",
                table: "UserContributorLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferences_Users_UserId",
                schema: "identity",
                table: "UserPreferences");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_CreatedByUserId",
                schema: "identity",
                table: "Organizations");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "UserPreferences",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "UserContributorLinks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                schema: "identity",
                table: "Organizations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "OrganizationMemberships",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "identity",
                table: "AccountTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)");
        }
    }
}
