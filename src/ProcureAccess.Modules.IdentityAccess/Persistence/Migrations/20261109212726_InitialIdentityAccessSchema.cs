using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProcureAccess.Modules.IdentityAccess.Persistence.Migrations
{
    /// <summary>
    /// First migration of the IdentityAccess context (S3-24, ARCHITECTURE ADR-014, §9.1): the
    /// <c>identity</c> schema with the Data Protection key ring <c>DataProtectionKeys</c>. Nothing
    /// moves from the legacy context: until now the keys lived in the container's file system.
    /// </summary>
    /// <remarks>Internal, like everything outside <c>Contracts</c> (ARC-03); EF finds internal migrations.</remarks>
    internal partial class InitialIdentityAccessSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtectionKeys",
                schema: "identity");
        }
    }
}
