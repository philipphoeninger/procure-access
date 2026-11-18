using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.Workspace.Persistence.Migrations
{
    /// <summary>
    /// The procurement projects (S6-11, ARCHITECTURE §5.4): a project of an organisation with its
    /// name, status, creator and last change. No foreign key leaves the schema (ADR-014). The
    /// organisation filter is bound in the context, not in the schema.
    /// </summary>
    internal partial class InitialWorkspaceSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "workspace");

            migrationBuilder.CreateTable(
                name: "ProcurementProjects",
                schema: "workspace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementProjects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementProjects_CreatedByUserId",
                schema: "workspace",
                table: "ProcurementProjects",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementProjects_OrganizationId",
                schema: "workspace",
                table: "ProcurementProjects",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcurementProjects",
                schema: "workspace");
        }
    }
}
