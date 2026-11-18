using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.ProcureAccess.Migrations
{
    /// <summary>
    /// Hands the seven ASP.NET Core Identity tables of schema <c>identity</c> (<c>Users</c>,
    /// <c>Roles</c>, <c>UserClaims</c>, <c>UserRoles</c>, <c>UserLogins</c>, <c>RoleClaims</c>,
    /// <c>UserTokens</c>) over to the IdentityAccess module's own context (S6-10, ARCHITECTURE
    /// ADR-014 rule 4; owner decision O-S6-1 (a)). The tables stay as they are, with their rows:
    /// only the model of this context forgets them. From here on the IdentityAccess migrations own
    /// them; <c>AdoptIdentityTables</c> adopts the existing tables (it creates them only where they
    /// are missing).
    /// </summary>
    /// <remarks>
    /// <para>EF scaffolds <c>DropTable</c> here because the entities left the model. Never let it
    /// drop them: the rows are the accounts. <c>Down</c> creates no table for the same reason: they
    /// still exist and the earlier migrations of this context describe them.</para>
    /// <para>The two foreign keys from <c>public."Proposals"</c> to the users go (owner decision
    /// S6-10 2 (a)): no foreign key crosses two contexts (ADR-014 rule 5). <c>ProposerId</c> and
    /// <c>ApproverId</c> stay as user ids with their indexes; the proposal tables retire with
    /// S6-23. <c>Down</c> adds the keys again.</para>
    /// </remarks>
    public partial class HandOverIdentityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_Users_ApproverId",
                schema: "public",
                table: "Proposals");

            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_Users_ProposerId",
                schema: "public",
                table: "Proposals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_Users_ApproverId",
                schema: "public",
                table: "Proposals",
                column: "ApproverId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_Users_ProposerId",
                schema: "public",
                table: "Proposals",
                column: "ProposerId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
