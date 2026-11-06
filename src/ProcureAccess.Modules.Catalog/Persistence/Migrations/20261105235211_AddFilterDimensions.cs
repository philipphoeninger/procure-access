using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProcureAccess.Modules.Catalog.Persistence.Migrations
{
    /// <inheritdoc />
    internal partial class AddFilterDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TestTypeId",
                schema: "catalog",
                table: "Criteria",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppTypes",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    HintDe = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Functionality = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NameDe = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductParts",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ExplanationDe = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsAlwaysSelected = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NameDe = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductParts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestTypes",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NameDe = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UseCaseTemplates",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    NameDe = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UseCaseTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CriterionApplicability",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CriterionRevisionId = table.Column<int>(type: "integer", nullable: false),
                    AppTypeId = table.Column<int>(type: "integer", nullable: false),
                    ProductPartId = table.Column<int>(type: "integer", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriterionApplicability", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriterionApplicability_AppTypes_AppTypeId",
                        column: x => x.AppTypeId,
                        principalSchema: "catalog",
                        principalTable: "AppTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CriterionApplicability_CriterionRevisions_CriterionRevision~",
                        column: x => x.CriterionRevisionId,
                        principalSchema: "catalog",
                        principalTable: "CriterionRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CriterionApplicability_ProductParts_ProductPartId",
                        column: x => x.ProductPartId,
                        principalSchema: "catalog",
                        principalTable: "ProductParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatalogReleaseAppTypeProcedures",
                schema: "catalog",
                columns: table => new
                {
                    CatalogReleaseId = table.Column<int>(type: "integer", nullable: false),
                    AppTypeId = table.Column<int>(type: "integer", nullable: false),
                    TestTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogReleaseAppTypeProcedures", x => new { x.CatalogReleaseId, x.AppTypeId, x.TestTypeId });
                    table.ForeignKey(
                        name: "FK_CatalogReleaseAppTypeProcedures_AppTypes_AppTypeId",
                        column: x => x.AppTypeId,
                        principalSchema: "catalog",
                        principalTable: "AppTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogReleaseAppTypeProcedures_CatalogReleases_CatalogRele~",
                        column: x => x.CatalogReleaseId,
                        principalSchema: "catalog",
                        principalTable: "CatalogReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogReleaseAppTypeProcedures_TestTypes_TestTypeId",
                        column: x => x.TestTypeId,
                        principalSchema: "catalog",
                        principalTable: "TestTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CriterionProcedureTexts",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CriterionRevisionId = table.Column<int>(type: "integer", nullable: false),
                    TestTypeId = table.Column<int>(type: "integer", nullable: false),
                    OriginalNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OriginalTitleDe = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApplicabilityDe = table.Column<string>(type: "text", nullable: true),
                    RequirementDe = table.Column<string>(type: "text", nullable: true),
                    RationaleWhyDe = table.Column<string>(type: "text", nullable: true),
                    AllowedRatings = table.Column<int>(type: "integer", nullable: false),
                    PositiveExamplesDe = table.Column<string>(type: "text", nullable: true),
                    NegativeExamplesDe = table.Column<string>(type: "text", nullable: true),
                    SourceUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriterionProcedureTexts", x => x.Id);
                    table.CheckConstraint("CK_CriterionProcedureTexts_AllowedRatings", "\"AllowedRatings\" > 0 AND (\"AllowedRatings\" & ~63) = 0");
                    table.ForeignKey(
                        name: "FK_CriterionProcedureTexts_CriterionRevisions_CriterionRevisio~",
                        column: x => x.CriterionRevisionId,
                        principalSchema: "catalog",
                        principalTable: "CriterionRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CriterionProcedureTexts_TestTypes_TestTypeId",
                        column: x => x.TestTypeId,
                        principalSchema: "catalog",
                        principalTable: "TestTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FilterExclusions",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceUseCaseId = table.Column<int>(type: "integer", nullable: true),
                    SourceAppTypeId = table.Column<int>(type: "integer", nullable: true),
                    SourceProductPartId = table.Column<int>(type: "integer", nullable: true),
                    TargetAppTypeId = table.Column<int>(type: "integer", nullable: true),
                    TargetProductPartId = table.Column<int>(type: "integer", nullable: true),
                    TargetTestTypeId = table.Column<int>(type: "integer", nullable: true),
                    ReasonDe = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterExclusions", x => x.Id);
                    table.CheckConstraint("CK_FilterExclusions_OneSourceOneTarget", "num_nonnulls(\"SourceUseCaseId\", \"SourceAppTypeId\", \"SourceProductPartId\") = 1\nAND num_nonnulls(\"TargetAppTypeId\", \"TargetProductPartId\", \"TargetTestTypeId\") = 1");
                    table.CheckConstraint("CK_FilterExclusions_TargetLayerLower", "NOT (\"SourceAppTypeId\" IS NOT NULL AND \"TargetAppTypeId\" IS NOT NULL)\nAND NOT (\"SourceProductPartId\" IS NOT NULL AND (\"TargetAppTypeId\" IS NOT NULL OR \"TargetProductPartId\" IS NOT NULL))");
                    table.ForeignKey(
                        name: "FK_FilterExclusions_AppTypes_SourceAppTypeId",
                        column: x => x.SourceAppTypeId,
                        principalSchema: "catalog",
                        principalTable: "AppTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FilterExclusions_AppTypes_TargetAppTypeId",
                        column: x => x.TargetAppTypeId,
                        principalSchema: "catalog",
                        principalTable: "AppTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FilterExclusions_ProductParts_SourceProductPartId",
                        column: x => x.SourceProductPartId,
                        principalSchema: "catalog",
                        principalTable: "ProductParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FilterExclusions_ProductParts_TargetProductPartId",
                        column: x => x.TargetProductPartId,
                        principalSchema: "catalog",
                        principalTable: "ProductParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FilterExclusions_TestTypes_TargetTestTypeId",
                        column: x => x.TargetTestTypeId,
                        principalSchema: "catalog",
                        principalTable: "TestTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FilterExclusions_UseCaseTemplates_SourceUseCaseId",
                        column: x => x.SourceUseCaseId,
                        principalSchema: "catalog",
                        principalTable: "UseCaseTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriorityRecommendations",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CriterionRevisionId = table.Column<int>(type: "integer", nullable: false),
                    AppTypeId = table.Column<int>(type: "integer", nullable: true),
                    UseCaseTemplateId = table.Column<int>(type: "integer", nullable: true),
                    Level = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: true),
                    RationaleDe = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriorityRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriorityRecommendations_AppTypes_AppTypeId",
                        column: x => x.AppTypeId,
                        principalSchema: "catalog",
                        principalTable: "AppTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriorityRecommendations_CriterionRevisions_CriterionRevisio~",
                        column: x => x.CriterionRevisionId,
                        principalSchema: "catalog",
                        principalTable: "CriterionRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriorityRecommendations_UseCaseTemplates_UseCaseTemplateId",
                        column: x => x.UseCaseTemplateId,
                        principalSchema: "catalog",
                        principalTable: "UseCaseTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatalogReleaseFilterExclusions",
                schema: "catalog",
                columns: table => new
                {
                    CatalogReleaseId = table.Column<int>(type: "integer", nullable: false),
                    FilterExclusionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogReleaseFilterExclusions", x => new { x.CatalogReleaseId, x.FilterExclusionId });
                    table.ForeignKey(
                        name: "FK_CatalogReleaseFilterExclusions_CatalogReleases_CatalogRelea~",
                        column: x => x.CatalogReleaseId,
                        principalSchema: "catalog",
                        principalTable: "CatalogReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogReleaseFilterExclusions_FilterExclusions_FilterExclu~",
                        column: x => x.FilterExclusionId,
                        principalSchema: "catalog",
                        principalTable: "FilterExclusions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "AppTypes",
                columns: new[] { "Id", "CreatedAt", "Functionality", "HintDe", "Key", "NameDe", "SortOrder" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "NotApplicable", null, "web", "Website oder Webanwendung", 1 },
                    { 2, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Open", "Eigene Hilfsmittel wie Screenreader lassen sich verwenden.", "softwareOpen", "Software (offen)", 2 },
                    { 3, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Closed", "Eigene Hilfsmittel wie Screenreader lassen sich nicht verwenden, etwa bei Kiosk-Terminals.", "softwareClosed", "Software (geschlossen)", 3 },
                    { 4, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Open", "Eigene Hilfsmittel wie Screenreader lassen sich verwenden.", "mobileOpen", "Mobile Anwendung (offen)", 4 },
                    { 5, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Closed", "Eigene Hilfsmittel wie Screenreader lassen sich nicht verwenden.", "mobileClosed", "Mobile Anwendung (geschlossen)", 5 },
                    { 6, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "NotApplicable", null, "document", "Dokument", 6 },
                    { 7, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "NotApplicable", null, "hardware", "Hardware", 7 },
                    { 8, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "NotApplicable", null, "relayEmergencyService", "Vermittlungs- oder Notrufdienst", 8 }
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "ProductParts",
                columns: new[] { "Id", "CreatedAt", "ExplanationDe", "IsAlwaysSelected", "Key", "NameDe", "SortOrder" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Basisfunktionalitäten sind alle Funktionalitäten, die erfahrungsgemäß in allen Anwendungen vorhanden sind. Der Filter „Basisfunktionalität\" ist standardmäßig immer ausgewählt.", true, "base", "Basisfunktionalität", 1 },
                    { 2, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Audiofunktionalität ist die akustische oder klangliche Wiedergabe von Informationen.", false, "audio", "Audio", 2 },
                    { 3, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Videofunktionalität ist die synchrone Wiedergabe von visuellen (sichtbaren) und optional akustischen (hörbaren) Informationen.", false, "video", "Video", 3 },
                    { 4, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Zweiwege-Sprachkommunikation ist der Austausch von Sprach- und/oder Videoinformationen in zwei Richtungen (z. B. Telefonie und Videokonferenzsysteme).", false, "twoWayVoice", "Zweiwege-Sprachkommunikation", 4 },
                    { 5, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Echtzeittext, Englisch \"real-time-text\" (RTT), bezeichnet die unmittelbare Übertragung von einzelnen schriftlichen Zeichen. Empfänger von Echtzeittext können somit den Schreibprozess direkt mitverfolgen. Echtzeittext findet vor allem in Kommunikations- und Kollaborationslösungen Anwendung, z. B. in Form einer Live-Untertitelung.", false, "rtt", "Echtzeittext (RTT)", 5 },
                    { 6, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Biometrie ist die Verwendung von biologischen Merkmalen zur Benutzeridentifikation oder Steuerung, z. B. Fingerabdruck oder Gesichtserkennung.", false, "biometrics", "Biometrie", 6 },
                    { 7, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Ein Autorenwerkzeug ist jede Möglichkeit, Text in strukturierter Form zu erstellen oder zu bearbeiten, z. B. Textverarbeitungsprogramme, White Boards, aber auch Texteingabefelder mit Strukturierungsmöglichkeiten (z. B. Kommentarfunktion).", false, "authoringTool", "Autorenwerkzeug", 7 }
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "TestTypes",
                columns: new[] { "Id", "CreatedAt", "Key", "NameDe", "SortOrder", "Url" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "softwareTest", "BIT inklusiv Software-Test", 1, "https://softwaretest.bit-inklusiv.de/" },
                    { 2, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "appTest", "BIT inklusiv App-Test", 2, "https://apptest.bit-inklusiv.de/" },
                    { 3, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "pdfTest", "BIT inklusiv PDF-Test", 3, "https://pdftest.bit-inklusiv.de/" },
                    { 4, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bikWeb", "BIK BITV-Test (Web)", 4, "https://bitvtest.de/pruefverfahren/bitv-20-web" },
                    { 5, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bikApp", "BIK BITV-Test (App)", 5, "https://bitvtest.de/pruefverfahren/bitv-20-app" }
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "UseCaseTemplates",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Key", "NameDe" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "hr", "HR-Anwendung" },
                    { 2, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "reporting", "Reporting-Anwendung" },
                    { 3, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "trainingManagement", "Fortbildungsmanagement" },
                    { 4, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "travelRequests", "Dienstreiseanträge" },
                    { 5, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "payroll", "Gehaltsabrechnungen" },
                    { 6, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "internalCommunication", "Interne Kommunikations- und Datenaustauschplattform" },
                    { 7, new DateTimeOffset(new DateTime(2026, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, false, "databaseApplication", "Datenbankgestützte Fachanwendung" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Criteria_TestTypeId",
                schema: "catalog",
                table: "Criteria",
                column: "TestTypeId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Criteria_TestTypeOnlyForProcedure",
                schema: "catalog",
                table: "Criteria",
                sql: "(\"TestTypeId\" IS NOT NULL) = (\"Kind\" = 'TestProcedure')");

            migrationBuilder.CreateIndex(
                name: "IX_AppTypes_Key",
                schema: "catalog",
                table: "AppTypes",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogReleaseAppTypeProcedures_AppTypeId",
                schema: "catalog",
                table: "CatalogReleaseAppTypeProcedures",
                column: "AppTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogReleaseAppTypeProcedures_TestTypeId",
                schema: "catalog",
                table: "CatalogReleaseAppTypeProcedures",
                column: "TestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogReleaseFilterExclusions_FilterExclusionId",
                schema: "catalog",
                table: "CatalogReleaseFilterExclusions",
                column: "FilterExclusionId");

            migrationBuilder.CreateIndex(
                name: "IX_CriterionApplicability_AppTypeId",
                schema: "catalog",
                table: "CriterionApplicability",
                column: "AppTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CriterionApplicability_CriterionRevisionId_AppTypeId_Produc~",
                schema: "catalog",
                table: "CriterionApplicability",
                columns: new[] { "CriterionRevisionId", "AppTypeId", "ProductPartId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_CriterionApplicability_ProductPartId",
                schema: "catalog",
                table: "CriterionApplicability",
                column: "ProductPartId");

            migrationBuilder.CreateIndex(
                name: "IX_CriterionProcedureTexts_CriterionRevisionId_TestTypeId",
                schema: "catalog",
                table: "CriterionProcedureTexts",
                columns: new[] { "CriterionRevisionId", "TestTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriterionProcedureTexts_TestTypeId",
                schema: "catalog",
                table: "CriterionProcedureTexts",
                column: "TestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterExclusions_SourceAppTypeId",
                schema: "catalog",
                table: "FilterExclusions",
                column: "SourceAppTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterExclusions_SourceProductPartId",
                schema: "catalog",
                table: "FilterExclusions",
                column: "SourceProductPartId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterExclusions_SourceUseCaseId",
                schema: "catalog",
                table: "FilterExclusions",
                column: "SourceUseCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterExclusions_TargetAppTypeId",
                schema: "catalog",
                table: "FilterExclusions",
                column: "TargetAppTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterExclusions_TargetProductPartId",
                schema: "catalog",
                table: "FilterExclusions",
                column: "TargetProductPartId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterExclusions_TargetTestTypeId",
                schema: "catalog",
                table: "FilterExclusions",
                column: "TargetTestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PriorityRecommendations_AppTypeId",
                schema: "catalog",
                table: "PriorityRecommendations",
                column: "AppTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PriorityRecommendations_CriterionRevisionId_AppTypeId_UseCa~",
                schema: "catalog",
                table: "PriorityRecommendations",
                columns: new[] { "CriterionRevisionId", "AppTypeId", "UseCaseTemplateId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_PriorityRecommendations_UseCaseTemplateId",
                schema: "catalog",
                table: "PriorityRecommendations",
                column: "UseCaseTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductParts_Key",
                schema: "catalog",
                table: "ProductParts",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestTypes_Key",
                schema: "catalog",
                table: "TestTypes",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UseCaseTemplates_Key",
                schema: "catalog",
                table: "UseCaseTemplates",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Criteria_TestTypes_TestTypeId",
                schema: "catalog",
                table: "Criteria",
                column: "TestTypeId",
                principalSchema: "catalog",
                principalTable: "TestTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Criteria_TestTypes_TestTypeId",
                schema: "catalog",
                table: "Criteria");

            migrationBuilder.DropTable(
                name: "CatalogReleaseAppTypeProcedures",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CatalogReleaseFilterExclusions",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CriterionApplicability",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "CriterionProcedureTexts",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "PriorityRecommendations",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "FilterExclusions",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "AppTypes",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "ProductParts",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "TestTypes",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "UseCaseTemplates",
                schema: "catalog");

            migrationBuilder.DropIndex(
                name: "IX_Criteria_TestTypeId",
                schema: "catalog",
                table: "Criteria");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Criteria_TestTypeOnlyForProcedure",
                schema: "catalog",
                table: "Criteria");

            migrationBuilder.DropColumn(
                name: "TestTypeId",
                schema: "catalog",
                table: "Criteria");
        }
    }
}
