using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.Workspace.Persistence.Migrations
{
    /// <summary>
    /// The assessments of the candidate products, one row per candidate and criterion revision,
    /// stored with the first value or note (S6-18; ARCHITECTURE §5.4, C53; WSP-02).
    /// </summary>
    internal partial class CandidateAssessments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateAssessments",
                schema: "workspace",
                columns: table => new
                {
                    CandidateProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionRevisionId = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fulfillment = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EvidenceNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateAssessments", x => new { x.CandidateProductId, x.CriterionRevisionId });
                    table.ForeignKey(
                        name: "FK_CandidateAssessments_CandidateProducts_CandidateProductId",
                        column: x => x.CandidateProductId,
                        principalSchema: "workspace",
                        principalTable: "CandidateProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateAssessments",
                schema: "workspace");
        }
    }
}
