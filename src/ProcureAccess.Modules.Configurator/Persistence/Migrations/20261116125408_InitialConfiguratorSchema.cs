using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureAccess.Modules.Configurator.Persistence.Migrations
{
    /// <summary>
    /// The saved criteria lists (S5-19, ARCHITECTURE §5.3): a list of an organisation, pinned to a
    /// catalog release by its version, with its selection by value keys and one decision per
    /// criterion; the four row tables are deleted with their list. No foreign key leaves the schema
    /// (ADR-014). The organisation filter is bound in the context, not in the schema.
    /// </summary>
    internal partial class InitialConfiguratorSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "configurator");

            migrationBuilder.CreateTable(
                name: "CriteriaConfigurations",
                schema: "configurator",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ReleaseVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UseCaseKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriteriaConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationAppTypes",
                schema: "configurator",
                columns: table => new
                {
                    AppTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationAppTypes", x => new { x.ConfigurationId, x.AppTypeKey });
                    table.ForeignKey(
                        name: "FK_ConfigurationAppTypes_CriteriaConfigurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalSchema: "configurator",
                        principalTable: "CriteriaConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationCriteria",
                schema: "configurator",
                columns: table => new
                {
                    CriterionRevisionId = table.Column<int>(type: "integer", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsIncluded = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvedLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    OverrideLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    OverrideWeight = table.Column<int>(type: "integer", nullable: true),
                    NoteDe = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationCriteria", x => new { x.ConfigurationId, x.CriterionRevisionId });
                    table.ForeignKey(
                        name: "FK_ConfigurationCriteria_CriteriaConfigurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalSchema: "configurator",
                        principalTable: "CriteriaConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationProductParts",
                schema: "configurator",
                columns: table => new
                {
                    ProductPartKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationProductParts", x => new { x.ConfigurationId, x.ProductPartKey });
                    table.ForeignKey(
                        name: "FK_ConfigurationProductParts_CriteriaConfigurations_Configurat~",
                        column: x => x.ConfigurationId,
                        principalSchema: "configurator",
                        principalTable: "CriteriaConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationTestTypes",
                schema: "configurator",
                columns: table => new
                {
                    TestTypeKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationTestTypes", x => new { x.ConfigurationId, x.TestTypeKey });
                    table.ForeignKey(
                        name: "FK_ConfigurationTestTypes_CriteriaConfigurations_Configuration~",
                        column: x => x.ConfigurationId,
                        principalSchema: "configurator",
                        principalTable: "CriteriaConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaConfigurations_CreatedByUserId",
                schema: "configurator",
                table: "CriteriaConfigurations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CriteriaConfigurations_OrganizationId",
                schema: "configurator",
                table: "CriteriaConfigurations",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigurationAppTypes",
                schema: "configurator");

            migrationBuilder.DropTable(
                name: "ConfigurationCriteria",
                schema: "configurator");

            migrationBuilder.DropTable(
                name: "ConfigurationProductParts",
                schema: "configurator");

            migrationBuilder.DropTable(
                name: "ConfigurationTestTypes",
                schema: "configurator");

            migrationBuilder.DropTable(
                name: "CriteriaConfigurations",
                schema: "configurator");
        }
    }
}
