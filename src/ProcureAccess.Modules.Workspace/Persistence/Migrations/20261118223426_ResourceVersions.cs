using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.Workspace.Persistence.Migrations
{
    /// <summary>
    /// The version and its editor on projects, candidate products and test evidences: the
    /// concurrency token behind their ETags and the competing editor of a 409 (S6-20; ARCHITECTURE
    /// §7.1; API-03, API-04). Existing rows start at version 1 without an editor.
    /// </summary>
    internal partial class ResourceVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModifiedByUserId",
                schema: "workspace",
                table: "TestEvidences",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "workspace",
                table: "TestEvidences",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByUserId",
                schema: "workspace",
                table: "ProcurementProjects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "workspace",
                table: "ProcurementProjects",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByUserId",
                schema: "workspace",
                table: "CandidateProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "workspace",
                table: "CandidateProducts",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedByUserId",
                schema: "workspace",
                table: "TestEvidences");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "workspace",
                table: "TestEvidences");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserId",
                schema: "workspace",
                table: "ProcurementProjects");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "workspace",
                table: "ProcurementProjects");

            migrationBuilder.DropColumn(
                name: "ModifiedByUserId",
                schema: "workspace",
                table: "CandidateProducts");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "workspace",
                table: "CandidateProducts");
        }
    }
}
