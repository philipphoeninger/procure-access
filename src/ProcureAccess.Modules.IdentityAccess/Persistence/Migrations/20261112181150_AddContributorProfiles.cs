using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Migrations
{
    /// <summary>
    /// Contributor profiles and their user links (S4-25, roadmap M3.4, ARCHITECTURE §5.2). The
    /// case-insensitive ICU collation of the display name lives in <c>public</c> (see
    /// <c>IdentityAccessDbContext.CaseInsensitiveCollation</c>). Nothing moves from the legacy context.
    /// </summary>
    internal partial class AddContributorProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:public.identity_case_insensitive", "und-u-ks-level2,und-u-ks-level2,icu,False");

            migrationBuilder.CreateTable(
                name: "ContributorProfiles",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false, collation: "identity_case_insensitive"),
                    Kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContributorProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserContributorLinks",
                schema: "identity",
                columns: table => new
                {
                    ContributorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserContributorLinks", x => x.ContributorProfileId);
                    table.ForeignKey(
                        name: "FK_UserContributorLinks_ContributorProfiles_ContributorProfile~",
                        column: x => x.ContributorProfileId,
                        principalSchema: "identity",
                        principalTable: "ContributorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContributorProfiles_DisplayName",
                schema: "identity",
                table: "ContributorProfiles",
                column: "DisplayName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserContributorLinks_UserId",
                schema: "identity",
                table: "UserContributorLinks",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserContributorLinks",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "ContributorProfiles",
                schema: "identity");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:CollationDefinition:public.identity_case_insensitive", "und-u-ks-level2,und-u-ks-level2,icu,False");
        }
    }
}
