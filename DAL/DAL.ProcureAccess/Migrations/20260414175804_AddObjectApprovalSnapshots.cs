using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.ProcureAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddObjectApprovalSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                schema: "dbo",
                table: "Proposals");

            migrationBuilder.RenameColumn(
                name: "ApprovalNote",
                schema: "dbo",
                table: "Proposals",
                newName: "ProductSnapshot");

            migrationBuilder.AddColumn<string>(
                name: "CriterionSnapshot",
                schema: "dbo",
                table: "Proposals",
                type: "nvarchar(max)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "dbo",
                table: "Proposals",
                type: "nvarchar(max)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "dbo",
                table: "Proposals",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CriterionSnapshot",
                schema: "dbo",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "dbo",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "dbo",
                table: "Proposals");

            migrationBuilder.RenameColumn(
                name: "ProductSnapshot",
                schema: "dbo",
                table: "Proposals",
                newName: "ApprovalNote");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                schema: "dbo",
                table: "Proposals",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
