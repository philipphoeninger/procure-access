using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProcureAccess.Modules.Catalog.Persistence.Migrations
{
    /// <summary>
    /// First migration of the Catalog context (S3-12, ARCHITECTURE ADR-014): the core tables of the
    /// <c>catalog</c> schema (releases, criteria, revisions, standard references and their two join
    /// tables). Nothing moves from the legacy context (O-S3-5 C), so these are plain
    /// <c>CreateTable</c> statements; the filter values and rules follow in S3-13.
    /// </summary>
    /// <remarks>Internal, like everything outside <c>Contracts</c> (ARC-03); EF finds internal migrations.</remarks>
    internal partial class InitialCatalogSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "CatalogReleases",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ReleaseNotes = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogReleases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Criteria",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Category = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SupersededByKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Criteria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StandardReferences",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Standard = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Clause = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandardReferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CriterionRevisions",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CriterionId = table.Column<int>(type: "integer", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    TitleDe = table.Column<string>(type: "text", nullable: false),
                    PlainLanguageDe = table.Column<string>(type: "text", nullable: true),
                    RationaleDe = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriterionRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriterionRevisions_Criteria_CriterionId",
                        column: x => x.CriterionId,
                        principalSchema: "catalog",
                        principalTable: "Criteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatalogReleaseCriteria",
                schema: "catalog",
                columns: table => new
                {
                    CatalogReleaseId = table.Column<int>(type: "integer", nullable: false),
                    CriterionRevisionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogReleaseCriteria", x => new { x.CatalogReleaseId, x.CriterionRevisionId });
                    table.ForeignKey(
                        name: "FK_CatalogReleaseCriteria_CatalogReleases_CatalogReleaseId",
                        column: x => x.CatalogReleaseId,
                        principalSchema: "catalog",
                        principalTable: "CatalogReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogReleaseCriteria_CriterionRevisions_CriterionRevision~",
                        column: x => x.CriterionRevisionId,
                        principalSchema: "catalog",
                        principalTable: "CriterionRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CriterionRevisionStandardReferences",
                schema: "catalog",
                columns: table => new
                {
                    CriterionRevisionId = table.Column<int>(type: "integer", nullable: false),
                    StandardReferenceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriterionRevisionStandardReferences", x => new { x.CriterionRevisionId, x.StandardReferenceId });
                    table.ForeignKey(
                        name: "FK_CriterionRevisionStandardReferences_CriterionRevisions_Crit~",
                        column: x => x.CriterionRevisionId,
                        principalSchema: "catalog",
                        principalTable: "CriterionRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CriterionRevisionStandardReferences_StandardReferences_Stan~",
                        column: x => x.StandardReferenceId,
                        principalSchema: "catalog",
                        principalTable: "StandardReferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogReleaseCriteria_CriterionRevisionId",
                schema: "catalog",
                table: "CatalogReleaseCriteria",
                column: "CriterionRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogReleases_Version",
                schema: "catalog",
                table: "CatalogReleases",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Criteria_Key",
                schema: "catalog",
                table: "Criteria",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriterionRevisions_CriterionId_RevisionNumber",
                schema: "catalog",
                table: "CriterionRevisions",
                columns: new[] { "CriterionId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriterionRevisionStandardReferences_StandardReferenceId",
                schema: "catalog",
                table: "CriterionRevisionStandardReferences",
                column: "StandardReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_StandardReferences_Standard_Clause",
                schema: "catalog",
                table: "StandardReferences",
                columns: new[] { "Standard", "Clause" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogReleaseCriteria",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CriterionRevisionStandardReferences",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CatalogReleases",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CriterionRevisions",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "StandardReferences",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Criteria",
                schema: "catalog");
        }
    }
}
