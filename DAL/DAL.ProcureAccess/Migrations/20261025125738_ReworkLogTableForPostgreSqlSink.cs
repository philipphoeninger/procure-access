using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAL.ProcureAccess.Migrations
{
    /// <summary>
    /// Replaces the unused MSSQL-era log table with the layout of the Serilog PostgreSQL sink
    /// (ARCHITECTURE §9.5). The table is dropped and recreated instead of altered: PostgreSQL has
    /// no cast from <c>xml</c> to <c>jsonb</c>, and no code ever wrote to the old table.
    /// </summary>
    public partial class ReworkLogTableForPostgreSqlSink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeriLogs",
                schema: "logging");

            migrationBuilder.CreateTable(
                name: "SeriLogs",
                schema: "logging",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Level = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    MessageTemplate = table.Column<string>(type: "text", nullable: false),
                    Exception = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "jsonb", nullable: true),
                    SourceContext = table.Column<string>(type: "text", nullable: true),
                    ApplicationName = table.Column<string>(type: "text", nullable: true),
                    MachineName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeriLogs_TimeStamp",
                schema: "logging",
                table: "SeriLogs",
                column: "TimeStamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeriLogs",
                schema: "logging");

            migrationBuilder.CreateTable(
                name: "SeriLogs",
                schema: "logging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Message = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MessageTemplate = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Level = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Exception = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Properties = table.Column<string>(type: "Xml", maxLength: 200, nullable: false),
                    LogEvent = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceContext = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RequestPath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ActionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ApplicationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MachineName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MemberName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriLogs", x => x.Id);
                });
        }
    }
}
