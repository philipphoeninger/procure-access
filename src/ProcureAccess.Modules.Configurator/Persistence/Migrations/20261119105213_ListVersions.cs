using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.Configurator.Persistence.Migrations
{
    /// <summary>
    /// The version and its editor on saved lists and project copies: the concurrency token behind
    /// their ETag and the competing editor of a 409 (S6-20b; ARCHITECTURE §7.1; API-03, API-04).
    /// Existing rows start at version 1 without an editor.
    /// </summary>
    internal partial class ListVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModifiedByUserId",
                schema: "configurator",
                table: "CriteriaConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "configurator",
                table: "CriteriaConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedByUserId",
                schema: "configurator",
                table: "CriteriaConfigurations");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "configurator",
                table: "CriteriaConfigurations");
        }
    }
}
