using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.Workspace.Persistence.Migrations
{
    /// <summary>
    /// The candidate products of the projects with their private sections, test evidences and the
    /// private notes of the evidences (S6-15; ARCHITECTURE §5.4; WSP-01, SIG-03).
    /// </summary>
    internal partial class CandidateProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CandidateProducts",
                schema: "workspace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcurementProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SoftwareProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VendorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    FunctionalSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TestStatus = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, defaultValue: "Unknown"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateProducts_ProcurementProjects_ProcurementProjectId",
                        column: x => x.ProcurementProjectId,
                        principalSchema: "workspace",
                        principalTable: "ProcurementProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CandidatePrivateSections",
                schema: "workspace",
                columns: table => new
                {
                    CandidateProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    InternalNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidatePrivateSections", x => x.CandidateProductId);
                    table.ForeignKey(
                        name: "FK_CandidatePrivateSections_CandidateProducts_CandidateProduct~",
                        column: x => x.CandidateProductId,
                        principalSchema: "workspace",
                        principalTable: "CandidateProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestEvidences",
                schema: "workspace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CandidateProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    TestKind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TestTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    OtherTestTypeName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    TestedByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TestDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TestedVersion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EvidenceUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestEvidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestEvidences_CandidateProducts_CandidateProductId",
                        column: x => x.CandidateProductId,
                        principalSchema: "workspace",
                        principalTable: "CandidateProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestEvidencePrivateNotes",
                schema: "workspace",
                columns: table => new
                {
                    TestEvidenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestEvidencePrivateNotes", x => x.TestEvidenceId);
                    table.ForeignKey(
                        name: "FK_TestEvidencePrivateNotes_TestEvidences_TestEvidenceId",
                        column: x => x.TestEvidenceId,
                        principalSchema: "workspace",
                        principalTable: "TestEvidences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateProducts_ProcurementProjectId",
                schema: "workspace",
                table: "CandidateProducts",
                column: "ProcurementProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TestEvidences_CandidateProductId_TestTypeKey",
                schema: "workspace",
                table: "TestEvidences",
                columns: new[] { "CandidateProductId", "TestTypeKey" },
                unique: true,
                filter: "\"TestTypeKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidatePrivateSections",
                schema: "workspace");

            migrationBuilder.DropTable(
                name: "TestEvidencePrivateNotes",
                schema: "workspace");

            migrationBuilder.DropTable(
                name: "TestEvidences",
                schema: "workspace");

            migrationBuilder.DropTable(
                name: "CandidateProducts",
                schema: "workspace");
        }
    }
}
