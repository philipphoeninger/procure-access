using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.Catalog.Persistence.Migrations
{
    /// <summary>
    /// The short description (Kurzbeschreibung, Kriterienkatalog §6.1) of a criterion revision
    /// (ADR-016, S4-24). Nullable: every stored revision keeps no value, and the project team fills
    /// it with a new release (K-1).
    /// </summary>
    internal partial class AddShortDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortDescriptionDe",
                schema: "catalog",
                table: "CriterionRevisions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortDescriptionDe",
                schema: "catalog",
                table: "CriterionRevisions");
        }
    }
}
