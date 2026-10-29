using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAL.ProcureAccess.Migrations
{
    /// <summary>
    /// The key of <c>analytics."UsageEvents"</c> becomes a random UUID v4 instead of an identity,
    /// so the key no longer shows the order of events within a day (owner decision 2026-10-29,
    /// #124). PostgreSQL cannot cast <c>bigint</c> to <c>uuid</c>, so the column is replaced; the
    /// volatile default gives every existing row its own random key, and no other column changes.
    /// </summary>
    public partial class UsageEventRandomKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UsageEvents",
                schema: "analytics",
                table: "UsageEvents");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "analytics",
                table: "UsageEvents");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "analytics",
                table: "UsageEvents",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsageEvents",
                schema: "analytics",
                table: "UsageEvents",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UsageEvents",
                schema: "analytics",
                table: "UsageEvents");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "analytics",
                table: "UsageEvents");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                schema: "analytics",
                table: "UsageEvents",
                type: "bigint",
                nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsageEvents",
                schema: "analytics",
                table: "UsageEvents",
                column: "Id");
        }
    }
}
