using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAL.ProcureAccess.Migrations
{
    /// <summary>
    /// Retires the legacy proposals (S6-23, ARCHITECTURE §12 step 5; owner decision O-S6-4 (a) of
    /// 2026-11-18): the table <c>public."Proposals"</c> goes with its rows, indexes, check
    /// constraint and the reviewer column <c>ApproverId</c>. Pre-pilot data, nothing to export
    /// (TD-61, #384). Products and criteria keep their rows: a row only came from an approved
    /// proposal, so every one of them was visible before and stays visible.
    /// </summary>
    /// <remarks>
    /// <c>Down</c> creates the empty table as the earlier migrations left it; the rows do not come back.
    /// </remarks>
    public partial class RetireProposals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Proposals",
                schema: "public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Proposals",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CriterionId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    ApproverId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CriterionSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "text", maxLength: 200, nullable: true),
                    ProductSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProposerId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proposals", x => x.Id);
                    table.CheckConstraint("CK_Proposal_Snapshot_NN", "(\"ProductSnapshot\" IS NULL) <> (\"CriterionSnapshot\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_Proposals_Criteria_CriterionId",
                        column: x => x.CriterionId,
                        principalSchema: "public",
                        principalTable: "Criteria",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Proposals_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "public",
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_ApproverId",
                schema: "public",
                table: "Proposals",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_CriterionId",
                schema: "public",
                table: "Proposals",
                column: "CriterionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_ProductId",
                schema: "public",
                table: "Proposals",
                column: "ProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_ProductId_CriterionId",
                schema: "public",
                table: "Proposals",
                columns: new[] { "ProductId", "CriterionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_ProposerId",
                schema: "public",
                table: "Proposals",
                column: "ProposerId");
        }
    }
}
