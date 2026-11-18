using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.Configurator.Persistence.Migrations
{
    /// <summary>
    /// The copy a procurement project owns (S6-12, ARCHITECTURE C53): the list's project id, empty
    /// for a list of the organisation, unique where set, so a project has at most one copy. No
    /// foreign key: the project lives in Workspace's context (ADR-014).
    /// </summary>
    internal partial class ProjectCopies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProcurementProjectId",
                schema: "configurator",
                table: "CriteriaConfigurations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaConfigurations_ProcurementProjectId",
                schema: "configurator",
                table: "CriteriaConfigurations",
                column: "ProcurementProjectId",
                unique: true,
                filter: "\"ProcurementProjectId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CriteriaConfigurations_ProcurementProjectId",
                schema: "configurator",
                table: "CriteriaConfigurations");

            migrationBuilder.DropColumn(
                name: "ProcurementProjectId",
                schema: "configurator",
                table: "CriteriaConfigurations");
        }
    }
}
